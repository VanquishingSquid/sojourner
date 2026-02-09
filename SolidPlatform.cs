using System;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace sojourner;

public class SolidPlatform {
    public int width, height, x, y;
    public bool fake;
    Texture2D? texture;
    Vector2? textureOffset;

    public SolidPlatform(int x, int y, int width, bool fake=false, Texture2D texture=null, Vector2? textureOffset=null) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = 8;
        this.fake = fake;
        this.texture=texture;
        this.textureOffset=textureOffset;
    }

    public void Update() {
    }

    public void Draw(SpriteBatch _spriteBatch, int xoffset) {
        if (texture is Texture2D t) {
            if (textureOffset is Vector2 o) {
                _spriteBatch.Draw(t, new Vector2(x+o.X-xoffset,y+o.Y), Color.White);
            } else {
                _spriteBatch.Draw(t, new Vector2(x-xoffset,y), Color.White);
            }
        } else {
            _spriteBatch.FillRectangle(new Rectangle(x-xoffset,y,width,height), fake ? Color.LightGray : Color.LightGray);
        }
    }
}