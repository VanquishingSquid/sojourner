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
using System.Linq;

namespace sojourner;

using CodeObject = KeyValuePair<List<bool>,Texture2D>;

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
    List<CodeObject> codeObjects;
    List<bool> randomCode;
    Texture2D correspondingCodeTexture;
    List<BoolBox> codeInputObjects = [];
    SolidRect codeMovingBlock;
    int codeBlockTimer = 0;

    GumService GumUI => GumService.Default;

    public GameRover() {
        Console.WriteLine("sojourner initiated");
        
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        // screenWidth = _graphics.PreferredBackBufferWidth;
        // screenHeight = _graphics.PreferredBackBufferHeight;
        // Console.WriteLine($"{screenWidth} {screenHeight}");
        screenWidth = 1200;
        screenHeight = 800;
        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;

        // server
        listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();
        client.Connect("localhost", 9050, "sojourner");
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            mcText = dataReader.GetString(300);
            dataReader.Recycle();
        };

        LoadDict();
    }

    private void MakeMap() {
        codeMovingBlock = new SolidRect(883,570,150,100);

        solids = new() {
            new SolidRect(-1000,770,2000,30), // ground
            new SolidRect(883,670,150,100),  // block after code entry below
            new SolidRect(883,0,150,570),    // block after code entry above
            codeMovingBlock,
        };

        triangles = new() {
            new SolidTriangle(300,670,200,100,false), // slope up to code
        };

        platforms = new() {
            new SolidPlatform(500,670,383), // code platform
        };
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
        base.Initialize();
    }

    protected void Send(string s) {
        var writer = new NetDataWriter();
        writer.Put(s);
        client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    protected void LoadCode() {
        // randomly generated code
        codeObjects = [
            new([true,  false, false, true,  true,  false, true,  false, false], Content.Load<Texture2D>("images/star")),
            new([true,  false, false, true,  true,  true,  true,  false, false], Content.Load<Texture2D>("images/circle")),
            new([false, true,  true,  false, false, false, true,  false, true ], Content.Load<Texture2D>("images/square")),
            new([true,  false, false, false, true,  false, false, true,  true ], Content.Load<Texture2D>("images/triangle"))
        ];
        CodeObject chosen = codeObjects[Random.Shared.Next(codeObjects.Count)];
        randomCode = chosen.Key;
        correspondingCodeTexture = chosen.Value;

        int initx = 510;
        int btnWidth = 32;
        int padding = 10;
        int inity = 628;
        for (int i=0; i<9; i++) {
            codeInputObjects.Add(new BoolBox(initx+(btnWidth+padding)*i,inity, btnWidth, btnWidth));
        }
    }

    protected void LoadUnstablePlatforms() {
        const int initx = -200, inity = 770, unitSize = 25;

        List<((int,int),(int,int))> xsysplatforms = [
            ((5,5),(20,5)),
            ((60,5),(80,5)),
            ((-5,10),(15,10)),
            ((45,10),(55,10)),
            ((75,10),(95,10)),
            ((20,15),(40,15)),
            ((50,15),(55,15)),
            ((70,15),(75,15)),
            ((80,15),(95,15)),
            ((10,20),(15,20)),
            ((40,20),(50,20)),
            ((60,20),(65,20)),
            ((80,20),(100,20)),
            ((45,25),(100,25)),
            ((105,20),(110,20))
        ];

        List<((int,int),(int,int))> xsystriangles = [
            ((0,0),(5,5)),
            ((5,5),(0,10)),
            ((-2,10),(10,20)),
            ((15,20),(20,25)),
            ((15,10),(20,15)),
            ((20,5),(25,10)),
            ((20,20),(25,15)),
            ((40,15),(50,5)),
            ((45,25),(50,20)),
            ((45,20),(50,15)),
            ((55,15),(60,20)),
            ((55,10),(60,15)),
            ((55,10),(60,5)),
            ((65,20),(70,15)),
            ((65,5),(70,10)),
            ((75,15),(80,20)),
            ((80,5),(85,10)),
            ((85,15),(90,10)),
            ((95,15),(100,20)),
            ((95,10),(100,15)),
            ((100,25),(105,20))
        ];

        // List<((int,int),(int,int))> xsysplatforms = [
        // ];

        platforms.AddRange(xsysplatforms.Select(coords => {
            int x1 = initx - unitSize*coords.Item1.Item1;
            int y1 = inity - unitSize*coords.Item1.Item2;
            int x2 = initx - unitSize*coords.Item2.Item1;
            int y2 = inity - unitSize*coords.Item2.Item2;
            return new SolidPlatform(Math.Min(x1,x2), Math.Min(y1,y2), Math.Abs(x1-x2));
        }));

        triangles.AddRange(xsystriangles.Select(coords => {
            int x1 = initx - unitSize*coords.Item1.Item1;
            int y1 = inity - unitSize*coords.Item1.Item2;
            int x2 = initx - unitSize*coords.Item2.Item1;
            int y2 = inity - unitSize*coords.Item2.Item2;

            if (x1>x2) {
                int xt = x1;
                int yt = y1;
                x1 = x2;
                y1 = y2;
                x2 = xt;
                y2 = yt;
            }

            // we know that x1 has to be less than x2
            return new SolidTriangle(x1,Math.Min(y1,y2),x2-x1,Math.Abs(y2-y1),y1<y2);
        }));
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // text
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("Cantarell");
        InitGumUI();

        LoadCode();
        MakeMap();
        LoadUnstablePlatforms();
        rover = new Rover(solids, triangles, platforms);
    }

    private void CheckCode() {
        bool codeCorrect = true;
        for (int i = 0; i < codeInputObjects.Count; i++) {
            if (codeInputObjects[i].status!=randomCode[i]) {
                codeCorrect=false;
                break;
            }
        }
        
        if (codeCorrect || codeBlockTimer>0) {
            if (codeBlockTimer++>3) {
                codeMovingBlock.y-=1;
                codeBlockTimer=1;
            }
        }
    }

    protected override void Update(GameTime gameTime) {
        Kb.Update();
        client.PollEvents(); // sever stuff
        
        if (Kb.IsDown(Ks.Quit)) {
            Exit();
        }

        xoffset = (int)(rover.Update()-screenWidth*gameProportion/2);
        codeInputObjects.ForEach(e => e.Update(rover));
        CheckCode();

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

        // code display
        codeInputObjects.ForEach(e=>e.Draw(_spriteBatch, xoffset));
        _spriteBatch.Draw(correspondingCodeTexture, new Vector2(510-xoffset,578), Color.White);
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
