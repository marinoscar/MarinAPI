using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCCR.TipoDeCambio
{
    public static class DbHelper
    {
        private const string DbName = "marinapi";

        public static string CreateConnectionString(string databaseName)
        {
            string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string? user = Environment.GetEnvironmentVariable("DB_USER");
            string? server = Environment.GetEnvironmentVariable("DB_SERVER");
            string? port = Environment.GetEnvironmentVariable("DB_PORT");

            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(server) ||
                string.IsNullOrWhiteSpace(port) ||
                string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("One or more required environment variables are missing or invalid.");
            }

            return $"Host={server};Port={port};Username={user};Password={password};Database={databaseName};";
        }

        public static string CreateConnectionString() => CreateConnectionString(DbName);

        public static async Task<int> UpsertExchangeRateAsync(IEnumerable<ExchangeRecord> records)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));

            const string upsertSql = @"
        INSERT INTO ""ExchangeRates"" (""Code"", ""TypeId"", ""Type"", ""Name"", ""Value"", ""Date"")
        VALUES (@Code, @TypeId, @Type, @Name, @Value, @Date)
        ON CONFLICT (""Code"", ""TypeId"", ""Date"") DO UPDATE
        SET ""Type"" = EXCLUDED.""Type"",
            ""Name"" = EXCLUDED.""Name"",
            ""Value"" = EXCLUDED.""Value"";";

            int affectedRows = 0;

            try
            {
                await using var conn = new NpgsqlConnection(CreateConnectionString());
                await conn.OpenAsync();

                foreach (var record in records)
                {
                    await using var cmd = new NpgsqlCommand(upsertSql, conn);
                    cmd.Parameters.AddWithValue("Code", record.Code);
                    cmd.Parameters.AddWithValue("TypeId", record.TypeId);
                    cmd.Parameters.AddWithValue("Type", record.Type);
                    cmd.Parameters.AddWithValue("Name", (object?)record.Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("Value", record.Value);
                    cmd.Parameters.AddWithValue("Date", record.Date);

                    affectedRows += await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] Upsert operation failed: {ex.Message}");
                throw;
            }

            return affectedRows;
        }

        public static async Task<IEnumerable<ExchangeRecord>> GetExchangeRateAsync(DateTime? startDate, DateTime? endDate)
        {
            DateTime from = startDate ?? DateTime.Today;
            DateTime to = endDate ?? DateTime.Today;

            if (from.Year < 1970) from = DateTime.Today;
            if (to.Year < 1970) to = DateTime.Today;

            if (from > to)
                throw new ArgumentException("Start date cannot be later than end date.");

            const string query = @"
        SELECT ""Code"", ""TypeId"", ""Type"", ""Name"", ""Value"", ""Date""
        FROM ""ExchangeRates""
        WHERE ""Date"" BETWEEN @StartDate AND @EndDate
        ORDER BY ""Date"", ""Code"", ""TypeId"";";

            var results = new List<ExchangeRecord>();

            try
            {
                await using var conn = new NpgsqlConnection(CreateConnectionString());
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", from);
                cmd.Parameters.AddWithValue("@EndDate", to);

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    int code = reader.GetInt32(0);
                    int typeId = reader.GetInt32(1);
                    string type = reader.GetString(2);
                    string? name = reader.IsDBNull(3) ? null : reader.GetString(3);
                    double value = reader.GetDouble(4);
                    DateTime date = reader.GetDateTime(5);

                    results.Add(new ExchangeRecord(code, typeId, type, name, value, date));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] Query failed: {ex.Message}");
                throw;
            }

            return results;
        }
    }
}
