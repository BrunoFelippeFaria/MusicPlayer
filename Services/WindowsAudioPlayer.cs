using System.Runtime.InteropServices;
using System.Text;
using Gdk;
using NAudio.Wave;

public class WindowsAudioPlayer : IAudioPlayerService
{
    private WaveOutEvent? outputDevice;
    private AudioFileReader? audioFile;
    public event Action? PlaybackStoped;


    public void PlayMusic(string file)
    {
        if (outputDevice == null)
        {
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OnPlaybackStopped;
        }

        if (audioFile == null)
        {
            audioFile = new AudioFileReader(file);
            outputDevice.Init(audioFile);
        }

        outputDevice.Play();
    }

    public void PauseMusic()
    {
        if (outputDevice != null)
        {
            outputDevice.Pause();
        }
    }

    public void StopMusic()
    {
        outputDevice?.Dispose();
        outputDevice = null;
        audioFile?.Dispose();
        audioFile = null;

        PlaybackStoped?.Invoke();
    }
    
    private void OnPlaybackStopped(object? sender, StoppedEventArgs args)
    {
        StopMusic();
    }
}