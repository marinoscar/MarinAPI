using Serilog;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.SystemConsole.Themes;

namespace API.Config
{
    public static class LoggingExtensions
    {
        public static void ConfigureSerilogLogging(this IHostBuilder hostBuilder)
        {
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password";
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "logsdb";

            var connectionString = $"Host={dbServer};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

            var columnWriters = new Dictionary<string, ColumnWriterBase>
            {
                { "timestamp", new TimestampColumnWriter() },
                { "level", new LevelColumnWriter() },
                { "message", new RenderedMessageColumnWriter() },
                { "message_template", new MessageTemplateColumnWriter() },
                { "exception", new ExceptionColumnWriter() },
                { "properties", new LogEventSerializedColumnWriter() },
                { "log_event", new LogEventSerializedColumnWriter() }
            };

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .WriteTo.PostgreSQL(
                    connectionString: connectionString,
                    tableName: "logs",
                    columnOptions: columnWriters,
                    needAutoCreateTable: false
                )
                .CreateLogger();

            hostBuilder.UseSerilog();
        }
    }
}
