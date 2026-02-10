using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace sojourner;

public class ExitDoor {
    int x,y;

    const int meterWidth = 5;
    const int spacer = 5;
    const float pulsesNeeded = 3f;
    int level = 0;
    float visualLevel = 0f;
    public bool canPulse = false;
    float timer = 0;
    float animframe = 0;
    const float maxTime = 40f;

    Rectangle chargerRect;
    Rectangle doorRect;
    Rectangle lekkyRect;

    Texture2D chargerTexture, doorTexture, doorOpenTexture, lekkyTexture;
    const int lekkywidth = 32, lekkyheight = 14;

    public ExitDoor(ContentManager Content, int x, int y) {
        this.x = x;
        this.y = y;

        // charger
        this.chargerTexture = Content.Load<Texture2D>("images/charger");
        this.chargerRect    = new Rectangle(x, y-this.chargerTexture.Height, this.chargerTexture.Width, this.chargerTexture.Height);

        // door
        this.doorTexture = Content.Load<Texture2D>("images/exit");
        this.doorOpenTexture = Content.Load<Texture2D>("images/exit-open");
        this.doorRect    = new Rectangle(
            x+chargerRect.Width+5*spacer,
            y-doorTexture.Height,
            doorTexture.Width,
            doorTexture.Height
        );

        // lekky
        this.lekkyTexture = Content.Load<Texture2D>("images/lekky");
        this.lekkyRect    = new Rectangle(
            chargerRect.X+chargerRect.Width,
            chargerRect.Y+chargerRect.Height/2-lekkyTexture.Height/4/2,
            doorRect.X-(chargerRect.X+chargerRect.Width),
            lekkyTexture.Height/4
        );
    }

    public void UpdatePulse(bool canPulse) {
        this.canPulse = canPulse;
    }

    public void Update(Rover r) {
        // sending pulses to charger
        if (level<pulsesNeeded && Kb.IsTapped(Ks.Interact) && new Rectangle(r.x,r.y,r.width,r.height).Intersects(chargerRect)) {
            if (canPulse) {
                canPulse = false;
                level++;
            } else {
                level = Math.Max(level-1,0);
            }
            timer = maxTime;
        }

        // exiting game
        if (level==pulsesNeeded && Kb.IsTapped(Ks.Interact) && new Rectangle(r.x,r.y,r.width,r.height).Intersects(doorRect)) {
            Environment.Exit(0);
        }

        // visually updating charger
        int actualLevel = (int)(level*doorRect.Height/pulsesNeeded);
        const float leeway = 0.5f;
        if (actualLevel-leeway < visualLevel && visualLevel < actualLevel+leeway) {
            visualLevel = actualLevel;
        }
        if (actualLevel<visualLevel) {
            visualLevel-=0.25f;
        } else if (actualLevel>visualLevel) {
            visualLevel+=0.25f;
        }

        // lekky opacity timer n animation frame
        animframe = (animframe + 0.25f) % 4;
        timer = Math.Max(timer-1,0);
    }

    public void Draw(SpriteBatch spriteBatch, int xoffset) {
        // charger
        spriteBatch.Draw(chargerTexture, new Vector2(chargerRect.X-xoffset, chargerRect.Y), Color.White);

        // door
        spriteBatch.Draw(level<pulsesNeeded ? doorTexture : doorOpenTexture, new Vector2(doorRect.X-xoffset, doorRect.Y), Color.White);

        // meter
        spriteBatch.FillRectangle(new Rectangle(doorRect.X+doorRect.Width+spacer-xoffset,y-doorRect.Height,meterWidth,doorRect.Height), Color.Gray);
        spriteBatch.FillRectangle(
            new Rectangle(doorRect.X+doorRect.Width+spacer-xoffset,y-(int)visualLevel,meterWidth,(int)visualLevel),
            level*doorRect.Height/pulsesNeeded<visualLevel ? Color.Red : Color.Yellow
        );

        // lekky
        int xi = lekkyRect.X-xoffset-1;
        int end = xi+lekkyRect.Width+1;
        while (xi<end) {
            spriteBatch.Draw(
                lekkyTexture,
                new Vector2(xi,lekkyRect.Y),
                new Rectangle(0,(int)(animframe)*lekkyRect.Height,Math.Min(end-xi,lekkyRect.Width),lekkyRect.Height),
                Color.White*(timer/maxTime),
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );
            xi+=lekkyRect.Width;
        }
    }
}