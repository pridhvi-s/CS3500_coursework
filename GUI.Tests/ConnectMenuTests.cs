using GUI.Components.Pages;

namespace GUI.Tests;

public class ConnectMenuTests
{
    private static void Type(ConnectMenu m, string s)
    {
        foreach (char c in s) m.HandleKey(c.ToString());
    }

    [Test]
    public void Tab_CyclesFieldsAndWraps()
    {
        var m = new ConnectMenu();
        Assert.That(m.Focused, Is.EqualTo(MenuField.Ip));
        m.HandleKey("Tab");
        Assert.That(m.Focused, Is.EqualTo(MenuField.Port));
        m.HandleKey("Tab");
        Assert.That(m.Focused, Is.EqualTo(MenuField.Name));
        m.HandleKey("Tab");
        Assert.That(m.Focused, Is.EqualTo(MenuField.Ip));
    }

    [Test]
    public void Typing_AppendsToFocusedField()
    {
        var m = new ConnectMenu();
        for (int i = 0; i < 9; i++) m.HandleKey("Backspace");
        Type(m, "ab");
        Assert.That(m.Ip, Is.EqualTo("ab"));
    }

    [Test]
    public void Port_RejectsNonDigits()
    {
        var m = new ConnectMenu();
        m.HandleKey("Tab");
        Type(m, "x1");
        Assert.That(m.Port, Is.EqualTo("110001"));
    }

    [Test]
    public void Backspace_OnEmptyField_DoesNothing()
    {
        var m = new ConnectMenu();
        for (int i = 0; i < 20; i++) m.HandleKey("Backspace");
        Assert.That(m.Ip, Is.EqualTo(""));
    }

    [Test]
    public void Enter_ReturnsTrue_OthersFalse()
    {
        var m = new ConnectMenu();
        Assert.That(m.HandleKey("Enter"), Is.True);
        Assert.That(m.HandleKey("a"), Is.False);
        Assert.That(m.HandleKey("Tab"), Is.False);
    }

    [Test]
    public void ArrowKeysAndShift_Ignored()
    {
        var m = new ConnectMenu();
        m.HandleKey("ArrowUp");
        m.HandleKey("Shift");
        Assert.That((m.Ip, m.Port, m.Name), Is.EqualTo(("localhost", "11000", "player")));
    }

    [Test]
    public void Field_CapsAtFortyChars()
    {
        var m = new ConnectMenu();
        Type(m, new string('z', 100));
        Assert.That(m.Ip.Length, Is.EqualTo(40));
    }
}
