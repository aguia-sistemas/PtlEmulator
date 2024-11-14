namespace ProtocolEmulate.Models;

public class Client
{
    public Client(int i, List<DisplayWidget> displayWidgets)
    {
        ClientId = i;
        Displays = displayWidgets;
    }

    public int ClientId { get; set; }
    public List<DisplayWidget> Displays { get; set; }
}