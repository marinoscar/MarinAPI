namespace API.Config
{
    public class DbHelper
    {
        public static string GetConnectionString()
        {
            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password";
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "logging";
            return $"Host={dbServer};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
        }
    }
}
