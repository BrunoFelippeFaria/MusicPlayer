using Gtk;
using MusicPlayer.Models;

namespace MusicPlayer.Interfaces;

public interface IMainWindowController
{
    public event System.Action? PlaybackStoped;
    public void CloseWindow(object obj, DeleteEventArgs args);
    public IEnumerable<MusicFile> GetMusicFiles(string directory);
    public IEnumerable<MusicFile> GetMusicFiles();
    public void LoadStore(IEnumerable<MusicFile> files, ListStore store);
    public void ChangeSelectedMusic(string file);
    public void OnPlayClicked(object? obj, EventArgs args);
    public void OnStopClicked(object? obj, EventArgs args);
}