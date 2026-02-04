using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net.Http;
using System.Collections.Generic;
using MonoGame.Extended;
using Gum.Forms;
using Gum.Forms.Controls;
using MonoGameGum;
using System.IO;

namespace sojourner;

public class GameRover : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    EventBasedNetListener listener;
    NetManager client;
    Rover rover;
    int screenWidth, screenHeight;
    int xoffset = 0;
    List<SolidRect> solids;
    List<SolidTriangle> triangles;
    List<SolidPlatform> platforms;
    const float gameProportion = 0.75f;
    SpriteFont font;
    TextBox tbToMc;
    HashSet<string> words = new();
    const int maxFeedbackMsgTime = 300;
    int successFeedbackMsgTime = 0;
    int failFeedbackMsgTime = 0;
    string mcText = "";

    GumService GumUI => GumService.Default;

    public GameRover() {
        Console.WriteLine("sojourner initiated");
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        screenWidth = _graphics.PreferredBackBufferWidth;
        screenHeight = _graphics.PreferredBackBufferHeight;

        // server
        listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();
        client.Connect("localhost", 9050, "sojourner");
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            mcText = dataReader.GetString(300);
        };

        // platforms
        solids = new() {
            new SolidRect(-100, screenHeight-100, screenWidth*2, 100)
        };

        triangles = new() {
            new SolidTriangle(-100, screenHeight-200, 200, 100, true)
        };

        platforms = new() {
            new SolidPlatform(0,350,150)
        };

        // rover
        rover = new Rover(solids, triangles, platforms);

        LoadDict();
    }

    private void LoadDict() {
        using StreamReader reader = new("Content/words.txt");
        string text = reader.ReadToEnd();
        foreach (var w in text.Split('\n')) {
            if (w.Length>0) {
                words.Add(w.ToLower());
            }
        }

    }
    
    private void SendToMissionControl() {
        List<string> ws = new List<string>();
        foreach (var w in tbToMc.Text.Split(' ')) {
            // Console.WriteLine($"the word: {w.ToLower()}, isvalid: {words.Contains(w)}");
            if (w.Length>0) {
                if (words.Contains(w)) {
                    ws.Add(w.ToLower());
                } else {
                    failFeedbackMsgTime=maxFeedbackMsgTime;
                    return;
                }
            }
        }

        tbToMc.Text="";
        successFeedbackMsgTime=maxFeedbackMsgTime;
        Send(string.Join(' ', ws));
    }

    protected override void Initialize() {
        // text
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("Cantarell");

        InitGumUI();
        base.Initialize();
    }

    protected void Send(string s) {
        var writer = new NetDataWriter();
        writer.Put(s);
        client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime) {
        Kb.Update();
        client.PollEvents(); // sever stuff
        
        if (Kb.IsDown(Ks.Quit)) {
            Exit();
        }

        xoffset = (int)(rover.Update()-screenWidth*gameProportion/2);

        GumUI.Update(gameTime);
        base.Update(gameTime);
    }

    private void DrawGameScreen() {
        rover.Draw(_spriteBatch, (int)(screenWidth*gameProportion));
        foreach (SolidRect s in solids) {
            s.Draw(_spriteBatch, xoffset);
        }
        foreach (SolidTriangle s in triangles) {
            s.Draw(_spriteBatch, xoffset);
        }
        foreach (SolidPlatform s in platforms) {
            s.Draw(_spriteBatch, xoffset);
        }
    }

    private void InitGumUI() {
        GumUI.Initialize(this, DefaultVisualsVersion.V3);
        tbToMc = new TextBox();
        tbToMc.AddToRoot();
        tbToMc.X = (int)(screenWidth*gameProportion+10);
        tbToMc.Y = 10;
        tbToMc.Width = (int)(screenWidth*(1-gameProportion)-20);
        tbToMc.Height = 150;
        tbToMc.TextWrapping = TextWrapping.Wrap;
        tbToMc.Text = "Send data to mission control here...";

        Button button = new Button();
        button.AddToRoot();
        button.X = (int)(screenWidth*gameProportion+10);
        button.Y = 170;
        button.Width = (int)(screenWidth*(1-gameProportion)-20);
        button.Height = 20;
        button.Text = "Send data";
        button.Click += (_,_) => {
            SendToMissionControl();
        };
    }

    private void DrawCommMenu() {
        int initx = (int)(screenWidth*gameProportion);
        int width = screenWidth-initx;

        // white bg
        _spriteBatch.FillRectangle(new Rectangle(initx, 0, width, screenHeight), Color.White);

        // feedback msgs
        if (successFeedbackMsgTime-->0) {
            _spriteBatch.DrawString(font, "Message sent!", new Vector2(initx+10, 230), Color.Green);
            failFeedbackMsgTime = 0;
        } else if (failFeedbackMsgTime-->0) {
            _spriteBatch.DrawString(font, "Message must be valid English words", new Vector2(initx+10, 230), Color.Red);
        }

        _spriteBatch.DrawString(font, "Messages from mission control:", new Vector2(initx+10, 255), Color.Black);

        _spriteBatch.DrawString(font, mcText, new Vector2(initx+10, 280), Color.Black);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        
        DrawGameScreen();
        DrawCommMenu();
        
        _spriteBatch.End();

        GumUI.Draw();

        base.Draw(gameTime);
    }
}
