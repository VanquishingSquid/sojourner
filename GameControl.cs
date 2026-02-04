using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net.Http;
using System.Net.Security;

namespace sojourner;

public class GameControl : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    EventBasedNetListener listener;
    NetManager server;

    public GameControl() {
        Console.WriteLine("mission control initiated");

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        listener = new EventBasedNetListener();
        server = new NetManager(listener);
        server.Start(9050);
        listener.ConnectionRequestEvent += request => {
            request.AcceptIfKey("sojourner");
        };
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            Console.WriteLine("We got: {0}", dataReader.GetString(100));
            dataReader.Recycle();
        };
    }

    protected override void Initialize() {
        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected void Send(string s) {
        var writer = new NetDataWriter();
        writer.Put(s);
        server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        server.PollEvents();

        if (Keyboard.GetState().IsKeyDown(Keys.A)) {
            Send("hi");
        }

        server.PollEvents(); // sever stuff
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
