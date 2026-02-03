using System;
using System.Collections.Generic;
using System.Threading;

using Lidgren.Network;

namespace Client;

static class Program {
    private static NetClient s_client;

    static void Main() {
        NetPeerConfiguration config = new NetPeerConfiguration("chat");
        config.AutoFlushSendQueue=false;
        s_client = new NetClient(config);
        s_client.RegisterReceivedCallback(new SendOrPostCallback(GotMessage));
        Connect("127.0.0.1",14242);
        Console.ReadLine();
        Send("bruh");
        Console.ReadLine();
        s_client.Shutdown("Bye");
    }

    public static void GotMessage(object peer) {
        NetIncomingMessage im;
        while ((im=s_client.ReadMessage()) != null) {
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
                    if (status == NetConnectionStatus.Connected) {
                        // s_form.EnableInput();
                    } else {
                        // s_form.DisableInput();
                    }

                    if (status == NetConnectionStatus.Disconnected) {
                        // s_form.button2.Text = "Connect";
                    }

                    string reason = im.ReadString();
                    Output(status.ToString() + ": " + reason);

                    break;
                default:
                    Output(im.ReadString());
                    break;
            }
        }
    }

    public static void Output(string s) {
        Console.WriteLine(s);
    }

    public static void Shutdown() {
        s_client.Disconnect("disconnect requested by user");
    }

    public static void Send(string text) {
        NetOutgoingMessage om = s_client.CreateMessage(text);
        s_client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
        Output("Sending '" + text + "'");
        s_client.FlushSendQueue();
    }
    public static void Connect(string host, int port) {
        s_client.Start();
        NetOutgoingMessage hail = s_client.CreateMessage("This is the hail message");
        s_client.Connect(host, port, hail);
    }
}