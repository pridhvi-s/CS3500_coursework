namespace GUI.Components.Pages;

/// <summary>The three text fields of the connect menu.</summary>
public enum MenuField { Ip, Port, Name }

/// <summary>
/// Model for the connect menu drawn on the canvas: the three field values,
/// which one has focus, and the last error to show. Keyboard only.
/// </summary>
public class ConnectMenu
{
    private const int MaxLength = 40;

    public string Ip { get; set; } = "localhost";
    public string Port { get; set; } = "11000";
    public string Name { get; set; } = "player";
    public MenuField Focused { get; set; } = MenuField.Ip;
    public string? Error { get; set; }

    /// <summary>Applies a key press. Returns true only when Enter was pressed (submit).</summary>
    public bool HandleKey(string key)
    {
        if (key == "Enter") return true;
        if (key == "Tab")
        {
            Focused = (MenuField)(((int)Focused + 1) % 3);
            return false;
        }

        string text = Get(Focused);
        if (key == "Backspace")
        {
            if (text.Length > 0) Set(Focused, text[..^1]);
        }
        else if (key.Length == 1 && text.Length < MaxLength && (Focused != MenuField.Port || char.IsAsciiDigit(key[0])))
        {
            Set(Focused, text + key);
        }
        return false;
    }

    public void CopyTo(ConnectMenu other)
    {
        other.Ip = Ip;
        other.Port = Port;
        other.Name = Name;
        other.Focused = Focused;
        other.Error = Error;
    }

    private string Get(MenuField f) => f switch { MenuField.Ip => Ip, MenuField.Port => Port, _ => Name };

    private void Set(MenuField f, string v)
    {
        switch (f)
        {
            case MenuField.Ip: Ip = v; break;
            case MenuField.Port: Port = v; break;
            default: Name = v; break;
        }
    }
}
