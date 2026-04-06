using Cronos;

namespace API.Services.Scheduling;

internal static class CronScheduleUtc
{
    /// <summary>
    /// Indica si el cron está programado para el minuto UTC actual (alineado al inicio del minuto).
    /// </summary>
    public static bool IsDueThisUtcMinute(string cronExpression, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
            return false;

        if (!CronExpression.TryParse(cronExpression.Trim(), CronFormat.Standard, out var expr))
            return false;

        if (utcNow.Kind != DateTimeKind.Utc)
            utcNow = utcNow.ToUniversalTime();

        var minuteStart = DateTime.SpecifyKind(
            new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 0),
            DateTimeKind.Utc);

        var fromInclusive = minuteStart.AddSeconds(-1);
        var next = expr.GetNextOccurrence(fromInclusive, inclusive: true);
        return next.HasValue && next.Value == minuteStart;
    }
}
