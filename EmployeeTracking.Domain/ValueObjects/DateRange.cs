using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.ValueObjects
{
    public sealed record DateRange
    {
        public DateOnly Start { get; }
        public DateOnly End { get; }

        public DateRange(DateOnly start, DateOnly end)
        {
            if (end < start)
                throw new DomainException("End date must be on or after start date.");
            Start = start;
            End = end;
        }

        public int TotalDays => End.DayNumber - Start.DayNumber + 1;
        public bool Overlaps(DateRange other) => Start <= other.End && End >= other.Start;
        public bool Contains(DateOnly date) => date >= Start && date <= End;
    }
}
