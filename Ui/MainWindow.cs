using Gdk;
using Gtk;
using MusicPlayer.Helpers;
using MusicPlayer.Interfaces;
using MusicPlayer.Models;

namespace MusicPlayer.Ui;

public class MainWindow : Gtk.Window
{
    private readonly IMainWindowController _controller;
    private readonly ISettingsService _settings;

    #region Ui_Componets
    // Image
    private Image _musicImage;

    // Scale
    private Scale _seekBar;

    // Buttons
    private Button _playBtn;
    private Button _stopBtn;

    // Checkboxes
    private CheckButton _repeatCb;
    private CheckButton _autoPlayCb;

    //treeview
    private TreeView _treeView;
    private ListStore _store;

    //menu itens
    MenuItem _openFolder;
    MenuItem _reloadFolder;

    #endregion

    public MainWindow(IMainWindowController controller, ISettingsService settings) : base("MusicPlayer")
    {
        _controller = controller;
        _settings = settings;

        #region Load_ui
        // MenuBar
        MenuBar menuBar = new MenuBar();
        Menu fileMenu = new Menu();
        MenuItem fileItem = new MenuItem("_File") { Submenu = fileMenu };

        _openFolder = new MenuItem("Open Folder");
        _reloadFolder = new MenuItem("Reload Folder");

        fileMenu.Add(_openFolder);
        fileMenu.Add(_reloadFolder);

        menuBar.Add(fileItem);

        // TreeView
        var scrolled = new ScrolledWindow();
        scrolled.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scrolled.SetSizeRequest(500, -1);

        _store = new ListStore(typeof(string), typeof(string), typeof(string));
        LoadStore(_store);

        _treeView = new TreeView(_store);

        var columnName = ColumnHelper.CreateTextColumn("Name", 0);
        var columnType = ColumnHelper.CreateTextColumn("Type", 1);
        var columnSize = ColumnHelper.CreateTextColumn("Size", 2);

        _treeView.AppendColumn(columnName);
        _treeView.AppendColumn(columnType);
        _treeView.AppendColumn(columnSize);

        scrolled.Add(_treeView);

        // Image
        _musicImage = new Image();
        UpdateImage();

        // SeekBar
        _seekBar = LoadSeekbar();

        // Button
        _playBtn = new Button("Play");
        _stopBtn = new Button("Stop");

        // checkbox
        _repeatCb = new CheckButton("Repeat");
        _autoPlayCb = new CheckButton("AutoPlay");

        // Layout
        var grid = new Grid();
        grid.RowSpacing = 5;
        grid.ColumnSpacing = 5;

        grid.Attach(menuBar, 0, 0, 2, 1);
        grid.Attach(scrolled, 0, 1, 1, 2);

        grid.Attach(_musicImage, 1, 1, 1, 1);

        // Adiciona ao grid acima dos botões
        grid.Attach(_seekBar, 1, 2, 1, 1);

        var buttonBox = new Box(Orientation.Horizontal, 5);
        buttonBox.PackStart(_playBtn, false, false, 0);
        buttonBox.PackStart(_stopBtn, false, false, 0);
        grid.Attach(buttonBox, 1, 3, 1, 1);

        var checksbox = new Box(Orientation.Vertical, 2);
        checksbox.PackStart(_repeatCb, false, false, 0);
        checksbox.PackStart(_autoPlayCb, false, false, 0);
        grid.Attach(checksbox, 1, 4, 1, 1);

        Add(grid);
        #endregion

        #region Window Config

        SetPosition(WindowPosition.Center);
        Resize(800, 600);
        Resizable = false;

        LoadSettings();

        ShowAll();
        #endregion

        AttachEvents();
    }

