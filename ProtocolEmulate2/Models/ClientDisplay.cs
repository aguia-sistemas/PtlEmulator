using ProtocolEmulate.Models;

namespace ProtocolEmulate2.Models;

public class ClientDisplay
{
    public int ClientId { get; set; }
    public List<DisplayWidget> DisplayWidgets { get; set; }

    public ClientDisplay(int clientId)
    {
        ClientId = clientId;
        DisplayWidgets = new List<DisplayWidget>();
    }
}