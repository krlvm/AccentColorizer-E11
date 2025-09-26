using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
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

        public static void TakeRegistryOwnership(RegistryKey key)
        {
            var accessControl = key.GetAccessControl();
            var identity = new SecurityIdentifier(GROUP_USERS_SID);

            accessControl.AddAccessRule(new RegistryAccessRule(
                identity,
                RegistryRights.FullControl,
                AccessControlType.Allow)
            );

            key.SetAccessControl(accessControl);
        }

        public static string ColorToHex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static string BlendColors(string color1, string color2, int intensity)
        {
            int color1_R = Convert.ToInt32($"{color1[1]}{color1[2]}", 16);
            int color1_G = Convert.ToInt32($"{color1[3]}{color1[4]}", 16);
            int color1_B = Convert.ToInt32($"{color1[5]}{color1[6]}", 16);

            int color2_R = Convert.ToInt32($"{color2[1]}{color2[2]}", 16);
            int color2_G = Convert.ToInt32($"{color2[3]}{color2[4]}", 16);
            int color2_B = Convert.ToInt32($"{color2[5]}{color2[6]}", 16);

            int R = (int)Math.Round((((color1_R * intensity)) + (color2_R * (255 - intensity))) / 255.0);
            int G = (int)Math.Round((((color1_G * intensity)) + (color2_G * (255 - intensity))) / 255.0);
            int B = (int)Math.Round((((color1_B * intensity)) + (color2_B * (255 - intensity))) / 255.0);

            return $"#{R:X2}{G:X2}{B:X2}";
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
