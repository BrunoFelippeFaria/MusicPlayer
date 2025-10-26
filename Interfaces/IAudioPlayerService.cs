namespace MusicPlayer.Interfaces;

public interface IAudioPlayerService
{
    public bool IsSeeking { get; set; }
    public bool HasActiveTrack { get; set; }
    public TimeSpan TotalTime { get; set; }
    public TimeSpan CurrentTime { get; set; }
    public void PlayMusic(string file);
    public void PauseMusic();
    public void StopMusic();
    public event Action? PlaybackStopped;
    public event Action? TimeUpdated;

}