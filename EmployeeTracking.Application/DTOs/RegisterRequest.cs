namespace EmployeeTracking.Application.DTOs
{
    public record RegisterRequest(
     string FirstName,
     string LastName,
     string Email,
     string Password,
     string Role         // "Employee" | "Manager" | "Admin"
 );
}
