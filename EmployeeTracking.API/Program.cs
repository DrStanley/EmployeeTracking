using EmployeeTracking.API.Extensions;
using EmployeeTracking.API.Middleware;
using EmployeeTracking.Application.Behaviours;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Infrastructure.BackgroundJobs;
using EmployeeTracking.Infrastructure.Identity;
using EmployeeTracking.Infrastructure.Persistence;
using EmployeeTracking.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Resend;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Database
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.MigrationsAssembly("EmployeeTracking.Infrastructure")));

// ── Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.Password.RequireDigit = true;
    opts.Password.RequiredLength = 8;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireNonAlphanumeric = true;

    // Lock account after 5 failed attempts for 5 minutes
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    opts.Lockout.MaxFailedAccessAttempts = 5;
    opts.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication 
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opts =>
{
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtKey)),
        // Prevent clock skew from rejecting valid tokens
        ClockSkew = TimeSpan.Zero
    };

    opts.Events = new JwtBearerEvents
    {
        OnChallenge = async ctx =>
        {
            ctx.HandleResponse();

            // Log the exact reason so you can see it in the console
            var reason = ctx.AuthenticateFailure?.Message ?? "Unknown";
            Console.WriteLine($"[JWT 401] {reason}");

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = string.IsNullOrWhiteSpace(ctx.ErrorDescription)
                    ? "A valid JWT token is required to access this endpoint."
                    : ctx.ErrorDescription
            };

            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/problem+json";

            await ctx.Response.WriteAsJsonAsync(problem);
        },

        OnForbidden = async ctx =>
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Access denied",
                Detail = "You do not have permission to access this endpoint."
            };

            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Response.ContentType = "application/problem+json";

            await ctx.Response.WriteAsJsonAsync(problem);
        },

        // Add this — logs exactly why token validation failed
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine($"[JWT Auth Failed] {ctx.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddRateLimiter(opts =>
{
    // Strict limit on auth endpoints — 5 requests per minute per IP
    opts.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // General API limit — 100 requests per minute per IP
    opts.AddFixedWindowLimiter("api", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 2;
    });

    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── MediatR + Validation Pipeline 
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(EmployeeTracking.Application.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(
        typeof(EmployeeTracking.Infrastructure.InfrastructureAssemblyReference).Assembly);
});


builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
builder.Services.AddValidatorsFromAssembly(
    typeof(EmployeeTracking.Application.AssemblyReference).Assembly);
// CORS
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins")
                    .Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// AutoMapper
//builder.Services.AddAutoMapper(
//    typeof(EmployeeTracking.Application.AssemblyReference).Assembly);
builder.Services.AddAutoMapper(cfg => { },
    typeof(EmployeeTracking.Application.AssemblyReference).Assembly);


// ── Custom Services ───
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITimeEntryFactory, TimeEntryFactory>();
builder.Services.AddScoped<ITimesheetCalculationService, TimesheetCalculationService>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ForbiddenResponseHandler>();
builder.Services.AddScoped<IOvertimeStrategy, StandardOvertimeStrategy>();

// Notification service
builder.Services.AddScoped<INotificationService, NotificationService>();

// Resend email client
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(opts =>
    opts.ApiToken = builder.Configuration["Resend:ApiKey"]!);
builder.Services.AddTransient<IResend, ResendClient>();

// Email services
builder.Services.AddScoped<IEmailService, ResendEmailService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// Background jobs
builder.Services.AddHostedService<MissedPunchJob>();
builder.Services.AddHostedService<PTOAccrualJob>();
builder.Services.AddHostedService<OvertimeAlertJob>();
builder.Services.AddHostedService<TokenCleanupJob>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Tracking API",
        Version = "v1",
        Description = "Enterprise employee time tracking — clock-in/out, timesheets, PTO, and payroll."
    });

    // Include XML comments from API project
    var apiXml = Path.Combine(AppContext.BaseDirectory,
        "EmployeeTracking.API.xml");
    if (File.Exists(apiXml))
        c.IncludeXmlComments(apiXml);

    // Include XML comments from Application project (DTOs)
    var appXml = Path.Combine(AppContext.BaseDirectory,
        "EmployeeTracking.Application.xml");
    if (File.Exists(appXml))
        c.IncludeXmlComments(appXml);

    // Show enum names + values instead of raw numbers
    c.SchemaFilter<EnumSchemaFilter>();

    // JWT Bearer auth button
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseRateLimiter();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//Seed data on startup (optional)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in new[] { "Employee", "Manager", "Admin" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}
// Seed default department and attendance policy to be removed in production
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Departments.Any())
    {
        db.Departments.Add(Department.Create("General", "Default department"));
        await db.SaveChangesAsync();
    }

    if (!db.AttendancePolicies.Any())
    {
        db.AttendancePolicies.Add(AttendancePolicy.CreateDefault("Standard Policy"));
        await db.SaveChangesAsync();
    }
    if (!db.PayPeriods.Any())
    {
        var now = DateOnly.FromDateTime(DateTime.Now);
        var start = new DateOnly(now.Year, now.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        db.PayPeriods.Add(PayPeriod.Create(
            $"{start:MMMM yyyy}", start, end));

        await db.SaveChangesAsync();
    }
    if (!db.PTOBalances.Any())
    {
        var employees = db.Employees.ToList();
        foreach (var emp in employees)
        {
            var balance = PTOBalance.CreateForYear(emp.Id, DateTime.Now.Year);
            balance.Accrue(80m); // seed with 80 hours (2 weeks)
            db.PTOBalances.Add(balance);
        }
        await db.SaveChangesAsync();
    }
}
app.Run();