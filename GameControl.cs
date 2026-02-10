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
using Gum.Forms.DefaultVisuals;
using MonoGameGum.GueDeriving;

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
    SendButtonContainer toSendWs;
    int wordButtonStartX;
    const int topWordButtonStartY=35;
    const int botWordButtonStartY=380;
    const int yWordSep=25;
    const int xWordSep=5;
    Screen displayScreen = Screen.Intro;
    List<MCButton> headerButtons;
    PulseHandler pulseHandler;
    MCIntroHandler introHandler;
    UpdatingMap updatingMap;
    RecvButtonContainer receivedWs;
    const int maxFeedbackMsgTime = 300;
    int feedbackMsgTime = 0;
    CodeHandler codeHandler;
    
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

    protected override void Initialize() {
        base.Initialize();
    }

    private void LoadHeaderButtons() {
        float scale = 1.5f;
        Texture2D buttontexture = Content.Load<Texture2D>("images/header-button");
        headerButtons = [
            new MCButton(0,0,"Intro Screen",()=>{displayScreen=Screen.Intro;}, font, 1.5f, buttontexture),
            new MCButton(0,0,"Code Manual",()=>{displayScreen=Screen.CodeManual;}, font, 1.5f, buttontexture),
            new MCButton(0,0,"Pulse Diagram", ()=>{displayScreen=Screen.PulseDiagram;}, font, 1.5f, buttontexture),
            new MCButton(0,0,"Platform Map", ()=>{displayScreen=Screen.PlatformMap;}, font, 1.5f, buttontexture),
        ];

        int padding = 10;
        int width = (int)(screenWidth*gameProportion/4f);
        for (int i = 0; i < headerButtons.Count; i++) {
            var item = headerButtons[i];
            item.UpdateX(padding/2+i*width-2);
            item.UpdateY(padding/2-2);
            item.width = width-padding;
            item.height = 80-padding;
            item.CenterText();
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
                    receivedWs.HandleRoverMsg(text[8..]);
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
        font = Content.Load<SpriteFont>("Corptic DEMO");
        toSendWs = new SendButtonContainer(wordButtonStartX, botWordButtonStartY+20, screenWidth-10-wordButtonStartX, 295, font, yWordSep, xWordSep);
        receivedWs = new RecvButtonContainer(wordButtonStartX, 40, screenWidth-10-wordButtonStartX, botWordButtonStartY-80, font, yWordSep, xWordSep, toSendWs);
        InitGumUI();

        LoadHeaderButtons();
        
        codeHandler = new CodeHandler(Content);
        introHandler = new MCIntroHandler(Content, 0, screenHeight-720, (int)(screenWidth*gameProportion), 720);
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

        toSendWs.Update();
        receivedWs.Update();

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
        _spriteBatch.FillRectangle(0,0,gameProportion*screenWidth,80,Color.Gray);
        foreach (var btn in headerButtons) {
            btn.Draw(_spriteBatch, font);
        }

        // draw content
        switch (displayScreen) {
            case Screen.CodeManual:
                codeHandler.Draw(_spriteBatch);
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

        // draw header separator last bc it overflows everyth else a lil 
        _spriteBatch.FillRectangle(0,78,gameProportion*screenWidth,4,new Color(40,40,40));
    }

    private void InitGumUI() {
        GumUI.Initialize(this, DefaultVisualsVersion.V3);

        Button button = new Button();
        button.AddToRoot();
        button.X = (int)(screenWidth*gameProportion+10);
        button.Y = screenHeight - 95;
        button.Width = (int)(screenWidth*(1-gameProportion)-20);
        button.Height = 20;
        button.Click += (_,_) => {
            SendToRover();
        };
        button.Text = "Send data";
        // text.Font = 
        // button.Text = "Send data";

        // Button button = new Button();
        // button.AddToRoot();
        // button.X = (int)(screenWidth*gameProportion+10);
        // button.Y = screenHeight - 95;
        // button.Width = (int)(screenWidth*(1-gameProportion)-20);
        // button.Height = 20;
        // button.Text = "Send data";
        // button.Click += (_,_) => {
        //     SendToRover();
        // };
    }

    private void SendToRover() {
        Send(toSendWs.CreateMessage());
        feedbackMsgTime = maxFeedbackMsgTime;
    }

    private void DrawWord(string w, int x, int y) {
        _spriteBatch.DrawString(font, w, new Vector2(x,y), Color.Black, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
    }

    private void DrawCommMenu() {
        int initx = (int)(screenWidth*gameProportion);
        int width = screenWidth-initx;

        // white bg
        _spriteBatch.FillRectangle(new Rectangle(initx, 0, width, screenHeight), Color.White);

        _spriteBatch.DrawString(font, "Incoming words:", new Vector2(initx+10, 10), Color.Black);
        _spriteBatch.DrawString(font, "Select above words\nto send back", new Vector2(initx+10,botWordButtonStartY-25), Color.Black);

        if (feedbackMsgTime-->0) {
            _spriteBatch.DrawString(font, "Message sent!", new Vector2(initx+10,screenHeight-35), Color.Green*(feedbackMsgTime/(float)maxFeedbackMsgTime));
        }

        receivedWs.Draw(_spriteBatch);
        toSendWs.Draw(_spriteBatch);

        // separator to rest of game
        _spriteBatch.FillRectangle(gameProportion*screenWidth-1,0,4,screenHeight,new Color(40,40,40));

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
