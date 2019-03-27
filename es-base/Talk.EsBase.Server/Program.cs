using System;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static System.Environment;
using Serilog;

namespace Talk.EsBase.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SetupThreadPool();
            var configuration = BuildConfiguration(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .MinimumLevel.Debug()
                .CreateLogger();

            ConfigureWebHost(configuration).Build().Run();
        }

        static IConfiguration BuildConfiguration(string[] args)
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

        static IWebHostBuilder ConfigureWebHost(
            IConfiguration configuration)
            => new WebHostBuilder()
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .UseContentRoot(CurrentDirectory)
                .UseSerilog()
                .UseKestrel();

        static void SetupThreadPool()
        {
            ThreadPool.GetMaxThreads(out var workerThreads, out var completionPortThreads);
            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            workerThreads = Math.Max(workerThreads, 100);
            completionPortThreads = Math.Max(completionPortThreads, 400);
            ThreadPool.SetMinThreads(workerThreads, completionPortThreads);

            ServicePointManager.DefaultConnectionLimit = 400;
            ServicePointManager.UseNagleAlgorithm = false;
        }
    }
}