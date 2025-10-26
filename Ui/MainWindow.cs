using System.Collections;
using Gtk;
using MusicPlayer.Helpers;
using MusicPlayer.Interfaces;
using MusicPlayer.Models;

namespace MusicPlayer.Ui;

public class MainWindow : Window
{
    public Image MusicImage { get; set; }

    private readonly IMainWindowController _controller;
    private readonly ISettingsService _settings;

    private CheckButton RepeatCb;
    private CheckButton AutoPlayCb;

    public MainWindow(IMainWindowController controller, ISettingsService settings) : base("MusicPlayer")
    {
        _controller = controller;
        _settings = settings;

        // -- Componentes --

        // MenuBar
        MenuBar menuBar = new MenuBar();
        Menu fileMenu = new Menu();
        MenuItem fileItem = new MenuItem("_File") { Submenu = fileMenu };

        MenuItem openFolder = new MenuItem("Open Folder");
        MenuItem reloadFolder = new MenuItem("Reload Folder");

        fileMenu.Add(openFolder);
        fileMenu.Add(reloadFolder);

        menuBar.Add(fileItem);

        // TreeView
        var scrolled = new ScrolledWindow();
        scrolled.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scrolled.SetSizeRequest(500, -1);

        ListStore store = new ListStore(typeof(string), typeof(string), typeof(string));
        IEnumerable<MusicFile> musicFiles = _controller.GetMusicFiles(@"C:\Users\bruno\programacao\gtk#\teste1");

        _controller.LoadStore(musicFiles, store);

        var treeView = new TreeView(store);

        var columnName = ColumnHelper.CreateTextColumn("Name", 0);
        var columnType = ColumnHelper.CreateTextColumn("Type", 1);
        var columnSize = ColumnHelper.CreateTextColumn("Size", 2);

        treeView.AppendColumn(columnName);
        treeView.AppendColumn(columnType);
        treeView.AppendColumn(columnSize);

        scrolled.Add(treeView);

        // Image
        MusicImage = new Image();
        _controller.UpdateImage(MusicImage);

        // Buttons
        Button PlayBtn = new Button("Play");
        Button StopBtn = new Button("Stop");

        // checkbox

        RepeatCb = new CheckButton("Repeat");
        AutoPlayCb = new CheckButton("AutoPlay");

        // Layout
        var grid = new Grid();
        grid.RowSpacing = 5;
        grid.ColumnSpacing = 5;

        grid.Attach(menuBar, 0, 0, 2, 1);
        grid.Attach(scrolled, 0, 1, 1, 2);

        grid.Attach(MusicImage, 1, 1, 1, 1);

        var buttonBox = new Box(Orientation.Horizontal, 5);
        buttonBox.PackStart(PlayBtn, false, false, 0);
        buttonBox.PackStart(StopBtn, false, false, 0);
        grid.Attach(buttonBox, 1, 2, 1, 1);

        var checksbox = new Box(Orientation.Vertical, 2);
        checksbox.PackStart(RepeatCb, false, false, 0);
        checksbox.PackStart(AutoPlayCb, false, false, 0);
        grid.Attach(checksbox, 1, 3, 1, 1);

        Add(grid);

        // -- configs da janela --
        SetPosition(WindowPosition.Center);
        Resize(800, 600);
        Resizable = false;

        LoadSettings();

        ShowAll();

        //-- eventos --
        DeleteEvent += _controller.CloseWindow;
        PlayBtn.Clicked += _controller.OnPlayClicked;
        StopBtn.Clicked += _controller.OnStopClicked;

        reloadFolder.Activated += (o, s) =>
        {
            musicFiles = _controller.GetMusicFiles();
            _controller.LoadStore(musicFiles, store);
        };

        openFolder.Activated += (o, s) =>
        {
            var path = FileDialogHelper.GetDirectory();

            if (string.IsNullOrEmpty(path)) return;

            musicFiles = _controller.GetMusicFiles(path);
            _controller.LoadStore(musicFiles, store);
        };

        // Altera o arquivo selecionado
        treeView.Selection.Changed += (obj, args) =>
        {
            if (treeView.Selection.GetSelected(out TreeIter iter))
            {
                var fileName = (string)treeView.Model.GetValue(iter, 0);
                var ext = (string)treeView.Model.GetValue(iter, 1);


                _controller.ChangeSelectedMusic(fileName + ext);
                _controller.UpdateImage(MusicImage);
            }
        };

        // Atualiza botão quando musica parar
        _controller.PlaybackStoped += () =>
        {
            PlayBtn.Label = "Play";
        };

        // Checkboxes
        RepeatCb.Toggled += OnCheckToggled;
        AutoPlayCb.Toggled += OnCheckToggled;
    }

    private void OnCheckToggled(object? obj, EventArgs args)
    {
        var cb = obj as CheckButton;

        if (cb == RepeatCb)
        {
            _settings.SessionSettings.Repeat = cb.Active;
            if (cb.Active && AutoPlayCb.Active)
                AutoPlayCb.Active = false;
        }

        else if (cb == AutoPlayCb)
        {
            _settings.SessionSettings.AutoPlay = cb.Active;
            
            if (cb.Active && RepeatCb.Active)
                RepeatCb.Active = false;
        }
    }

    private void LoadSettings()
    {
        var sessionSettings = _settings.SessionSettings;
        RepeatCb.Active = sessionSettings.Repeat;
        AutoPlayCb.Active = sessionSettings.AutoPlay;
    }

}