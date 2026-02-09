using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace sojourner;

public abstract class ButtonContainer {
    public int x,y,width,height,xw,yw,ydelta,xdelta;
    protected List<MCButton> btns = [];
    const float scale = 1f;
    SpriteFont font;
    protected Color color = Color.Blue;
    protected Action<int> onClick;

    public ButtonContainer(int x, int y, int width, int height, SpriteFont font, int ydelta, int xdelta, Action<int> onClick) {
        this.x = x;
        this.y = y;
        this.xw = x;
        this.yw = y;
        this.width = width;
        this.height = height;
        this.font = font;
        this.ydelta = ydelta;
        this.xdelta = xdelta;
        this.onClick = onClick;
    }

    public void Update() {
        foreach (var btn in btns) {
            btn.Update();
        }
    }

    public void Reset() {
        this.btns = [];
        this.xw = x;
        this.yw = y;
    }

    public void AddButton(string w) {
        int n = btns.Count;
        MCButton btn = new(xw,yw,w,()=>{onClick(n);},font,scale);

        if (xw+btn.width>x+width) {
            if (yw+2*ydelta>y+height) {
                return;
            }
            yw+=ydelta;
            xw=x;
            btn.UpdateX(xw);
            btn.UpdateY(yw);
            xw+=btn.width+xdelta;
        } else {
            xw+=btn.width+xdelta;
        }

        btns.Add(btn);
    }

    public void Draw(SpriteBatch _spriteBatch) {
        _spriteBatch.FillRectangle(new RectangleF(x,y,width,height),color*0.5f);

        foreach (var btn in btns) {
            btn.Draw(_spriteBatch, font);
        }
    }
}
