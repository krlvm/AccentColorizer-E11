using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace AccentColorizer_E11
{
    class Utility
    {
        private const string GROUP_USERS_SID = "S-1-5-11";

        public static void TakeOwnership(string filepath)
        {
            ExecuteCommand("takeown.exe", $"/R /F \"{filepath}\"");
            ExecuteCommand("icacls.exe", $"\"{filepath}\" /grant *{GROUP_USERS_SID}:F /T");
        }

        public static string ColorToHex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static string ReadFile(string filepath)
        {
            return File.ReadAllText(filepath);
        }

        public static void WriteFile(string filepath, string text)
        {
            var sw = new StreamWriter(filepath);
            sw.Write(text);
            sw.Close();
        }

        public static void ExecuteCommand(string exec, string args)
        {
            System.Console.WriteLine(exec + " " + args);
            var proc = new Process();
            proc.StartInfo.FileName = exec;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            proc.WaitForExit();
        }

        public static void RunElevated(string args)
        {
            Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName)
            {
                UseShellExecute = true,
                Verb = "runas",
                Arguments = args
            });
        }
    }
}
