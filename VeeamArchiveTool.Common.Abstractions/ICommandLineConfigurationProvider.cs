using System;

using VeeamArchiveTool.Common.Models;

namespace VeeamArchiveTool.Common.Abstractions
{
    public interface ICommandLineConfigurationProvider
    {
        AppConfiguration GetConfiguration(string[] args);
    }
}
