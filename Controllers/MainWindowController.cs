using Gdk;
using Gtk;
using MusicPlayer.Interfaces;
using MusicPlayer.Models;
using TagLib;

namespace MusicPlayer.Controllers;

public class MainWindowController : IMainWindowController
{
    private readonly IAudioPlayerService _audioPlayer;
    private string[] SupportedAudioFormats = { ".mp3", ".mp4" };
    private string? SelectedMusicFile { get; set; }
    private bool IsPlaying { get; set; } = false;
    public event System.Action? PlaybackStoped;
    private string DefaultImage { get; set; }

    public MainWindowController(IAudioPlayerService audioPlayer)
    {
        _audioPlayer = audioPlayer;
        _audioPlayer.PlaybackStoped += OnMusicStop;

        DefaultImage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ui", "Images", "music-note-icon.png");
    }

    public void CloseWindow(object obj, DeleteEventArgs args)
    {
        Application.Quit();
    }

    public IEnumerable<MusicFile> GetMusicFiles(string directory)
    {
        string[] files = Directory.GetFiles(directory);

        List<MusicFile> musicFiles = [];

        foreach (string file in files)
        {
            string ext = Path.GetExtension(file);

            if (SupportedAudioFormats.Contains(ext))
            {
                musicFiles.Add(new MusicFile(file));
            }
        }

        if (musicFiles.Count > 0)
        {
            MusicFile musicFile = musicFiles.First();
            SelectedMusicFile = musicFile.File;
        }

        else
        {
            SelectedMusicFile = null;
        }

        Directory.SetCurrentDirectory(directory);
        return musicFiles;
    }

    public IEnumerable<MusicFile> GetMusicFiles()
    {
        string path = Directory.GetCurrentDirectory();
        return GetMusicFiles(path);
    }

    public void LoadStore(IEnumerable<MusicFile> files, ListStore store)
    {
        store.Clear();

        foreach (var file in files)
        {
            store.AppendValues(file.FileName, file.Extension, file.FileSize);
        }
    }

    public void ChangeSelectedMusic(string file)
    {
        SelectedMusicFile = file;
    }

    public void OnPlayClicked(object? obj, EventArgs args)
    {
        if (string.IsNullOrEmpty(SelectedMusicFile) || !System.IO.File.Exists(SelectedMusicFile)) return;

        if (obj == null) return;

        Button button = (obj as Button)!;

        if (!IsPlaying)
        {
            IsPlaying = true;
            _audioPlayer.PlayMusic(SelectedMusicFile);
            button.Label = "Pause";
        }

        else
        {
            IsPlaying = false;
            _audioPlayer.PauseMusic();
            button.Label = "Play";
        }
    }

    public void UpdateImage(Image image)
    {
        if (!System.IO.File.Exists(SelectedMusicFile)) return;

        try
        {
            var file = TagLib.File.Create(SelectedMusicFile);

            if (file.Tag.Pictures.Length > 0)
            {
                byte[] bin = file.Tag.Pictures[0].Data.Data;

                var loader = new PixbufLoader();
                loader.Write(bin);
                loader.Close();
                Pixbuf pixbuf = loader.Pixbuf;

                image.Pixbuf = pixbuf;
            }

            else
            {
                image.Pixbuf = new Pixbuf(DefaultImage);
            }
        }

        catch (CorruptFileException)
        {            
            image.Pixbuf = new Pixbuf(DefaultImage);
        }
    }

    public void OnStopClicked(object? obj, EventArgs args)
    {
        _audioPlayer.StopMusic();
    }

    private void OnMusicStop()
    {
        IsPlaying = false;
        PlaybackStoped?.Invoke();
    }
}