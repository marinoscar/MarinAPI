
using API.Config;
using API.Middleware;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
             
            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();

            // Add swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerWithApiKey();

            //adds logging
            builder.Host.ConfigureSerilogLogging();

            var app = builder.Build();

            //add support to custom logging on the middleware
            app.UseMiddleware<ObservabilityMiddleware>();


            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.MapOpenApi();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // Enable Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }
}
