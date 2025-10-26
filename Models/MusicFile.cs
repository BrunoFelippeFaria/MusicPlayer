namespace MusicPlayer.Models;

public class MusicFile
{
    public string File { get; set; }
    public string FileName { get; set; }
    public string FileSize { get; set; }
    public string Extension { get; set; }

    public MusicFile(string file)
    {
        File = file;
        FileName = Path.GetFileNameWithoutExtension(file);
        FileSize = GetFormatedFileSize(file);
        Extension = Path.GetExtension(file);
    }

    private static string GetFormatedFileSize(string path)
    {
        var file = new FileInfo(path);
        double size = file.Length;
        string[] units = { "B", "KB", "MB", "GB" };
        int i = 0;

        while (size >= 1024 && i < units.Length - 1)
        {
            size /= 1024;
            i++;
        }

        return $"{size:F2} {units[i]}";
    }
}