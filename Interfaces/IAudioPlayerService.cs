namespace MusicPlayer.Interfaces;

public interface IAudioPlayerService
{
    public void PlayMusic(string file);
    public void PauseMusic();
    public void StopMusic();
    public event Action? PlaybackStoped;

}