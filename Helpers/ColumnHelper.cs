using Gtk;

namespace MusicPlayer.Helpers;

public static class ColumnHelper
{
    public static TreeViewColumn CreateTextColumn(string title, int index)
    {
        var column = new TreeViewColumn { Title = title };
        var cell = new CellRendererText();
        column.PackStart(cell, true);
        column.AddAttribute(cell, "text", index);

        return column;
    }
}