using Squirrel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SmartCardReader
{
    public static class Extensions
    {
        public static async Task VerifyUpdates()
        {
            try
            {                
                //using (var updateManager = new UpdateManager(@"https://github.com/majvr93/SmartCardReader/releases/latest"))
                using (var updateManager = new UpdateManager(@"C:\SquirrelReleases"))
                {
                    Console.WriteLine($"Current version: {updateManager.CurrentlyInstalledVersion()}");
                    var releaseEntry = await updateManager.UpdateApp();
                    Console.WriteLine($"Update Version: {releaseEntry?.Version.ToString() ?? "No update"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update check error...");
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        public static void KillIOldRunningProcesses()
        {
            try
            {
                if (Process.GetProcessesByName("SmartCardReader").Length > 1)
                {
                    var proccessList = Process.GetProcessesByName("SmartCardReader")
                        .OrderBy(x => x.StartTime)
                        .ToList();
                    Console.WriteLine(proccessList.Count() + $" processes found! ");

                    proccessList.Remove(proccessList.Last());
                    foreach (var process in proccessList)
                    {
                        process.Kill();
                        Console.WriteLine($"KILL process id = " + process.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing old process.");
            }
        }
    }
}
