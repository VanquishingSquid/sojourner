using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using Gum.DataTypes;
using Microsoft.Xna.Framework.Content;


namespace sojourner;

public class PowerSystem {
    int switchX = -2950, switchY = 230, switchWidth = 30, switchHeight = 30;
    bool power = false;
    int timer = 0;
    // SolidRect movingPlatform = new SolidRect(1033,670,150,100);
    SolidPlatform movingPlatform;
    Texture2D switchTextureOn, switchTextureOff;
    
    public PowerSystem(SolidPlatform p, ContentManager Content) {
        movingPlatform = p;
        switchTextureOff = Content.Load<Texture2D>("images/switch-off");
        switchTextureOn  = Content.Load<Texture2D>("images/switch-on");
    }

    public void Update(Rover r) {
        if (Kb.IsTapped(Ks.Interact) && new Rectangle(switchX,switchY,switchWidth,switchHeight).Intersects(new Rectangle(r.x,r.y,r.width,r.height))) {
            power = true;
        }

        if (power) {
            timer++;
        }

        movingPlatform.y = (int)(100*Math.Cos(timer/100f)+570);
        // movingPlatform.y = (int)(100*Math.Cos(timer/100f)+770);
    }

    public void Draw(SpriteBatch _spriteBatch, int xoffset) {
        _spriteBatch.Draw(power ? switchTextureOn : switchTextureOff, new Vector2(switchX-xoffset, switchY), Color.White);
    }
}