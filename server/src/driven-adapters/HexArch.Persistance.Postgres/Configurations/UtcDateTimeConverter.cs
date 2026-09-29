using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HexArch.Persistance.Postgres.Configurations
{
    /// <summary>
    /// The users schema stores UTC wall-clock time in <c>timestamp</c> (without time zone) columns - the
    /// same convention its SQL functions use (<c>now() AT TIME ZONE 'utc'</c>). Npgsql refuses to write a
    /// UTC DateTime to such a column, and reads them back with an unspecified Kind, so this strips the
    /// Kind on the way in and restores it as UTC on the way out.
    /// </summary>
    internal sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                value => DateTime.SpecifyKind(value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value, DateTimeKind.Unspecified),
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
        {
        }
    }
}
