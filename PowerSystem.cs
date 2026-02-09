using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using Gum.DataTypes;
using Microsoft.Xna.Framework.Content;


namespace sojourner;

public class PowerSystem {
    int switchX = -2950, switchY = 770-20*25, switchWidth, switchHeight;
    bool power = false;
    int timer = 0;
    SolidPlatform movingPlatform;
    Texture2D switchTextureOn, switchTextureOff;
    Texture2D liftTextureOn, liftTextureOff;
    
    public PowerSystem(SolidPlatform p, ContentManager Content) {
        movingPlatform = p;
        switchTextureOff = Content.Load<Texture2D>("images/switch-off");
        switchTextureOn  = Content.Load<Texture2D>("images/switch-on");
        liftTextureOff   = Content.Load<Texture2D>("images/lift-off");
        liftTextureOn    = Content.Load<Texture2D>("images/lift-on");

        switchWidth = switchTextureOff.Width;
        switchHeight = switchTextureOff.Height;
    }

    public void Update(Rover r) {
        if (Kb.IsTapped(Ks.Interact) && new Rectangle(switchX,switchY-switchHeight,switchWidth,switchHeight).Intersects(new Rectangle(r.x,r.y,r.width,r.height))) {
            power = true;
        }

        if (power) {
            timer++;
        }

        movingPlatform.y = (int)(50*Math.Cos(timer/100f)+620);
    }

    public void Draw(SpriteBatch _spriteBatch, int xoffset) {
        _spriteBatch.Draw(power ? switchTextureOn : switchTextureOff, new Vector2(switchX-xoffset, switchY-switchHeight), Color.White);
        _spriteBatch.Draw(power ? liftTextureOn : liftTextureOff, new Vector2(1033-xoffset,470), Color.White);
    }
}