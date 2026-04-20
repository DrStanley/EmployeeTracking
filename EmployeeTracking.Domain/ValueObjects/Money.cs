using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.ValueObjects
{
    public sealed record Money(decimal Amount, string Currency = "USD")
    {
        public static Money Zero(string currency = "USD") => new(0m, currency);

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new DomainException("Cannot add amounts in different currencies.");
            return new(Amount + other.Amount, Currency);
        }
    }
}
