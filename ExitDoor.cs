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

    Rectangle chargerRect;
    Rectangle doorRect;

    Texture2D chargerTexture, doorTexture, doorOpenTexture;

    public ExitDoor(ContentManager Content, int x, int y) {
        this.x = x;
        this.y = y;

        // charger
        this.chargerTexture = Content.Load<Texture2D>("images/charger");
        this.chargerRect    = new Rectangle(x, y-this.chargerTexture.Height, this.chargerTexture.Height, this.chargerTexture.Width);

        // door
        this.doorTexture = Content.Load<Texture2D>("images/exit");
        this.doorOpenTexture = Content.Load<Texture2D>("images/exit-open");
        this.doorRect    = new Rectangle(
            x+chargerRect.Width+spacer+meterWidth+spacer,
            y-doorTexture.Height,
            doorTexture.Width,
            doorTexture.Height
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
    }

    public void Draw(SpriteBatch spriteBatch, int xoffset) {
        spriteBatch.Draw(chargerTexture, new Vector2(chargerRect.X-xoffset, chargerRect.Y), Color.White);
        spriteBatch.Draw(level<pulsesNeeded ? doorTexture : doorOpenTexture, new Vector2(doorRect.X-xoffset, doorRect.Y), Color.White);
        spriteBatch.FillRectangle(new Rectangle(x+chargerRect.Width+spacer-xoffset,y-doorRect.Height,meterWidth,doorRect.Height), Color.Gray);
        spriteBatch.FillRectangle(
            new Rectangle(x+chargerRect.Width+spacer-xoffset,y-(int)visualLevel,meterWidth,(int)visualLevel),
            level*doorRect.Height/pulsesNeeded<visualLevel ? Color.Red : Color.Yellow
        );
    }
}