    public void AttachEvents()
    {
        //-- eventos --
        DeleteEvent += CloseWindow;
        _playBtn.Clicked += OnPlayClicked;
        _stopBtn.Clicked += OnStopClicked;

        _controller.TimeUpdated += UpdateSeekBar;

        //seek bar
        _seekBar.ButtonPressEvent += (o, e) =>
        {
            _controller.IsSeeking = false;
        };

        // Para de buscar quando o usuário solta
        _seekBar.ButtonReleaseEvent += (o, e) =>
        {
            _controller.IsSeeking = true;

            // Atualiza o player para o novo tempo final
            var seekBar = o as Scale;
            if (seekBar != null)
                _controller.CurrentTime = TimeSpan.FromSeconds(seekBar.Value);
        };

        // Atualiza continuamente enquanto o usuário arrasta
        _seekBar.ValueChanged += (o, e) =>
        {
            if (!_controller.IsSeeking)
            {
                var seekBar = o as Scale;
                if (seekBar != null)
                    _controller.CurrentTime = TimeSpan.FromSeconds(seekBar.Value);
            }
        };

        _reloadFolder.Activated += (o, s) =>
        {
            LoadStore(_store);
        };

        _openFolder.Activated += (o, s) =>
        {
            var path = FileDialogHelper.GetDirectory();

            if (string.IsNullOrEmpty(path)) return;

            _controller.ChangeCurrentDirectory(path);
            LoadStore(_store);
        };

        // Altera o arquivo selecionado
        _treeView.Selection.Changed += (obj, args) =>
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var fileName = (string)_treeView.Model.GetValue(iter, 0);
                var ext = (string)_treeView.Model.GetValue(iter, 1);

                _controller.ChangeSelectedMusic(fileName + ext);
                UpdateImage();
            }
        };

        // Atualiza botão quando musica parar
        _controller.PlaybackStopped += () => 
        {
            _playBtn.Label = "Play";
        };

        // Checkboxes
        _repeatCb.Toggled += OnCheckToggled;
        _autoPlayCb.Toggled += OnCheckToggled;
    }

    private Scale LoadSeekbar()
    {
        var seekBar = new Scale(
            Orientation.Horizontal, 
            new Adjustment(0, 0, 100, 0.2, 1, 0) // pageSize = 0
        );
        seekBar.DrawValue = false;

        return seekBar;
    }

    private void UpdateSeekBar()
    {
        if (!_controller.IsSeeking)
            _seekBar.Value = _controller.CurrentTime.TotalSeconds;
    }

    private void OnStopClicked(object? obj, EventArgs args)
    {
        _controller.StopMusic();
    }

    private void OnCheckToggled(object? obj, EventArgs args)
    {
        var cb = obj as CheckButton;

        if (cb == _repeatCb)
        {
            _settings.SessionSettings.Repeat = cb.Active;
            if (cb.Active && _autoPlayCb.Active)
                _autoPlayCb.Active = false;
        }

        else if (cb == _autoPlayCb)
        {
            _settings.SessionSettings.AutoPlay = cb.Active;

            if (cb.Active && _repeatCb.Active)
                _repeatCb.Active = false;
        }
    }

    private void LoadSettings()
    {
        var sessionSettings = _settings.SessionSettings;
        _repeatCb.Active = sessionSettings.Repeat;
        _autoPlayCb.Active = sessionSettings.AutoPlay;
    }

    private void OnPlayClicked(object? obj, EventArgs args)
    {
        bool newAudio = false;

        
        if (!_controller.HasActiveTrack)
        {
            _seekBar.Value = 0;
            newAudio = true;
        }

        if (_controller.IsPlaying)
        {
            _playBtn.Label = "Play";
        }

        else
        {
            _playBtn.Label = "Pause";
        }

        _controller.PlayMusic(_controller.SelectedMusicFile);
    
        if (newAudio)
        {
            _seekBar.Adjustment.Upper = _controller.TotalTime.TotalSeconds;
        }
    }

    private void LoadStore(ListStore store)
    {
        store.Clear();

        var files = _controller.MusicFiles;

        foreach (var file in files)
        {
            store.AppendValues(file.FileName, file.Extension, file.FileSize);
        }
    }

    public void UpdateImage()
    {
        byte[]? artData = _controller.GetAlbumArt();

        if (artData != null)
        {
            var loader = new PixbufLoader();
            loader.Write(artData);
            loader.Close();
            _musicImage.Pixbuf = loader.Pixbuf;
        }

        else
        {
            _musicImage.Pixbuf = new Pixbuf(_controller.DefaultImage);
        }
    }

    public void CloseWindow(object obj, DeleteEventArgs args)
    {
        Application.Quit();
    }

}