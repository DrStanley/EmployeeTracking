using EmployeeTracking.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Application.Commands.Auth
{
    public record RefreshTokenCommand(
     string AccessToken,
     string RefreshToken
 ) : IRequest<AuthResponse>;
}
