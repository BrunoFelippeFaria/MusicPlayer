using MusicPlayer.Interfaces;
using MusicPlayer.Models;

namespace MusicPlayer.Controllers;

public class MainWindowController : IMainWindowController
{
    private readonly IAudioPlayerService _audioPlayer;
    private readonly ISettingsService _settings;

    // Campos Privados
    private readonly string[] _supportedAudioFormats = [".mp3", ".mp4"];
    private string? _currentSong;
    private bool _stopClicked;
    
    private string? _selectedMusicFile;
    private bool _isPlaying;
    private ICollection<MusicFile> _musicFiles = [];
    private readonly string _defaultImage;

    // Campos publicos
    public string? SelectedMusicFile => _selectedMusicFile;
    public bool IsPlaying => _isPlaying;
    public TimeSpan TotalTime => _audioPlayer.TotalTime;
    public TimeSpan CurrentTime
    {
        get => _audioPlayer.CurrentTime;
        set => _audioPlayer.CurrentTime = value;
    } 
    public bool HasActiveTrack => _audioPlayer.HasActiveTrack;
    public ICollection<MusicFile> MusicFiles => _musicFiles;
    public string DefaultImage => _defaultImage;
    
    public bool IsSeeking
    {
        get => _audioPlayer.IsSeeking;
        set => _audioPlayer.IsSeeking = value;
    }


    // Eventos
    public event Action? TimeUpdated;
    public event Action? PlaybackStopped;



    public MainWindowController(IAudioPlayerService audioPlayer, ISettingsService settings)
    {
        _audioPlayer = audioPlayer;
        _audioPlayer.PlaybackStopped += OnMusicStop;
        _audioPlayer.TimeUpdated += OnTimeUpdated;

        _settings = settings;

        _defaultImage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ui", "Images", "music-note-icon.png");
        _stopClicked = false;

        _musicFiles = GetMusicFiles().ToList();
    }

    private IEnumerable<MusicFile> GetMusicFiles(string directory)
    {
        string[] files = Directory.GetFiles(directory);

        List<MusicFile> musicFiles = [];

        foreach (string file in files)
        {
            string ext = Path.GetExtension(file);

            if (_supportedAudioFormats.Contains(ext))
            {
                musicFiles.Add(new MusicFile(file));
            }
        }

        if (musicFiles.Count > 0)
        {
            MusicFile musicFile = musicFiles.First();
            _selectedMusicFile = musicFile.File;
        }

        else
        {
            _selectedMusicFile = null;
        }

        Directory.SetCurrentDirectory(directory);
        return musicFiles;
    }

    private IEnumerable<MusicFile> GetMusicFiles()
    {
        string path = @"C:\Users\bruno\programacao\gtk#\teste1"; // Directory.GetCurrentDirectory();
        return _musicFiles = GetMusicFiles(path).ToList();
    }

    public void ChangeCurrentDirectory(string path)
    {
        Directory.SetCurrentDirectory(path);
        _musicFiles = GetMusicFiles(Directory.GetCurrentDirectory()).ToList();
    }

    public void ChangeSelectedMusic(string file)
    {
        _selectedMusicFile = file;
    }

    public void PlayMusic(string? music)
    {
        if (string.IsNullOrEmpty(music) || !File.Exists(music)) return;

        if (!IsPlaying)
        {
            _isPlaying = true;
            _audioPlayer.PlayMusic(music);
        }

        else
        {
            _isPlaying = false;
            _audioPlayer.PauseMusic();
        }

        _currentSong = _selectedMusicFile;
    }

    public byte[]? GetAlbumArt()
    {
        if (!File.Exists(_selectedMusicFile))
            return null;

        try
        {
            var file = TagLib.File.Create(_selectedMusicFile);
            if (file.Tag.Pictures.Length > 0)
            {
                return file.Tag.Pictures[0].Data.Data;
            }
        }

        catch
        {
            Console.WriteLine("Erro ao carregar imagem");
        }
        
        return null;
    }

    public void StopMusic()
    {
        _stopClicked = true;
        _audioPlayer.StopMusic();
    }

    public void OnTimeUpdated()
    {
        TimeUpdated?.Invoke();
    }

    private void OnMusicStop()
    {        
        _isPlaying = false;

        if (!_stopClicked && _settings.SessionSettings.Repeat)
        {
            PlayMusic(_currentSong);
            _isPlaying = true;
        }

        _stopClicked = false;

        PlaybackStopped?.Invoke();
    }
}