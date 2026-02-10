using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace sojourner;

public class DecorHandler {
    Texture2D bg,dust;
    int screenWidth, screenHeight;
    List<(int,int)> frontDusts = [], backDusts = [];
    const int xSpeed = 5, ySpeed = 1;
    Random r = new();

    public DecorHandler(ContentManager Content, int screenWidth,int screenHeight) {
        bg = Content.Load<Texture2D>("images/bg");
        dust = Content.Load<Texture2D>("images/dust");
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
    }

    public void DrawBg(SpriteBatch sb, int xoffset) {
        // background
        int x = (int)(-0.6f*xoffset%screenWidth);
        sb.Draw(bg, new Vector2(x,0), Color.White);
        sb.Draw(bg, new Vector2(x-bg.Width,0), Color.White);
        sb.Draw(bg, new Vector2(x+bg.Width,0), Color.White);
    }

    public void DrawFg(SpriteBatch sb, int xoffset) {
        // move and display dust
        List<(int,int)> tmp = [];
        foreach (var item in frontDusts) {
            int x = item.Item1;
            int y = item.Item2;
            sb.Draw(dust, new Vector2(x-xoffset,y), Color.White);
            // if (x<screenWidth-xoffset && y<screenHeight) {
                tmp.Add((x+xSpeed, y+ySpeed));
            // }
        }
        frontDusts = tmp;


        // generate new dust
        if (r.NextDouble()>0.5) {
            frontDusts.Add((xoffset+(int)(r.NextDouble()*screenWidth),-dust.Height));
        } else {
            frontDusts.Add((xoffset-dust.Width,(int)(r.NextDouble()*screenHeight)));
        }
    }
}