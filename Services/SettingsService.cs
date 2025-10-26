public class SettingsService : ISettingsService
{
    public SessionSettings SessionSettings { get; set; }

    public SettingsService ()
    {
        // Inicia nova sessão
        SessionSettings = new()
        {
            AutoPlay = false,
            Repeat = false
        };
    }
}