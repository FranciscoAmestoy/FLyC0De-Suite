using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using FLyC0De.Core.Interception;

namespace FLyC0De.Core.Actions
{
    /// <summary>
    /// Plays an audio file.
    /// </summary>
    public class PlaySoundAction : ActionBase
    {
        public override string TypeId => "play_sound";
        public override string DisplayName => "Play Sound";

        /// <summary>
        /// Path to the audio file (.wav, .mp3, etc).
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Volume (0.0 to 1.0).
        /// </summary>
        public float Volume { get; set; } = 1.0f;

        /// <summary>
        /// Whether to wait for the sound to finish.
        /// </summary>
        public bool WaitForCompletion { get; set; }

        /// <summary>
        /// Audio device number (-1 for default).
        /// </summary>
        public int DeviceNumber { get; set; } = -1;

        public override async Task ExecuteAsync(KeyboardEventArgs keyEvent)
        {
            if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
                return;

            try
            {
                await Task.Run(() =>
                {
                    // If not waiting, we fire and forget BUT we must keep the device alive
                    // So we cannot just exit the using block.
                    // Actually, since ActionEngine calls this in Task.Run, we are already on a background thread.
                    // The issue is that ExecuteAsync returns, but we want the sound to keep playing.
                    // BUT: ActionEngine awaits ExecuteAsync. So if we return, the Task.Run in ActionEngine completes.
                    // That's fine, ActionEngine doesn't dispose anything.
                    // The issue is the 'using' block inside THIS task.
                    
                    // Logic:
                    // 1. Initialize device
                    // 2. Play
                    // 3. Wait for playback to stop (always!)
                    // 4. Dispose
                    
                    // If !WaitForCompletion, we should spawn a NEW task that does the above, 
                    // so ExecuteAsync can return immediately.
                    
                    if (WaitForCompletion)
                    {
                        PlayAndWait(FilePath, Volume, DeviceNumber);
                    }
                    else
                    {
                         Task.Run(() => PlayAndWait(FilePath, Volume, DeviceNumber));
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play sound: {ex.Message}");
            }
        }

        private void PlayAndWait(string path, float vol, int deviceId)
        {
            try 
            {
                using (var audioFile = new AudioFileReader(path))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.DeviceNumber = deviceId;
                    audioFile.Volume = Math.Max(0, Math.Min(1, vol));
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Background playback error: {ex.Message}");
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
