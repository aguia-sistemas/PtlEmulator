namespace PtlEmulator.App.Command;

public class TurnOn(int clientId, byte displayId, string value) : BaseCommand(clientId, displayId)
{
    public string Value { get; } = value;
}