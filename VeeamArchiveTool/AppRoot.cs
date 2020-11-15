using Microsoft.Extensions.Logging;

using VeeamArchiveTool.Common.Abstractions;
using VeeamArchiveTool.Common.Models;
using VeeamArchiveTool.DomainModels;
using VeeamArchiveTool.Services.Abstractions;

namespace VeeamArchiveTool
{
    public class AppRoot
    {

        private readonly IArchiveService _archiveService;

        private readonly ILogger<AppRoot> _logger;

        public AppRoot(
            IArchiveService archiveService,
            ILogger<AppRoot> logger)
        {
            _archiveService = archiveService;
            _logger = logger;
        }

        public void Run(string[] args)
        {
            _logger.LogInformation("Archive tool application is running");

            _archiveService.Process();
        }
    }
}
