﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Surfea.Net
{
	/// <summary>
	/// Client side TCP socket helper class, based on the example at:
	/// http://msdn.microsoft.com/en-us/library/kb5kfec7(v=vs.110).aspx
	/// </summary>
	public class SocketClient
	{
		// Underlying socket
		private Socket _socket;
		private IPEndPoint _server;

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
		}

		#endregion

		public void Connect()
		{
			try {
				_socket.Connect(_server);

				Console.WriteLine("Socket connected to {0}", _socket.RemoteEndPoint.ToString());
			} catch (SocketException se) {
				Console.WriteLine("SocketException : {0}",se.ToString());
			} catch (Exception e) {
				Console.WriteLine("Unexpected exception : {0}", e.ToString());
			}
		}

		public void Send(byte[] arr)
		{
			_socket.Send(arr);
		}

		public void Send(String msg)
		{
			byte[] byteMsg = Encoding.ASCII.GetBytes(msg);
			Send (byteMsg);
		}

		public void Close()
		{
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();
		}
	}
}