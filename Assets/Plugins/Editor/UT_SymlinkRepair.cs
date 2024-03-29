using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Plugins.Editor
{
    using SymlinkList = List<(string path, string target)>;
     
    [InitializeOnLoad]
    public class UT_SymlinkRepair
    {
        // Thanks to this script being located in Plugins directory, it'll have chance
        // to be assembled before anything else and fix broken/non-existing symlinks.
        static UT_SymlinkRepair()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // execute repair tool only once at the beginning of the session.
            if (!SessionState.GetBool("UTSymlinkRepairToolInitialized", false))
            {
                FindAndRepairSymlinks();
                SessionState.SetBool("UTSymlinkRepairToolInitialized", true);
            }
        }

        [MenuItem("UT System/Repair Symlinks")]
        // attempts to repair broken submodules symlinks in Plugins directory
        static void FindAndRepairSymlinks()
        {
            string submodulesPath = Application.dataPath.Replace("/Assets", $"/Submodules");
            string pluginsPath = Application.dataPath + "/Plugins";

            SymlinkList symlinks = new();

            foreach (var module in Directory.GetDirectories(submodulesPath))
            {
                string symlinkPath = $"{pluginsPath}\\{Path.GetFileName(module)}";
                // no symbolic link has ever been created for this submodule
                if (!File.Exists(symlinkPath))
                {
                    continue;
                }

                // retrieve symlink target
                string symlinkTarget = File.ReadAllText(symlinkPath).Replace('/', '\\');

                Debug.Log($"Found broken symlink \"{symlinkPath}\"->\"{symlinkTarget}\"");

                // store symlink to create
                symlinks.Add((symlinkPath, symlinkTarget));
            }

            if (symlinks.Count == 0)
            {
                Debug.Log($"No broken symlinks have been found.");
                return;
            }

            Debug.Log($"Found {symlinks.Count} broken symlinks. Attempting to fix them.");
#if UNITY_EDITOR_WIN
            RepairSymlinksWindows(symlinks);
#else
            Debug.LogWarning("Symlink repair tool has not been implemented for this platform.");
#endif
        }

#if UNITY_EDITOR_WIN
        // windows-exclusive method of creating symlinks.
        private static void RepairSymlinksWindows(SymlinkList symlinks)
        {
            bool result = AlertQuestion(
                "Unity - UT Symlink Repair", 
                $"Broken symlinks detected: {symlinks.Count}\n" +
                "Would you like to repair them?"
            );

            if (!result)
            {
                return;
            }

            // creating a list of commands to execute through CMD.
            string cmds = "";
            foreach (var symlink in symlinks)
            {
                if (cmds.Length > 0) cmds += " & ";
                // delete file so symlink can be created
                // (done as a command to prevent deletion when no permissions for symlink have been given)
                cmds += $"del \"{symlink.path}\" & ";
                // create symlink
                cmds += $"mklink /D \"{symlink.path}\" \"{symlink.target}\"";
            }
            //Console.WriteLine(cmds);
            ExecuteInCmdWithAdminPrivileges(cmds);
        }

        private static void ExecuteInCmdWithAdminPrivileges(string cmds)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C {cmds}";

            // request admin privileges
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;

            process.StartInfo = startInfo;

            try
            {
                process.Start();
                process.WaitForExit();
            } 
            catch (Exception)
            {
                AlertInfo(
                    "Unity - UT Symlink Repair",
                    "Unable to start command line with admin privileges!\n" + 
                    "Try again by using \"UT System/Repair Symlinks\""
                );
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int MessageBox(IntPtr hwnd, String lpText, String lpCaption, uint uType);

        private static bool AlertQuestion(string title, string text)
        {
            int result = MessageBox(GetActiveWindow(), text, title, (uint)(0x00000004L | 0x00000030L));
            return result == 6;
        }

        private static void AlertInfo(string title, string text)
        {
            MessageBox(GetActiveWindow(), text, title, (uint)(0x00000000L | 0x00000030L));
        }
#endif

    }
}
