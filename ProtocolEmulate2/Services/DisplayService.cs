// DisplayService.cs

using System.Text;
using ProtocolEmulate.Models;
using ProtocolEmulate2.Models;
using PtlEmulator.App.Command;
using RabbitMQ.Client;

namespace ProtocolEmulate2.Services
{
    public class DisplayService
    {
        public List<ClientDisplay> ClientDisplays { get; private set; }

        public event Action<int, string, string, string>? SendMessageEvent;

        public DisplayService()
        {
            ClientDisplays = new List<ClientDisplay>();
        }

        public void CreatePtlGrid(int clientId)
        {
            if (!ClientDisplays.Any(cd => cd.ClientId == clientId))
            {
                var clientDisplay = new ClientDisplay(clientId);

                for (int i = 1; i <= 120; i++)
                {
                    var displayWidget = new DisplayWidget
                    {
                        ConfirmLabel = "_",
                        ButtonConfirm = new Button { Label = "_", Color = "blink" }
                    };
                    clientDisplay.DisplayWidgets.Add(displayWidget);
                }

                ClientDisplays.Add(clientDisplay);
            }
        }

        public Action<BaseCommand> OnClientConnected(int clientId)
        {
            CreatePtlGrid(clientId);
            return OnReceiveCommand;
        }

        private void OnReceiveCommand(BaseCommand command)
        {
            var clientDisplay = ClientDisplays.First(cd => cd.ClientId == command.ClientId);
            var displayWidget = clientDisplay.DisplayWidgets[command.DisplayId - 1];

            if (command is TurnOn turnOn)
            {
                displayWidget.ButtonConfirm.Label = turnOn.Value;
            }

            if (command is TurnOff)
            {
                displayWidget.ButtonConfirm.Label = "_";
                displayWidget.ButtonConfirm.Color = "";
            }

            if (command is SetColor setColor)
            {
                switch (setColor.Color)
                {
                    case 0x00:
                        displayWidget.ButtonConfirm.Color = "red";
                        break;
                    case 0x01:
                        displayWidget.ButtonConfirm.Color = "green";
                        break;
                    case 0x02:
                        displayWidget.ButtonConfirm.Color = "orange";
                        break;
                    case 0x03:
                        displayWidget.ButtonConfirm.Color = "blue";
                        break;
                    case 0x04:
                        displayWidget.ButtonConfirm.Color = "pink";
                        break;
                    case 0x05:
                        displayWidget.ButtonConfirm.Color = "cyan";
                        break;
                }

                if (setColor.Blink != 0x00)
                    displayWidget.ButtonConfirm.Color = "blink";
            }
        }

        public void SendMessageToClient(int clientId, string messageType, string device, string value)
        {
            SendMessageEvent?.Invoke(clientId, messageType, device, value);
        }
    }
}