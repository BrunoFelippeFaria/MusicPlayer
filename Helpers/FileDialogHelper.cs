using Gtk;

namespace MusicPlayer.Helpers;

public static class FileDialogHelper
{
    public static string? GetDirectory()
    {
        var fileChooser = new FileChooserDialog(
            "Select a Folder",
            null,
            FileChooserAction.SelectFolder,
            "Cancel", ResponseType.Cancel,
            "Select", ResponseType.Accept
        );

        fileChooser.SetCurrentFolder(Directory.GetCurrentDirectory());

        string? path = null;

        if (fileChooser.Run() == (int)ResponseType.Accept)
        {
            path = fileChooser.Filename;
        }

        fileChooser.Destroy();

        return path;
    }
}