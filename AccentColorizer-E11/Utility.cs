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
        public static string ColorToHex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static void TakeOwnership(string filepath)
        {
            try
            {
                GetFullAccess(filepath);
            } 
            catch
            {
                ExecuteCommand("takeown.exe", $"/F \"{filepath}\"");
                ExecuteCommand("icacls.exe", $"\"{filepath}\" /grant {Environment.UserName}:F");
            }
        }

        public static void NormalizeOwnership(string filepath)
        {
            ExecuteCommand("icacls.exe", $"\"{filepath}\" /setowner \"NT SERVICE\\TrustedInstaller\"", false);
            ExecuteCommand("icacls.exe", $"\"{filepath}\" /remove {Environment.UserName}:F", false);
            ExecuteCommand("icacls.exe", $"\"{filepath}\" /t /q /c /reset", false);
        }

        private static void GetFullAccess(string filepath)
        {
            var security = File.GetAccessControl(filepath);
            SecurityIdentifier user = WindowsIdentity.GetCurrent().User;
            security.SetOwner(user);
            security.SetAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl, AccessControlType.Allow));
            File.SetAccessControl(filepath, security);
        }

        public static void ExecuteCommand(string exec, string args, bool wait = true)
        {
            var proc = new Process();
            proc.StartInfo.FileName = exec;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            if (wait)
            {
                proc.WaitForExit();
            }
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
    }
}
