namespace BlankoCRM.Application.Services;

/// <summary>
/// Вспомогательный статический форматтер времени.
/// </summary>
public static class DateTimeFormatter
{
    /// <summary>
    /// Форматирует UTC-время в локальное время мастера по его IANA-таймзоне.
    /// При некорректной таймзоне возвращает время в UTC.
    /// </summary>
    public static string FormatInTimeZone(DateTime utcDateTime, string ianaTimeZone)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZone);
            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            var local = TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
            return local.ToString("dd.MM.yyyy HH:mm");
        }
        catch (Exception)
        {
            return utcDateTime.ToString("dd.MM.yyyy HH:mm") + " UTC";
        }
    }
}
