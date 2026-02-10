using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace sojourner;

public class PulseHandler {
    int x,y,width,height;
    int barx, bary, barwidth, barheight;
    int padding = 20;

    Texture2D texture;
    List<int> times = [0,10,18,27,40,50,57,70,82];
    const int twindow = 3;
    bool allowed = false;
    int timer = 0;
    int i=0;

    public PulseHandler(ContentManager Content, int x, int y, int width, int height) {
        this.texture = Content.Load<Texture2D>("images/pulse-manual");
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.barx = x+padding;
        this.barwidth = width-2*padding;
        this.bary = (int)(width*0.3);
        this.barheight = 30;

        int len = times.Count;
        for (int i=0; i<len; i++) {
            times.Add(times[i]+90);
        }
    }

    public void Update(Action<string> Send) {
        timer++;
        if (timer>=60*90) {
            timer=0;
        }

        if (timer/60f<twindow || (timer/60f>twindow && timer/60f>times[i])) {
            if (!allowed) {
                allowed = true;
                Send("startpulse");
            }
            if (timer/60f>times[i]+twindow) {
                allowed = false;
                i = (i+1) % (times.Count/2);
                Send("endpulse");
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        spriteBatch.Draw(texture, new Vector2(x,y), Color.White);
        spriteBatch.FillRectangle(new(barx,bary,barwidth,barheight), Color.Red);

        float secondlength = barwidth/30f;
        float timeoffset  = timer/60f * secondlength;
        foreach (int i in times) {
            float originalPos = barx + i*secondlength;
            float newPos      = originalPos - timeoffset;
            float newEnd      = newPos + twindow*secondlength;
            if (newEnd>barx && newPos<barx+barwidth) {
                newPos = Math.Max(barx, newPos);
                newEnd = Math.Min(newEnd, barx+barwidth);
                spriteBatch.FillRectangle(new(newPos,bary,newEnd-newPos,barheight), Color.Green);
            }
        }
    }
}