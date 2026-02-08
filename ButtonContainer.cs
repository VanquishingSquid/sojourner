using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace sojourner;

public class ButtonContainer {
    public int x,y,width,height,xw,yw,ydelta,xdelta;
    List<MCButton> btns = [];
    const float scale = 1f;
    SpriteFont font;

    public ButtonContainer(int x, int y, int width, int height, SpriteFont font, int ydelta, int xdelta) {
        this.x = x;
        this.y = y;
        this.xw = x;
        this.yw = y;
        this.width = width;
        this.height = height;
        this.font = font;
        this.ydelta = ydelta;
        this.xdelta = xdelta;
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
        MCButton btn = new(xw,yw,w,()=>{RemoveButton(n);},font,scale);

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

    public void RemoveButton(int i) {
        List<MCButton> tmps = [];
        for (int j=i+1;j<btns.Count;j++) {
            tmps.Add(btns[j]);
        }

        List<MCButton> tmps2 = btns.GetRange(0,i);
        btns = [];

        // reset xw and yw
        xw = x;
        yw = y;
    
        tmps.AddRange(tmps2);
        foreach (var btn in tmps) {
            AddButton(btn.text);
        }
    }

    public string CreateMessage() {
        string t = "message ";
        foreach (var btn in btns) {
            t += btn.text + " ";
        }
        btns = [];
        return t;
    }

    public void Draw(SpriteBatch _spriteBatch) {
        _spriteBatch.FillRectangle(new RectangleF(x,y,width,height),Color.Blue*0.5f);

        foreach (var btn in btns) {
            btn.Draw(_spriteBatch, font);
        }
    }
}
