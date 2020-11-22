using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.IO;

using VeeamArchiveTool.Common;
using VeeamArchiveTool.Common.Abstractions;
using VeeamArchiveTool.Providers;
using VeeamArchiveTool.Services;
using VeeamArchiveTool.Services.Abstractions;

namespace VeeamArchiveTool
{
    //compress C:\archiveTest\test2.iso C:\archiveTest\UntitledGZ.gz
    //decompress C:\archiveTest\UntitledGZ.gz C:\archiveTest\test22.iso
    class Program
    {
        static void Main(string[] args)
        {
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

                IHost host = CreateHostBuilder(args).ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                }).Build();

                using (var serviceScope = host.Services.CreateScope())
                {
                    var appRoot = serviceScope
                        .ServiceProvider
                        .GetRequiredService<AppRoot>();

                    appRoot.Run(args);
                }
            
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(((Exception)e.ExceptionObject).Message);
            Environment.Exit(1);
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            })
            .ConfigureServices((_, services) =>
            services.AddTransient<AppRoot>()
                    .AddTransient<ICommandLineConfigurationProvider, CommandLineConfigurationProvider>()
                    .AddTransient<IArchiveService, ArchiveService>()
                    .AddSingleton<IThreadPool, Common.ThreadPool>()
                    .AddSingleton<FileProcessor>()
                    .AddSingleton<IGZipProcessor, GZipProcessor>()
                    .AddSingleton<IExecutionContext>(sp =>
                    {
                        var configurationProvider = sp.GetService<ICommandLineConfigurationProvider>();
                        var configuration = configurationProvider.GetConfiguration(args);

                        var size = InfoHelper.GetChunkSize();

                        return new Common.ExecutionContext
                        {
                            ProcessDirection = configuration.ProcessDirection,
                            InputFilePath = configuration.InputFilePath,
                            OutputFilePath = configuration.OutputFilePath,
                            ChunkSize = InfoHelper.GetChunkSize()
                        };
                    })
                    .AddScoped<IProgressState, ProgressState>()
                    .AddScoped<IExceptionHandler, ConsoleExceptionHandler>()
                    .AddScoped<Lazy<IProgressState>>(provider => new Lazy<IProgressState>(() => {
                        var progressState = provider.GetService<IProgressState>();
                        progressState.Initialize();
                        return progressState;
                        })));
    }
}
