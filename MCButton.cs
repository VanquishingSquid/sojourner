using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace sojourner;

public class MCButton {
    Action f;
    public int x,y,width,height;
    public string text;
    const float scale = 0.75f;

    public MCButton(int x, int y, string text, Action f, SpriteFont font) {
        this.x = x;
        this.y = y;
        this.text = text;
        this.f = f;

        // Console.WriteLine($"a");
        // Console.WriteLine($"{text}");
        // Console.WriteLine($"{font.MeasureString(text)}");
        Vector2 dims = font.MeasureString(text);
        this.width = (int)(dims.X*0.75);
        this.height = (int)(dims.Y*0.75);
    }

    public void Update() {
        float xm = Mouse.GetState().X;
        float ym = Mouse.GetState().Y;
        if (Kb.IsMouseClicked() && x<=xm && xm<=x+width && y<=ym && ym<=y+height) {
            // Console.WriteLine($"{Kb.IsMouseClicked()} | {x} <= {xm} <= {x+width} | {y} <= {ym} <= {y+height}");
            f();
        }
    }

    public void Draw(SpriteBatch _spriteBatch, SpriteFont font) {
        _spriteBatch.DrawString(font, text, new Vector2(x,y), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
