namespace WebApp
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public static class Program
    {
        public static readonly string AppName = typeof(Program).Namespace.Replace("Energybase.ServicePortal.", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(".Presentation.Web", string.Empty, StringComparison.OrdinalIgnoreCase);
        public static IConfiguration Configuration;

        public static int Main(string[] args)
        {
            //Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));
            Configuration = GetConfiguration();
            Log.Logger = CreateLogger(Configuration);

            try
            {
                Log.Information("starting web host (service={ServiceName})...", AppName);
                CreateHostBuilder(args).Build().Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "program terminated unexpectedly (service={ServiceName})!", AppName);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .UseSerilog()
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseConfiguration(Configuration);
                   webBuilder.UseStartup<Startup>();
               });

        private static ILogger CreateLogger(IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ServiceName", AppName)
                .Enrich.FromLogContext()
                .WriteTo.Trace()
                .WriteTo.Console()
                .WriteTo.Seq(string.IsNullOrWhiteSpace(configuration["Serilog:SeqServerUrl"]) ? "http://localhost:5340" /*"http://seq"*/ : configuration["Serilog:SeqServerUrl"])
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            //var config = builder.Build();
            //if (config.GetValue("AzureAppConfiguration:Enabled", false))
            //{
            //    builder.AddAzureAppConfiguration(config["AzureAppConfiguration:ConnectionString"]);
            //}

            //if (config.GetValue("AzureKeyVault:Enabled", false))
            //{
            //    builder.AddAzureKeyVault(
            //        $"https://{config["AzureKeyVault:Name"]}.vault.azure.net/",
            //        config["AzureKeyVault:ClientId"],
            //        config["AzureKeyVault:ClientSecret"]);
            //}

            return builder.Build();
        }
    }
}
