﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using log4net.Appender;
using log4net.Core;

namespace log4net.Appenders.Contrib
{
	public class RemoteSyslog5424Appender : AppenderSkeleton, IDisposable
	{
		public RemoteSyslog5424Appender(string server, int port, X509Certificate certificate)
		{
			_socket = new Socket(SocketType.Stream, ProtocolType.IP);
			_socket.Connect(server, port);

			var rawStream = new NetworkStream(_socket);

			_stream = new SslStream(rawStream, false, VerifyServerCertificate);
			var certificates = new X509CertificateCollection(new[] { certificate });
			_stream.AuthenticateAsClient(server, certificates, SslProtocols.Tls, false);

			_writer = new StreamWriter(_stream, Encoding.UTF8);
		}

		public RemoteSyslog5424Appender(string host, int port, string certificatePath)
			: this(host, port, new X509Certificate(certificatePath))
		{
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			var message = RenderLoggingEvent(loggingEvent);
			_writer.WriteLine(message);
			_writer.Flush();
		}

		private static bool VerifyServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		public void Dispose()
		{
			if (_writer != null)
			{
				_writer.Dispose();
				_writer = null;
			}

			if (_stream != null)
			{
				_stream.Dispose();
				_stream = null;
			}

			if (_socket != null)
			{
				_socket.Disconnect(false);
				_socket.Dispose();
				_socket = null;
			}
		}

		private Socket _socket;
		private SslStream _stream;
		private TextWriter _writer;
	}
}
