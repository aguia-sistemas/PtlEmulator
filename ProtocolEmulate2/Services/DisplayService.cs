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
        public Dictionary<int, Dictionary<int, DisplayWidget>> DisplayDictionary { get; private set; }

        public event Action<int, string, string, string>? SendMessageEvent;

        public DisplayService()
        {
            DisplayDictionary = new Dictionary<int, Dictionary<int, DisplayWidget>>();
        }

        public void CreatePtlGrid(int clientId)
        {
            if (!DisplayDictionary.ContainsKey(clientId))
            {
                DisplayDictionary.Add(clientId, new Dictionary<int, DisplayWidget>());

                for (int i = 1; i < 121; i++)
                {
                    var displayWidget = new DisplayWidget
                    {
                        ConfirmLabel = "____",
                        ButtonConfirm = new Button { Label = "____", Color = "blink" }
                    };
                    DisplayDictionary[clientId].Add(i, displayWidget);
                }
            }
        }

        public Action<BaseCommand> OnClientConnected(int clientId)
        {
            CreatePtlGrid(clientId);
            return OnReceiveCommand;
        }

        private void OnReceiveCommand(BaseCommand command)
        {
            var displayWidget = DisplayDictionary[command.ClientId][command.DisplayId];

            if (command is TurnOn turnOn)
            {
                displayWidget.ButtonConfirm.Label = turnOn.Value;
            }

            if (command is TurnOff)
            {
                displayWidget.ButtonConfirm.Label = "____";
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