using System;
using System.Management;

namespace VeeamArchiveTool.Services
{
    public class InfoHelper
    {
        public static int GetChunkSize()
        {
            try
            {
                string Query = "SELECT Capacity FROM Win32_PhysicalMemory";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
                ulong amount = 0;

                foreach (ManagementObject memo in searcher.Get())
                {
                    amount += Convert.ToUInt64(memo.Properties["Capacity"].Value);
                }

                int chunkSize;

                checked
                {
                    chunkSize = Convert.ToInt32(amount / 1024);
                }

                return chunkSize;
            }
            catch { }

            return 1024 * 1024 * 5;
            
        }
    }
}
