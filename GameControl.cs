using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net.Http;
using System.Net.Security;

using Gum.Forms;
using Gum.Forms.Controls;
using MonoGameGum;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Linq;

namespace sojourner;

public enum Screen {
    CodeManual, PulseDiagram, PlatformMap, Intro
}

public class GameControl : Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    EventBasedNetListener listener;
    NetManager server;
    SpriteFont font;
    int screenWidth, screenHeight;
    const float gameProportion = 0.75f;
    List<MCButton> ws = [];
    ButtonContainer toSendWs;
    int wordButtonStartX;
    const int topWordButtonStartY=35;
    const int botWordButtonStartY=350;
    const int yWordSep=25;
    const int xWordSep=5;
    Texture2D codeManualTexture;
    Screen displayScreen = Screen.PlatformMap;
    List<MCButton> headerButtons;
    PulseHandler pulseHandler;
    IntroHandler introHandler;
    UpdatingMap updatingMap;
    
    GumService GumUI => GumService.Default;

    public GameControl() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        screenWidth = 1200;
        screenHeight = 800;
        _graphics.PreferredBackBufferWidth = screenWidth;
        _graphics.PreferredBackBufferHeight = screenHeight;

        wordButtonStartX = (int)(screenWidth*gameProportion)+10;
    }

    private void HandleRoverMsg(string s) {
        List<string> newws = s.Split(' ').ToList();
        List<MCButton> newbtns = [];
        
        int wx = wordButtonStartX;
        int wy = topWordButtonStartY;

        for (int i=0;i<newws.Count;i++) {
            string w = newws[i];
            MCButton btn = new(wx,wy,w,()=>{toSendWs.AddButton(w);}, font,1f);

            if (wx+btn.width>screenWidth-10) {
                wx=wordButtonStartX;
                wy+=yWordSep;
                btn.x=wx;
                btn.y=wy;
            } else {
                wx+=btn.width+xWordSep;
            }
            
            newbtns.Add(btn);
        }
        
        ws = newbtns;
    }

    protected override void Initialize() {
        base.Initialize();
    }

    private void LoadHeaderButtons() {
        headerButtons = [
            new MCButton(0,0,"intro screen",()=>{displayScreen=Screen.Intro;}, font),
            new MCButton(0,0,"code manual",()=>{displayScreen=Screen.CodeManual;}, font),
            new MCButton(0,0,"pulse diagram", ()=>{displayScreen=Screen.PulseDiagram;}, font),
            new MCButton(0,0,"platform map", ()=>{displayScreen=Screen.PlatformMap;}, font),
        ];

        int pos = 20;
        foreach (var item in headerButtons) {
            item.UpdateX(pos);
            item.UpdateY(20);
            pos += item.width + 20;
        }
    }

    private void LoadServer() {
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
        server.Start(9050);
        listener.ConnectionRequestEvent += request => {
            request.AcceptIfKey("sojourner");
        };
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            string text = dataReader.GetString(500);
            switch (text.Split(' ')[0]) {
                case "message":
                    HandleRoverMsg(text[8..]);
                    break;
                case "position":
                    string[] split = text.Split(' ');
                    updatingMap.UpdateRoverPos(Convert.ToInt32(split[1]),Convert.ToInt32(split[2]));
                    break;
            }
            dataReader.Recycle();
        };
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Content.Load<SpriteFont>("Cantarell");
        toSendWs = new ButtonContainer(wordButtonStartX, botWordButtonStartY, screenWidth-10-wordButtonStartX, 360, font, yWordSep, xWordSep);
        InitGumUI();
        codeManualTexture = Content.Load<Texture2D>("images/code-manual");

        LoadHeaderButtons();
        
        introHandler = new IntroHandler(Content, 0, screenHeight-720, (int)(screenWidth*gameProportion), 720);
        pulseHandler = new PulseHandler(Content, 0, screenHeight-720, (int)(screenWidth*gameProportion), 720);
        int padding=20;
        updatingMap  = new UpdatingMap(padding,screenHeight-720-padding,(int)(screenWidth*gameProportion)-2*padding,720-2*padding,Content,GraphicsDevice);

        LoadServer();
    }

    protected void Send(string s) {
        var writer = new NetDataWriter();
        writer.Put(s);
        server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    protected override void Update(GameTime gameTime) {
        Kb.Update();
        server.PollEvents();
        
        if (Kb.IsDown(Ks.Quit)) {
            Exit();
        }

        // if (Kb.IsTapped(Ks.Left)) {
        //     HandleRoverMsg("banana pirate skibidi orange xkcd");
        // }

        foreach (var w in ws) {
            w.Update();
        }
        toSendWs.Update();

        foreach (var btn in headerButtons) {
            btn.Update();
        }

        pulseHandler.Update(Send);
        introHandler.Update();

        GumUI.Update(gameTime);
        base.Update(gameTime);
    }

    private void DrawGameScreen() {
        // draw headers
        foreach (var btn in headerButtons) {
            btn.Draw(_spriteBatch, font);
        }

        // draw content
        switch (displayScreen) {
            case Screen.CodeManual:
                _spriteBatch.Draw(codeManualTexture, new Vector2(0,screenHeight-400), Color.White);
                break;

            case Screen.PlatformMap:
                updatingMap.Draw(_spriteBatch);
                break;

            case Screen.PulseDiagram:
                pulseHandler.Draw(_spriteBatch);
                break;

            case Screen.Intro:
                introHandler.Draw(_spriteBatch);
                break;
        }
    }

    private void InitGumUI() {
        GumUI.Initialize(this, DefaultVisualsVersion.V3);

        Button button = new Button();
        button.AddToRoot();
        button.X = (int)(screenWidth*gameProportion+10);
        button.Y = screenHeight - 80;
        button.Width = (int)(screenWidth*(1-gameProportion)-20);
        button.Height = 20;
        button.Text = "Send data";
        button.Click += (_,_) => {
            SendToRover();
        };
    }

    private void SendToRover() {
        Send(toSendWs.CreateMessage());
    }

    private void DrawWord(string w, int x, int y) {
        _spriteBatch.DrawString(font, w, new Vector2(x,y), Color.Black, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
    }

    private void DrawCommMenu() {
        int initx = (int)(screenWidth*gameProportion);
        int width = screenWidth-initx;

        // white bg
        _spriteBatch.FillRectangle(new Rectangle(initx, 0, width, screenHeight), Color.White);

        // incoming messages
        _spriteBatch.DrawString(font, "Incoming words:", new Vector2(initx+10, 10), Color.Black);
        foreach (var w in ws) {
            w.Draw(_spriteBatch, font);
        }

        // outgoing
        toSendWs.Draw(_spriteBatch);
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
