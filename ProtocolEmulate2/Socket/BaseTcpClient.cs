using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using PtlEmulator.App.Command;

namespace PtlEmulator.App.Socket;

public abstract class BaseTcpClient
{
    protected readonly Queue<byte[]> _sendDataQueue;
    protected readonly object _sendDataQueueLock = new ();
    public int ClientId { get; private set; }
    
    public BaseTcpClient(TcpClient client, int clientId, CancellationTokenSource tokenSource,
        Func<int, Action<BaseCommand>>? clientConnectedEvent, TaskFactory taskFactory)
    {
        this.ClientId = clientId;
        _sendDataQueue = new Queue<byte[]>();
        
        this.ReceiveDataAction = clientConnectedEvent?.Invoke(ClientId);

        var processDataLoop = async () =>
        {   
            await using var networkStream = client.GetStream();
               
            while (true)
            {
                try
                {
                    if (client.Connected)
                    {
                        if (client.Available == 0)
                        {
                            await Task.Delay(100);                            
                            ProcessReceiveData(Array.Empty<byte>());
                        }
                        else
                        {
                            var bufferLength = client.Available;
                            var buffer = new byte[bufferLength];
                            await networkStream.ReadAsync(buffer, 0, bufferLength);
                            
                            ProcessReceiveData(buffer);
                        }

                        await ProcessSendData(networkStream);
                    }
                    else
                    {
                        Debug.WriteLine($"Client disconnected {clientId}");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    throw;
                }
            }
        };
        
        taskFactory.StartNew(processDataLoop, tokenSource.Token);
    }


    public Action<BaseCommand>? ReceiveDataAction { get; set; }

    private async Task<bool> ProcessSendData(NetworkStream networkStream)
    {
        byte[]? buffer = null;
        lock (_sendDataQueueLock)
        {
            if (_sendDataQueue.Any())
            {
                // calc the total amount of bytes in queue
                var totalLength = _sendDataQueue.Sum(it => it.Length);
                // create the buffer
                buffer = new byte[totalLength];
                
                // move each item on queue to buffer
                var pos = 0;
                while (_sendDataQueue.Count > 0)
                {
                    var it = _sendDataQueue.Dequeue();
                    Array.Copy(it, 0, buffer, pos, it.Length);
                    pos += it.Length;
                }
            }
        }

        if (buffer != null)
        {
            string hexMessage = BitConverter.ToString(buffer);
            Debug.WriteLine($"SendData ClientId: {ClientId} Message: {hexMessage}");
            await networkStream.WriteAsync(buffer);
            // data was sent
            return true;
        }
        // no data was sent
        return false;
    }

    protected abstract bool ProcessReceiveData(byte[] buffer);
    /*
       {
           var str = new string(buffer);
           this.ReceiveDataAction?.Invoke(str);
       }
    */
}