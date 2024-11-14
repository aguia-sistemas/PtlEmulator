// AtopClient.cs
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using PtlEmulator.App.Command;
using Array = System.Array;

namespace PtlEmulator.App.Socket;

public class AtopClient : BaseTcpClient
{
    const byte STX = 0x02;
    const byte ETX = 0x03;
    
    private byte[] _receiveBuffer = new byte[0];
    private byte[] _sendBuffer = new byte[0];
    
    public AtopClient(TcpClient client, int clientId, CancellationTokenSource tokenSource, Func<int, Action<BaseCommand>>? clientConnectedEvent, TaskFactory taskFactory) : base(client, clientId, tokenSource, clientConnectedEvent, taskFactory)
    {            
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    }

    protected override bool ProcessReceiveData(byte[] buffer)
    {
        var received = false;
        try
        {
            if (buffer is { Length: > 0 })
            {
                var tempBuffer = new byte[_receiveBuffer.Length + buffer.Length];
                _receiveBuffer.CopyTo(tempBuffer, 0);
                buffer.CopyTo(tempBuffer, _receiveBuffer.Length);
                _receiveBuffer = tempBuffer;
            }

            if (_receiveBuffer.Length > 0)
            {
                received = true;
                // o tamanho da mensagem
                byte messageSize = _receiveBuffer[0];
                // pega x bytes do buffer
                var message = _receiveBuffer[..messageSize];
                // remove a mensagem do buffer
                _receiveBuffer = _receiveBuffer[(messageSize)..];
                
                ProcessReceivedMessage(message);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"ProcessReceiveData: ClientId {ClientId}. Error: {e}");
        }
        
        return received;
    }

    private void ProcessReceivedMessage(byte[] message)
    {
        // converte a mensagem para string em hexadecimal
        string hexMessage = BitConverter.ToString(message);
        Debug.WriteLine($"ProcessReceiveData: ClientId {ClientId}. Message: {hexMessage}");

        try
        {
            // minimo de 8 bytes
            if (message.Length >= 7)
            {
                // o tamanho da mensagem
                byte messageSize = message[0];
                // pega o comando
                byte subCommand = message[6];
                
                // node id
                byte? displayId = null;
                if(message.Length >= 8)
                    displayId = message[7];
                
                // pega a mensagem
                byte[] messageData = Array.Empty<byte>();
                if(message.Length >= 9)
                    messageData = message[8..];
                
                BaseCommand? command = null;
                // para cada tipo de comando, monta um objeto correspondente
                switch (subCommand)
                {
                    case 0x00: // turn on
                        command = new TurnOn(ClientId, displayId.Value, Encoding.Default.GetString(messageData[..^1]));
                        break;
                    case 0x01: // turn of
                        command = new TurnOff(ClientId, displayId.Value);
                        break;
                    case 0x1f: // set color
                        byte blink = messageData[0];
                        byte color = messageData[1];
                        command = new SetColor(ClientId, displayId.Value, color, blink);
                        break;
                    case 0x09: // get status (heartBit)
                        lock (_sendDataQueueLock)
                        {
                            message[6] = 252; // status command
                            _sendDataQueue.Enqueue(message);
                        }
                        break;
                    case 0x11: // set blink
                        command = new SetBlink(ClientId, displayId.Value);
                        break;
                }
                if(command != null && displayId.HasValue && displayId != 0)
                    ReceiveDataAction?.Invoke(command);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"ProcessReceiveData: ClientId {ClientId}. Message: {hexMessage} Error creating command. {e}");
        }
    }

    /// <summary>
    /// Varrega o array de bytes e encontra o par de STX e ETX, se tiver dois STX antes de um ETX, desconsidera a primeira parte
    /// </summary>
    /// <param name="strRcvd">String Recebida</param>
    /// <returns>retorna uma tupla com a posicao do STX e do ETX, retorna -1 caso não encontrar</returns>
    /// //<STX>R01,22,456,2024-02-02<ETX><STX>R01,23,667
    private (int stx, int ext) GetMessageDelimiters(byte[] strRcvd)
    {
        int stxPos = -1;
        int extPos = -1;

        int i = 0;
        while (i < strRcvd.Length)
        {
            if (strRcvd[i] == STX)
            {
                stxPos = i;
            }
            else if (strRcvd[i] == ETX)
            {
                extPos = i;
                break;
            }

            i++;
        }

        return (stxPos, extPos);
    }
    
    public byte[] CreateMessage(string messageType, string device, string value)
    {
        // Monta a mensagem no formato esperado
        string message = $"{messageType}|{device}|{value}";
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);

        // Adiciona os bytes de controle STX e ETX
        List<byte> messageWithControlBytes = new List<byte>();
        messageWithControlBytes.AddRange(new byte[] { 0x02, 0x02 }); // STX_LC
        messageWithControlBytes.AddRange(messageBytes);
        messageWithControlBytes.AddRange(new byte[] { 0x03, 0x03 }); // ETX_LC

        return messageWithControlBytes.ToArray();
    }

    public void SendMessage(string messageType, string device, string value)
    {
        byte[] message = CreateMessage(messageType, device, value);
        _sendDataQueue.Enqueue(message);
    }
}