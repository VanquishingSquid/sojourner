using System;
using System.Collections.Generic;
using System.Threading;

using Lidgren.Network;

namespace Server;

static class Program {
    private static NetServer s_server;

    static void Main() {
        NetPeerConfiguration config = new NetPeerConfiguration("chat");
        config.MaximumConnections=2;
        config.Port=14242;
        s_server=new NetServer(config);
        ServerLoop();
        Console.ReadLine();
    }

    public static void ServerLoop() {
        while (true) {
            NetIncomingMessage im;
            while ((im=s_server.ReadMessage()) != null) {
                switch (im.MessageType) {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        string text = im.ReadString();
                        Output(text);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                        string reason = im.ReadString();
                        Output(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier)+" "+status.ToString() + ": " + reason);

                        if (status == NetConnectionStatus.Connected) {
                            Output("Remote hail: " + im.SenderConnection.RemoteHailMessage.ReadString());
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        string chat = im.ReadString();
                        Output("Broadcasting '" + chat + "'");
                        List<NetConnection> all = s_server.Connections;
                        all.Remove(im.SenderConnection);
                        if (all.Count>0)
                        {
                            NetOutgoingMessage om = s_server.CreateMessage();
                            om.Write("incoming: " + chat);
                            s_server.SendMessage(om, all, NetDeliveryMethod.ReliableOrdered, 0);
                        }
                        break;
                    default:
                        Output(im.ReadString());
                        break;
                }
                s_server.Recycle(im);
            }
            Thread.Sleep(1);
        }
    }
    
    public static void Output(string s) {
        Console.WriteLine(s);
    }

    public static void StartServer() {
        s_server.Start();
    }

    public static void Shutdown() {
        s_server.Shutdown("Requested by user");
    }
}