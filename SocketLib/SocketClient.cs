﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Surfea.Net
{
	/// <summary>
	/// Client side TCP socket helper class, based on the example at:
	/// http://msdn.microsoft.com/en-us/library/kb5kfec7(v=vs.110).aspx
	/// </summary>
	public class SocketClient
	{
		#region Member Variables

		// Underlying socket params
		private Socket _socket;
		private IPEndPoint _server;
		private bool _listening = false;
		private Thread _listenThread;
		private byte[] _recv = new byte[4096];
		protected const int CONNECT_TIMEOUT = 5000; // ms

		// Exposed Properties
		public bool Connected { get; private set; }

		#endregion

		#region Events

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

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Surfea.Net.TCPClient"/> class.
		/// Defaults to "localhost" port 23 if no arguments supplied
		/// </summary>
		public SocketClient () : this(Dns.GetHostEntry("localhost").AddressList[0], 23) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="Surfea.Net.TCPClient"/> class.
		/// Defaults to "localhost" if no IP Address supplied
		/// </summary>
		/// <param name="port">Port.</param>
		public SocketClient (int port) : this(Dns.GetHostEntry("localhost").AddressList[0], port) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Surfea.Net.SocketClient"/> class.
		/// Defaults to port 23 if no port supplied
		/// </summary>
		/// <param name="ipAddress">Ip address.</param>
		public SocketClient (IPAddress ipAddress) : this(ipAddress, 23) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Surfea.Net.SocketClient"/> class.
		/// </summary>
		/// <param name="ipString">IP Address as a string.</param>
		/// <param name="port">Port.</param>
		public SocketClient (String ipString, int port) : this(IPAddress.Parse(ipString), port) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="Surfea.Net.SocketClient"/> class.
		/// </summary>
		/// <param name="ipAddress">IP Address.</param>
		/// <param name="port">Port.</param>
		public SocketClient (IPAddress ipAddress, int port)
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_server = new IPEndPoint (ipAddress, port);

			Connected = false;
		}

		#endregion

//		public void ConnectB()
//		{
//			try {
//				_socket.Connect(_server);
//
//				Console.WriteLine("Socket connected to {0}", _socket.RemoteEndPoint.ToString());
//			} catch (SocketException se) {
//				Console.WriteLine("SocketException : {0}",se.ToString());
//			} catch (Exception e) {
//				Console.WriteLine("Unexpected exception : {0}", e.ToString());
//			}
//
//			// Start the listening thread
//			_listening = true;
//			ThreadStart listenDelegate = new ThreadStart (Listen);
//			_listenThread = new Thread (listenDelegate);
//			_listenThread.Start ();
//		}

		/// <summary>
		/// Connect to the socket using the default timeout of 5000ms (CONNECT_TIMEOUT).
		/// </summary>
		public void Connect()
		{
			//IAsyncResult result = _socket.BeginConnect( _server, port, null, null );
			Connect (CONNECT_TIMEOUT);

		}

		/// <summary>
		/// Connect to the socket using the specified timeout (ms).
		/// </summary>
		/// <param name="timeout">Timeout (ms).</param>
		public void Connect(int timeout)
		{
			IAsyncResult result = _socket.BeginConnect(_server, null, null);

			bool success = result.AsyncWaitHandle.WaitOne( timeout, true );

			Connected = true;

			if ( !success )
			{
				_socket.Close();
				throw new TimeoutException();
			}

			// Start the listening thread
			_listening = true;
			ThreadStart listenDelegate = new ThreadStart (Listen);
			_listenThread = new Thread (listenDelegate);
			_listenThread.Start ();
		}

		/// <summary>
		/// Send the specified byte arr over the socket.
		/// </summary>
		/// <param name="arr">Arr.</param>
		public void Send(byte[] arr)
		{
			_socket.Send(arr);
		}

		/// <summary>
		/// Send the specified string over the socket.
		/// </summary>
		/// <param name="msg">Message.</param>
		public void Send(String msg)
		{
			byte[] byteMsg = Encoding.ASCII.GetBytes(msg);
			Send (byteMsg);
		}

		/// <summary>
		/// Close the socket.
		/// </summary>
		public void Close()
		{
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();
		}

		/// <summary>
		/// Routine to listen for output from the server and generate events.
		/// </summary>
		public void Listen()
		{
			while (_listening) {
				int numBytes = _socket.Receive (_recv);

				if (numBytes == 0) {
					Console.WriteLine ("Client was disconnected");

					Connected = false;

					// TODO: Fire disconnected event
					OnDisconnected (EventArgs.Empty);

					// Need to close down socket?
				}

				byte[] receivedBytes = new byte[numBytes];
				Array.Copy (_recv, receivedBytes, numBytes);

				// Fire messages
				OnByte (new ByteEventArgs (receivedBytes));
				OnMessage (new MessageEventArgs (Encoding.ASCII.GetString (receivedBytes, 0, numBytes)));

				// Clear receive buffer
				_recv = new byte[4096];
			}
		}
	}

	#region Events

	/// <summary>
	/// Event arguments for passing back byte arrays.
	/// </summary>
	public class ByteEventArgs : EventArgs
	{
		public ByteEventArgs(byte[] bytes)
		{
			this.Bytes = bytes;
		}

		public byte[] Bytes { get; private set; }
	}

	/// <summary>
	/// Event arguments for passing back string messages.
	/// </summary>
	public class MessageEventArgs : EventArgs
	{
		public MessageEventArgs(string msg)
		{
			this.Message = msg;
		}

		public string Message { get; private set; }
	}

	#endregion
}