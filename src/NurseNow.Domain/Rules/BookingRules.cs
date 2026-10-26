namespace NurseNow.Domain.Rules;
public static class BookingRules
{
    public static bool Overlaps(TimeSpan start, TimeSpan end, TimeSpan otherStart, TimeSpan otherEnd) => start < otherEnd && end > otherStart;
}
