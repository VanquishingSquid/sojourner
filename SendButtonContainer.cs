using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace sojourner;

public class SendButtonContainer : ButtonContainer {
    public SendButtonContainer(int x, int y, int width, int height, SpriteFont font, int ydelta, int xdelta)
     : base(x,y,width,height,font,ydelta,xdelta,_=>{}) {
        onClick = OnClick;
    }

    public void OnClick(int i) {
        List<MCButton> tmps = [];
        for (int j=i+1;j<btns.Count;j++) {
            tmps.Add(btns[j]);
        }

        List<MCButton> tmps2 = btns.GetRange(0,i);
        btns = [];

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
        xw = x;
        yw = y;
        btns = [];
        return t;
    }
}
