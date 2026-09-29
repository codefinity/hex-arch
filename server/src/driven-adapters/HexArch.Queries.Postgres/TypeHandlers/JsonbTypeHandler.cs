using System.Data;
using System.Text.Json;
using Dapper;

namespace HexArch.Queries.Postgres.TypeHandlers
{
    /// <summary>
    /// Maps a jsonb column to/from a CLR type. Npgsql surfaces jsonb as a plain string,
    /// so Dapper needs this handler rather than its default type mapping.
    /// </summary>
    internal sealed class JsonbTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public override T? Parse(object value) =>
            value is string json ? JsonSerializer.Deserialize<T>(json, Options) : default;

        public override void SetValue(IDbDataParameter parameter, T? value) =>
            parameter.Value = JsonSerializer.Serialize(value, Options);
    }
}
