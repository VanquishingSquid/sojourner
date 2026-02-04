using System;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace sojourner;

public class SolidPlatform {
    public int width, height, x, y;

    public SolidPlatform(int x, int y, int width) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = 8;
    }

    public void Update() {
    }

    public void Draw(SpriteBatch _spriteBatch, int xoffset) {
        _spriteBatch.FillRectangle(new Rectangle(x-xoffset,y,width,height), Color.LightGray);
    }
}