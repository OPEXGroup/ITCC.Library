// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ITCC.Logging.Core;
using LogLevel = ITCC.Logging.Core.LogLevel;

namespace ITCC.HTTP.Testing.Networking
{
    public class AsynchronousClient
    {
        private const int Port = 8888;
        private static readonly ManualResetEvent ConnectDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent SendDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        private static string _response = string.Empty;

        public static void StartClient()
        {
            try
            {
                var ipAddress = IPAddress.Parse("127.0.0.1");
                var remoteEp = new IPEndPoint(ipAddress, Port);
                var client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEp, ConnectCallback, client);
                ConnectDone.WaitOne();

                const string request = "GET 127.0.0.1:8888/bigdata HTTP/1.1\r\nName: Value\r\n\r\n";
                Send(client, request);
                SendDone.WaitOne();
                Receive(client);
                Thread.Sleep(4000);
                Send(client, request);
                SendDone.WaitOne();
                

                
                ReceiveDone.WaitOne();

                LogMessage($"Response received : {_response}");

                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                LogMessage(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                LogMessage($"Socket connected to {client.RemoteEndPoint}");
                ConnectDone.Set();
            }
            catch (Exception e)
            {
                LogMessage(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                var state = new StateObject { WorkSocket = client };
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }
            catch (Exception e)
            {
                LogMessage(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;
                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.TotalReceived += bytesRead;
                    var kb = bytesRead/1024;
                    state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                    LogMessage($"Got {kb} kbytes ({state.TotalReceived / 1024} total)");
                    client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                }
                else
                {
                    if (state.Sb.Length > 1)
                    {
                        _response = state.Sb.ToString();
                    }
                    ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                LogMessage(e.ToString());
            }
        }

        private static void Send(Socket client, string data)
        {
            var byteData = Encoding.UTF8.GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;
                var bytesSent = client.EndSend(ar);
                LogMessage($"Sent {bytesSent} bytes to server.");
                SendDone.Set();
            }
            catch (Exception e)
            {
                LogMessage(e.ToString());
            }
        }

        private static void LogMessage(string message, LogLevel level = LogLevel.Debug)
        {
            Logger.LogEntry("ASYNC CLIENT", level, message);
        }
    }
}
