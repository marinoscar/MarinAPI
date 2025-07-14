using Serilog;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.SystemConsole.Themes;

namespace API.Config
{
    public static class LoggingExtensions
    {
        public static void ConfigureSerilogLogging(this IHostBuilder hostBuilder)
        {
            var dbTableName = Environment.GetEnvironmentVariable("DB_TABLENAME") ?? "serilogs";

            var connectionString = DbHelper.GetConnectionString();

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
                    tableName: dbTableName,
                    columnOptions: columnWriters,
                    needAutoCreateTable: false, respectCase: false
                )
                .MinimumLevel.Debug()
                .CreateLogger();

            hostBuilder.UseSerilog();
        }
    }
}
