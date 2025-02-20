namespace ProtocolEmulate2.Models;

public class ProtocolRequest
{
    public string Protocol { get; set; }
    public string ScannerValue { get; set; }
    
    public string ScannerEst { get; set; } = "PTW";
}