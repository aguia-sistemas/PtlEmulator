namespace PtlEmulator.App.Command;

public class BaseCommand(int clientId, byte displayId)
{
    public int ClientId { get; set; } = clientId;
    public byte DisplayId { get; } = displayId;
}