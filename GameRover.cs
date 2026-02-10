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

public class GameRover : Game {
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
    int recvdFeedbackMsgTime = 0;
    List<CodeObject> codeObjects;
    List<bool> randomCode;
    Texture2D correspondingCodeTexture;
    List<BoolBox> codeInputObjects = [];
    MovingBlock codeMovingBlock;
    int codeBlockTimer = 0;
    SolidPlatform powerMovingPlatform;
    PowerSystem powerSystem;
    ExitDoor exitDoor;
    WordContainer wordContainer;
    Texture2D unstablePlatformSign;
    RoverIntroHandler introHandler;
    DecorHandler decorHandler;

    GumService GumUI => GumService.Default;

    public GameRover() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        screenWidth = 1200;
        screenHeight = 800;
        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;

        LoadDict();
    }

    private void HandleMissionControlMessage(NetPacketReader dataReader) {
        string text = dataReader.GetString(600);
        switch (text.Split(' ')[0]) {
            case "message": 
                recvdFeedbackMsgTime = maxFeedbackMsgTime;
                wordContainer.NewMessage(text[8..].Split(' ').ToList());
                break;
            case "startpulse":
                exitDoor.UpdatePulse(true);
                break;
            case "endpulse":
                exitDoor.UpdatePulse(false);
                break;
        }
        dataReader.Recycle();
    }

    private void MakeMap() {
        codeMovingBlock = new MovingBlock(Content, correspondingCodeTexture, 883,570,150,100);

        solids = new() {
            new SolidRect(883,670,300,100),  // block after code entry below
            new SolidRect(883,0,150,570),    // block after code entry above
            codeMovingBlock,
            new SolidRect(1033,0,150,470), // block above moving platform
            new SolidRect(1183,570,700,200), // block after moving platform
            new SolidRect(-3250,0,300,screenHeight), // block preventing falling off the left
            new SolidRect(1883,0,400,screenHeight), // block preventing falling off the right
            
            new SolidGround(-3000,770,6000,30,Content), // ground
        };

        triangles = new() {
            new SolidTriangle(Content,300,670,100,100,false), // slope up to code
        };

        platforms = new() {
            new SolidPlatform(Content,400,670,483), // code platform
            powerMovingPlatform,
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
            if (w.Length>0) {
                if (words.Contains(w.ToLower())) {
                    ws.Add(w.ToLower());
                } else {
                    failFeedbackMsgTime=maxFeedbackMsgTime;
                    return;
                }
            }
        }

        tbToMc.Text="";
        successFeedbackMsgTime=maxFeedbackMsgTime;
        Send("message " + string.Join(' ', ws));
    }

    protected override void Initialize() {
        base.Initialize();
    }

    private void Send(string s) {
        var writer = new NetDataWriter();
        writer.Put(s);
        client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    private void LoadCode() {
        // randomly generated code
        codeObjects = [
            new([false,true ,false,false,false,true ,false,false,true ,false,false], Content.Load<Texture2D>("images/symbols/blob")),
            new([true ,false,true ,false,true ,true ,false,true ,false,true ,true ], Content.Load<Texture2D>("images/symbols/bridge")),
            new([false,false,false,false,false,true ,true ,false,true ,false,false], Content.Load<Texture2D>("images/symbols/circle")),
            new([false,false,true ,false,false,true ,true ,false,true ,false,false], Content.Load<Texture2D>("images/symbols/cylinder")),
            new([false,false,true ,true ,false,true ,true ,false,true ,true ,false], Content.Load<Texture2D>("images/symbols/hexagon")),
            new([true ,false,true ,true ,true ,true ,false,false,false,true ,true ], Content.Load<Texture2D>("images/symbols/parallelogram")),
            new([true ,false,false,true ,true ,false,false,false,true ,true ,true ], Content.Load<Texture2D>("images/symbols/plus")),
            new([true ,false,true ,false,true ,false,false,true ,true ,false,true ], Content.Load<Texture2D>("images/symbols/spiral")),
            new([true ,false,true ,false,false,false,false,true ,true ,false,false], Content.Load<Texture2D>("images/symbols/star")),
            new([true ,false,true ,false,false,false,false,true ,false,false,false], Content.Load<Texture2D>("images/symbols/trident")),
        ];
        CodeObject chosen = codeObjects[Random.Shared.Next(codeObjects.Count)];
        randomCode = chosen.Key;
        correspondingCodeTexture = chosen.Value;

        int initx = 410;
        int btnWidth = 32;
        int padding = 10;
        int inity = 628;
        for (int i=0; i<11; i++) {
            codeInputObjects.Add(new BoolBox(Content, initx+(btnWidth+padding)*i,inity, btnWidth, btnWidth));
        }
    }

    private void LoadUnstablePlatforms() {
        int initx = PlatformData.initx, inity = PlatformData.inity, unitSize = PlatformData.unitSize;

        platforms.AddRange(PlatformData.xsysplatforms.Select(coords => {
            int x1 = initx - unitSize*coords.Item1.Item1;
            int y1 = inity - unitSize*coords.Item1.Item2;
            int x2 = initx - unitSize*coords.Item2.Item1;
            int y2 = inity - unitSize*coords.Item2.Item2;
            return new SolidPlatform(Content, Math.Min(x1,x2), Math.Min(y1,y2), Math.Abs(x1-x2));
        }));

        platforms.AddRange(PlatformData.xsysplatformsfake.Select(coords => {
            int x1 = initx - unitSize*coords.Item1.Item1;
            int y1 = inity - unitSize*coords.Item1.Item2;
            int x2 = initx - unitSize*coords.Item2.Item1;
            int y2 = inity - unitSize*coords.Item2.Item2;
            return new SolidPlatform(Content, Math.Min(x1,x2), Math.Min(y1,y2), Math.Abs(x1-x2), true);
        }));

        triangles.AddRange(PlatformData.xsystriangles.Select(coords => {
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
            return new SolidTriangle(Content, x1,Math.Min(y1,y2),x2-x1,Math.Abs(y2-y1),y1<y2);
        }));

        triangles.AddRange(PlatformData.xsystrianglesfake.Select(coords => {
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
            return new SolidTriangle(Content,x1,Math.Min(y1,y2),x2-x1,Math.Abs(y2-y1),y1<y2, true);
        }));
    }

    private void StartClient() {
        listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();
        client.Connect("localhost", 9050, "sojourner");
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            HandleMissionControlMessage(dataReader);
        };
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // text
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("Corptic DEMO");

        unstablePlatformSign = Content.Load<Texture2D>("images/unstable-platforms-sign");
        introHandler = new RoverIntroHandler(Content);
        InitGumUI();

        powerMovingPlatform = new SolidPlatform(Content,1033,570,150,false,Content.Load<Texture2D>("images/moving-platform"),new Vector2(0,-30));
        powerSystem = new PowerSystem(powerMovingPlatform, Content);
        exitDoor    = new ExitDoor(Content,1350,570);
        wordContainer = new WordContainer((int)(screenWidth*gameProportion+10), 280, (int)(screenWidth*(1-gameProportion)-20),screenHeight-280-10,font);
        decorHandler = new DecorHandler(Content, (int)(screenWidth*gameProportion), screenHeight);
        LoadCode();
        MakeMap();
        LoadUnstablePlatforms();
        rover = new Rover(solids, triangles, platforms, Content);

        StartClient();
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

        if (!introHandler.isActive) {
            xoffset = (int)(rover.Update(!tbToMc.IsFocused)-screenWidth*gameProportion/2);
            codeInputObjects.ForEach(e => e.Update(rover));
            Send($"position {rover.x} {rover.y+rover.height}");
            CheckCode();
            powerSystem.Update(rover);
            exitDoor.Update(rover);

            GumUI.Update(gameTime);
        }

        base.Update(gameTime);
    }

    private void DrawRandomSigns() {
        _spriteBatch.Draw(unstablePlatformSign, new Vector2(-90-xoffset,770-118), Color.White);
    }

    private void DrawGameScreen() {
        decorHandler.DrawBg(_spriteBatch, xoffset);

        foreach (SolidRect s in solids) {
            if (s is SolidGround sg) {
                sg.Draw(_spriteBatch, xoffset);
            } else if (s is MovingBlock mb) {
                mb.Draw(_spriteBatch, xoffset);
            } else {
                s.Draw(_spriteBatch, xoffset);
            }
        }
        foreach (SolidTriangle s in triangles) {
            s.Draw(_spriteBatch, xoffset);
        }
        
        DrawRandomSigns();
        powerSystem.DrawLift(_spriteBatch,xoffset);
        
        foreach (SolidPlatform s in platforms) {
            s.Draw(_spriteBatch, xoffset);
        }

        powerSystem.DrawSwitch(_spriteBatch,xoffset);

        // code display
        codeInputObjects.ForEach(e=>e.Draw(_spriteBatch, xoffset));
        // _spriteBatch.Draw(correspondingCodeTexture, new Vector2(510-xoffset,578), Color.White);

        exitDoor.Draw(_spriteBatch, xoffset);

        rover.Draw(_spriteBatch, (int)(screenWidth*gameProportion));

        decorHandler.DrawFg(_spriteBatch, xoffset);
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
            _spriteBatch.DrawString(font, "Message sent!", new Vector2(initx+10, 230), Color.Green*(successFeedbackMsgTime/(float)maxFeedbackMsgTime));
            failFeedbackMsgTime = 0;
        } else if (failFeedbackMsgTime-->0) {
            _spriteBatch.DrawString(font, "Message must be valid English words", new Vector2(initx+10, 230), Color.Red*(failFeedbackMsgTime/(float)maxFeedbackMsgTime));
        }

        if (recvdFeedbackMsgTime-->0) {
            _spriteBatch.DrawString(font, "New message from mission control:", new Vector2(initx+10, 255), Color.Black*(recvdFeedbackMsgTime/(float)maxFeedbackMsgTime));
        }

        wordContainer.Draw(_spriteBatch);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        if (introHandler.isActive) {
            introHandler.Draw(_spriteBatch);
        } else {
            DrawGameScreen();
            DrawCommMenu();
        }

        _spriteBatch.End();

        if (!introHandler.isActive) {
            GumUI.Draw();
        }

        base.Draw(gameTime);
    }
}
