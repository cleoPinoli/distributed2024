using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Orleans.Hosting;
using Orleans;
using StackExchange.Redis;



namespace MicrosoftOrleansWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Website: Waiting");


            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Host.UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
                siloBuilder.UseDashboard();
                // Configura lo storage in memoria per i Grain
                siloBuilder.AddMemoryGrainStorageAsDefault();
                // Opzionale: Configura lo storage in memoria per la PubSubStore
                siloBuilder.AddMemoryGrainStorage("PubSubStore");

                siloBuilder.ConfigureServices(services =>
                {
                    // Configura la connessione Redis come singleton
                    services.AddSingleton(ConnectionMultiplexer.Connect("localhost:6379"));
                });
            });

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();



           
        }
    }
}
