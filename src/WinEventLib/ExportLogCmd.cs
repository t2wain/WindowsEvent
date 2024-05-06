using System;
using System.Diagnostics;

namespace WinEventLib
{
    public class ExportLogCmd
    {
        /// <summary>
        /// Run the command "webtutil" to export 
        /// Windows event to a file .evtx
        /// </summary>
        /// <param name="logName">Typically the Application Windows event log</param>
        /// <param name="query">An XPath filter for the Windows event</param>
        public static void ExportLog(string logName, string query, string fileName)
        {
            var cmd = "wevtutil";
            var args = $"epl {logName} \"{fileName}\" /q:\"{query}\" /ow:true";
            var res = CaptureConsoleAppOutput(cmd, args,
                Convert.ToInt32(TimeSpan.FromMinutes(1).TotalMilliseconds));
            if (res.ExitCode != 0)
                throw new Exception(res.Message);
        }


        /// <summary>
        /// A utility method to run an external program
        /// </summary>
        public static (int ExitCode, string Message) CaptureConsoleAppOutput(string exeName, string arguments, int timeoutMilliseconds)
        {
            using var process = new Process();

            process.StartInfo.FileName = exeName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();

                var output = process.StandardOutput.ReadToEnd();

                var exited = process.WaitForExit(timeoutMilliseconds);
                var exitCode = 0;
                if (exited)
                {
                    exitCode = process.ExitCode;
                }
                else
                {
                    exitCode = -1;
                }

                return (exitCode, output);
            }
            catch (Exception e)
            {
                return (-1, e.Message.Substring(0, 25));
            }


        }

    }
}
