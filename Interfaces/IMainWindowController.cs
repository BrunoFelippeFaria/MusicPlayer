using Gtk;
using MusicPlayer.Models;

namespace MusicPlayer.Interfaces;

public interface IMainWindowController
{
    // Atributos
    public TimeSpan TotalTime { get; }
    public TimeSpan CurrentTime { get; set;  }
    public bool IsPlaying { get; }
    public string? SelectedMusicFile { get; }
    public string DefaultImage { get; }
    public bool HasActiveTrack { get; }
    public ICollection<MusicFile> MusicFiles { get; }
    public bool IsSeeking { get; set; }

    // Eventos
    public event System.Action? TimeUpdated;
    public event System.Action? PlaybackStopped;

    // Métodos principais
    public void PlayMusic(string? music);
    public void StopMusic();
    public void ChangeSelectedMusic(string file);
    public byte[]? GetAlbumArt();
    public void ChangeCurrentDirectory(string path);

}