namespace PtlEmulator.App.Command;

public class SetColor(int clientId, byte displayId, byte color, byte blink) : BaseCommand(clientId, displayId)
{
    public byte Color { get; } = color;
    public byte Blink { get; } = blink;
}