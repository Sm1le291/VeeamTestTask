using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Linq;

using VeeamArchiveTool.Common.Abstractions;
using VeeamArchiveTool.Common.Models;

namespace VeeamArchiveTool.Providers
{
    public class CommandLineConfigurationProvider : ICommandLineConfigurationProvider
    {
        private readonly ILogger<CommandLineConfigurationProvider> _logger;

        public CommandLineConfigurationProvider(ILogger<CommandLineConfigurationProvider> logger)
        {
            _logger = logger;
        }

        public AppConfiguration GetConfiguration(string[] args)
        {
            if (args.Length != 3)
            {
                var message = "Wrong income arguments";
                _logger.LogError(message);

                throw new ArgumentException(message);
            }

            if (!(new string[] { "compress", "decompress" }).Contains(args[0]))
            {
                var message = "First argument must be in { \"compress\", \"decompress\"} range";
                _logger.LogError(message);

                throw new ArgumentException(message);
            }

            var mode = args[0];

            if (!File.Exists(args[1]))
            {
                var message = "Input file does not exist";
                _logger.LogError(message);

                throw new ArgumentException(message);
            }

            if (mode == "decompress" && Path.GetExtension(args[1]) != ".gz")
            {
                var message = "You can decompress only .gz archives";
                _logger.LogError(message);

                throw new ArgumentException(message);
            }

            string outputDirectory = Path.GetDirectoryName(args[2]);

            if (!Directory.Exists(outputDirectory))
            {
                var message = $"Output directory {outputDirectory} does not exist";
                _logger.LogError(message);

                throw new ArgumentException(message);
            }

            var appConfiguration =  new AppConfiguration
            {
                ProcessDirection = args[0] == "compress" ? ProcessDirection.Compress : ProcessDirection.Decompress,
                InputFilePath = args[1],
                OutputFilePath = args[2]
            };

            _logger.LogInformation("Parameters accepted");
            _logger.LogInformation(appConfiguration.ToString());

            return appConfiguration;
        }
    }
}
