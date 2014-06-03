using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;


namespace Surfea.Net
{
	public class SocketServer
	{
		// Underlying socket params
		private Socket _listener;
		private List<Socket> _connections;

		private Socket _socket;
		private IPEndPoint _server;

		private bool _listening = false;
		private Thread _listenThread;
		private byte[] _recv = new byte[4096];
		protected const int CONNECT_TIMEOUT = 5000; // ms

		public event EventHandler ByteEvent;
		public event EventHandler MessageEvent;
		public event EventHandler DisconnectedEvent;

		public virtual void OnByte(ByteEventArgs e)
		{
			if (ByteEvent != null)
				ByteEvent (this, e);
		}

		public virtual void OnMessage(MessageEventArgs e)
		{
			if (MessageEvent != null)
				MessageEvent (this, e);
		}

		public virtual void OnDisconnected(EventArgs e)
		{
			if (DisconnectedEvent != null)
				DisconnectedEvent (this, e);
		}
//
		public SocketServer () : this(23) { }
//
		public SocketServer(int port)
		{
			_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
			IPAddress ip = IPAddress.Any;
			_server = new IPEndPoint (ip, port);
			_listener.Bind (_server);

			_connections = new List<Socket> ();
		}

		public void Start()
		{
			// Start the listening thread
			_listening = true;
			ThreadStart listenDelegate = new ThreadStart (Listen);
			_listenThread = new Thread (listenDelegate);
			_listenThread.Start ();
		}

		/// <summary>
		/// Routine to listen for connections from clients.
		/// </summary>
		protected void Listen()
		{
			_listener.Listen (1);

			while (_listening) {
				Socket clientSocket = _listener.Accept ();

				SocketClient client = new SocketClient (clientSocket);

				client.ByteEvent += (object sender, EventArgs e) => {
					Console.WriteLine("Got bytes: " + e.ToString());
					OnByte((ByteEventArgs)e);
				};

				client.MessageEvent += (object sender, EventArgs e) => {
					Console.WriteLine("Got message: " + ((MessageEventArgs)e).Message);
					OnMessage((MessageEventArgs)e);
				};

//				client.DisconnectedEvent += (object sender, EventArgs e) => {
//					Console.WriteLine("Client Disconnected");
//					OnDisconnected
//				};

				_connections.Add (client);
			}

		}
	}
}

