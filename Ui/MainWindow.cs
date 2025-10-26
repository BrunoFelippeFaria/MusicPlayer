using Gtk;
using MusicPlayer.Helpers;
using MusicPlayer.Interfaces;
using MusicPlayer.Models;

namespace MusicPlayer.Ui;

public class MainWindow : Window
{
    public Image MusicImage { get; set; }

    public MainWindow(IMainWindowController controller) : base("MusicPlayer")
    {
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
        IEnumerable<MusicFile> musicFiles = controller.GetMusicFiles(@"C:\Users\bruno\programacao\gtk#\teste1");

        controller.LoadStore(musicFiles, store);

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
        controller.UpdateImage(MusicImage);

        // Buttons
        Button PlayBtn = new Button("Play");
        Button StopBtn = new Button("Stop");

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

        Add(grid);

        // -- configs da janela --
        SetPosition(WindowPosition.Center);
        Resize(800, 600);
        Resizable = false;
        ShowAll();

        //-- eventos --
        DeleteEvent += controller.CloseWindow;
        PlayBtn.Clicked += controller.OnPlayClicked;
        StopBtn.Clicked += controller.OnStopClicked;

        reloadFolder.Activated += (o, s) =>
        {
            musicFiles = controller.GetMusicFiles();
            controller.LoadStore(musicFiles, store);
        };

        openFolder.Activated += (o, s) =>
        {
            var path = FileDialogHelper.GetDirectory();

            if (string.IsNullOrEmpty(path)) return;

            musicFiles = controller.GetMusicFiles(path);
            controller.LoadStore(musicFiles, store);
        };

        // Altera o arquivo selecionado
        treeView.Selection.Changed += (obj, args) =>
        {
            if (treeView.Selection.GetSelected(out TreeIter iter))
            {
                var fileName = (string)treeView.Model.GetValue(iter, 0);
                var ext = (string)treeView.Model.GetValue(iter, 1);


                controller.ChangeSelectedMusic(fileName + ext);
                controller.UpdateImage(MusicImage);
            }
        };

        // Atualiza botão quando musica parar
        controller.PlaybackStoped += () =>
        {
            PlayBtn.Label = "Play";
        };
    }


}