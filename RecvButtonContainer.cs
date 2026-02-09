using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sojourner;

public class RecvButtonContainer : ButtonContainer {
    SendButtonContainer toSendWs;

    public RecvButtonContainer(int x, int y, int width, int height, SpriteFont font, int ydelta, int xdelta, SendButtonContainer toSendWs)
        : base(x,y,width,height,font,ydelta,xdelta,_=>{}) {
        this.toSendWs = toSendWs;
        this.onClick = OnClick;
        color = Color.Red;
    }

    public void HandleRoverMsg(string s) {
        List<string> newws = s.Split(' ').ToList();
        newws.AddRange(this.btns.Select(btn=>btn.text));
        Reset();
        foreach (var item in newws) {
            AddButton(item);
        }
        
        /*
        int wx = x;
        int wy = y;

        for (int i=0;i<newws.Count;i++) {
            string w = newws[i];
            MCButton btn = new(wx,wy,w,()=>{toSendWs.AddButton(w);}, font,1f);


            if (wx+btn.width>screenWidth-10) {
                wx=wordButtonStartX;
                wy+=yWordSep;
                btn.x=wx;
                btn.y=wy;
            } else {
                wx+=btn.width+xWordSep;
            }
            
            newbtns.Add(btn);
        }
        
        btns = newbtns;
        */
    }

    public void OnClick(int i) {
        toSendWs.AddButton(btns[i].text);
    }

    // public override void OnClick(int i) {
    //     List<MCButton> tmps = [];
    //     for (int j=i+1;j<btns.Count;j++) {
    //         tmps.Add(btns[j]);
    //     }

    //     List<MCButton> tmps2 = btns.GetRange(0,i);
    //     btns = [];

    //     // reset xw and yw
    //     xw = x;
    //     yw = y;
    
    //     tmps.AddRange(tmps2);
    //     foreach (var btn in tmps) {
    //         AddButton(btn.text);
    //     }
    // }

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
