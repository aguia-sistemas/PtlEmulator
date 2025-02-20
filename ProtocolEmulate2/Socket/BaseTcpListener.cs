// BaseTcpListener.cs
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using PtlEmulator.App.Command;

namespace PtlEmulator.App.Socket
{
    public class BaseTcpListener
    {
        private readonly TcpListener _listener;
        private Task? _listenerTask;
        private readonly TaskFactory _taskFactory;
        private readonly CancellationTokenSource _tokenSource;
        private bool _listen = true;
        private int _clientCounter = 0;
        private readonly ConcurrentDictionary<int, BaseTcpClient> _clientList;

        public event Func<int, Action<BaseCommand>>? ClientConnectedEvent;

        public BaseTcpListener(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            this._taskFactory = new TaskFactory();
            this._tokenSource = new CancellationTokenSource();
            this._clientList = new ConcurrentDictionary<int, BaseTcpClient>();
        }

        public void StartServer(int clientId)
        {
            _listen = true;
            _listenerTask = _taskFactory.StartNew(() => StartListener(clientId), _tokenSource.Token);
        }

        public async void StartListener(int clientId)
        {
            _listener.Start();

            while (_listen)
            {
                if (_listener.Pending())
                {
                    Debug.WriteLine($"Client connected {clientId}");
                    var client = new AtopClient(await _listener.AcceptTcpClientAsync(), clientId, _tokenSource, ClientConnectedEvent, _taskFactory);
                    _clientList.TryAdd(clientId, client);

                    // Chame o evento ClientConnectedEvent para cada cliente conectado
                    ClientConnectedEvent?.Invoke(clientId);
                }
                else
                {
                    await Task.Delay(100); //<--- timeout
                }
            }
        }

        public AtopClient GetClient(int clientId)
        {
            _clientList.TryGetValue(clientId, out var client);
            return client as AtopClient;
        }
    }
}