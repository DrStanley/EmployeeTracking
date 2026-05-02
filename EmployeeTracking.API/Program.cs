using EmployeeTracking.API.Extensions;
using EmployeeTracking.API.Middleware;
using EmployeeTracking.Application.Behaviours;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Infrastructure.Identity;
using EmployeeTracking.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.MigrationsAssembly("EmployeeTracking.Infrastructure")));

// ── Identity ─────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.Password.RequireDigit = true;
    opts.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ────────────────────────────────────────────────────────
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ── MediatR + Validation Pipeline ────────────────────────────────────────────
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

// ── AutoMapper ────────────────────────────────────────────────────────────────
//builder.Services.AddAutoMapper(
//    typeof(EmployeeTracking.Application.AssemblyReference).Assembly);
builder.Services.AddAutoMapper(cfg => { },
    typeof(EmployeeTracking.Application.AssemblyReference).Assembly);


// ── Custom Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITimeEntryFactory, TimeEntryFactory>();


// ── Swagger ───────────────────────────────────────────────────────────────────
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
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
// Seed default department and attendance policy
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
}
app.Run();