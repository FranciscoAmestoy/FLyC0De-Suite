using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FLyC0De.Core.Interception;

namespace FLyC0De.Core.Actions
{
    /// <summary>
    /// Launches an application or runs a script.
    /// Supports .exe, .vbs, .bat, .cmd, .ps1, and any other executable.
    /// </summary>
    public class RunApplicationAction : ActionBase
    {
        public override string TypeId => "run_app";
        public override string DisplayName => "Run Application";

        /// <summary>
        /// Path to the executable or script.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Command line arguments.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Working directory. If empty, uses the file's directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Whether to run hidden (no visible window).
        /// </summary>
        public bool RunHidden { get; set; }

        /// <summary>
        /// Whether to wait for the process to exit.
        /// </summary>
        public bool WaitForExit { get; set; }

        public override async Task ExecuteAsync(KeyboardEventArgs keyEvent)
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return;

            var startInfo = new ProcessStartInfo
            {
                FileName = FilePath,
                Arguments = Arguments ?? "",
                UseShellExecute = true,
            };

            // Set working directory
            if (!string.IsNullOrWhiteSpace(WorkingDirectory))
            {
                startInfo.WorkingDirectory = WorkingDirectory;
            }
            else if (File.Exists(FilePath))
            {
                startInfo.WorkingDirectory = Path.GetDirectoryName(FilePath);
            }

            // Handle hidden execution
            if (RunHidden)
            {
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
            }

            try
            {
                using (var process = Process.Start(startInfo))
                {
                    if (WaitForExit && process != null)
                    {
                        await Task.Run(() => process.WaitForExit());
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                Debug.WriteLine($"Failed to run {FilePath}: {ex.Message}");
            }
        }

        public override bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                error = "File path is required";
                return false;
            }

            if (!File.Exists(FilePath))
            {
                error = $"File not found: {FilePath}";
                return false;
            }

            error = null;
            return true;
        }
    }
}
