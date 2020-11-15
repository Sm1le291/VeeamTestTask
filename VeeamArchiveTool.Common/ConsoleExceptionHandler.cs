using System;

using VeeamArchiveTool.Common.Abstractions;

namespace VeeamArchiveTool.Common
{
    public class ConsoleExceptionHandler : IExceptionHandler
    {
        public bool HandleException(Exception ex)
        {
            switch (ex)
            {
                case FileAlreadyExistsException:
                    {
                        Console.WriteLine("Rewrite output file (y/n):");
                        string result = Console.ReadLine();
                         return result == "y" ? true : false;
                    };

                    default: return false; 
            }
        }
    }
}
