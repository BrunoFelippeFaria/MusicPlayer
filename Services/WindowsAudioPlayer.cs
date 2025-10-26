using MusicPlayer.Interfaces;
using NAudio.Wave;

public class WindowsAudioPlayer : IAudioPlayerService
{
    private WaveOutEvent? outputDevice;
    private AudioFileReader? audioFile;
    private bool _stoping;
    private System.Timers.Timer? _timer;

    public bool HasActiveTrack { get; set; }

    public bool IsSeeking { get; set; }
    private TimeSpan _currentTime;

    public TimeSpan CurrentTime
    {
        get => _currentTime;
        set
        {
            _currentTime = value;

            if (audioFile != null)
                audioFile.CurrentTime = _currentTime;

            TimeUpdated?.Invoke();
        }
    }

    public TimeSpan TotalTime { get; set; }

    public event Action? PlaybackStopped;
    public event Action? TimeUpdated;

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
            TotalTime = audioFile.TotalTime;
            HasActiveTrack = true;
        }

        outputDevice.Play();

        if (_timer == null)
        {
            _timer = new System.Timers.Timer(200); // 200ms
            _timer.Elapsed += (s, e) =>
            {
                if (audioFile != null && !IsSeeking)
                {
                    _currentTime = audioFile.CurrentTime;
                    
                    if (outputDevice.PlaybackState == PlaybackState.Playing)
                        TimeUpdated?.Invoke();                    
                }
            };
        }
        _timer.Start();
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
        _stoping = true;

        outputDevice?.Dispose();
        outputDevice = null;
        audioFile?.Dispose();
        audioFile = null;
        HasActiveTrack = false;
        PlaybackStopped?.Invoke();
    }
    
    private void OnPlaybackStopped(object? sender, StoppedEventArgs args)
    {
        if (!_stoping)
        {
            StopMusic();
        }

        CurrentTime = TimeSpan.FromSeconds(0);
        _stoping = false;
    }
}