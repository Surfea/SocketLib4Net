using System;

using Surfea.Net;

namespace SocketLib
{
	public class EchoServer
	{
		SocketServer _serv;

		public EchoServer (int port)
		{
			// Start server on 23
			_serv = new SocketServer ();


		}
	}
}

