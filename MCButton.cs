using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;

namespace sojourner;

public class MCButton {
    Action f;
    public int x,y,width,height;
    int textx,texty,textwidth,textheight;
    public string text;
    float scale;
    Texture2D? texture;

    public MCButton(int x, int y, string text, Action f, SpriteFont font, float scale=0.75f, Texture2D? texture=null) {
        this.x = x;
        this.y = y;
        this.text = text;
        this.f = f;
        this.scale = scale;

        Vector2 dims = font.MeasureString(text);
        this.width = (int)(dims.X*scale);
        this.height = (int)(dims.Y*scale);

        this.textx = x;
        this.texty = y;
        this.textwidth = width;
        this.textheight = height;

        this.texture = texture;
    }

    public void UpdateX(int x) {
        int diff = this.textx - this.x;
        this.x = x;
        this.textx = this.x + diff;
    }

    public void UpdateY(int y) {
        int diff = this.texty - this.y;
        this.y = y;
        this.texty = this.y + diff;
    }

    public void Update() {
        float xm = Mouse.GetState().X;
        float ym = Mouse.GetState().Y;
        if (Kb.IsMouseClicked() && x<=xm && xm<=x+width && y<=ym && ym<=y+height) {
            f();
        }
    }

    public void CenterText() {
        textx = (int)(x+width/2f-textwidth/2);
        texty = (int)(y+height/2f-textheight/2);
    }

    public void Draw(SpriteBatch _spriteBatch, SpriteFont font) {
        if (texture is Texture2D t) {
            _spriteBatch.Draw(t, new Vector2(x,y), Color.White);
        } else {
            _spriteBatch.FillRectangle(new RectangleF(x,y,width,height), Color.Beige);
        }

        _spriteBatch.DrawString(font, text, new Vector2(textx,texty), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
