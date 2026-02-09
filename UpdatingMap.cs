using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace sojourner;

public class UpdatingMap {
    int x,y;
    float width,height;
    int minx,maxx,miny,maxy;
    Texture2D roverTexture, warningTexture,bgTexture;
    List<((int,int),(int,int))> lines;
    float pixelsperunit;
    int roverx=0, rovery=0;
    float boxwidth,boxx,boxheight,boxy;

    public UpdatingMap(int x, int y, int width, int height, ContentManager Content, GraphicsDevice graphicsDevice) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.roverTexture = Content.Load<Texture2D>("images/rover");
        this.warningTexture = Content.Load<Texture2D>("images/warning");
        this.bgTexture = Content.Load<Texture2D>("images/platform-map");

        minx = int.MaxValue;
        maxx = int.MinValue;
        miny = int.MaxValue;
        maxy = int.MinValue;

        foreach (var item in PlatformData.All()) {
            minx = Math.Min(minx, Math.Min(item.Item1.Item1, item.Item2.Item1));
            miny = Math.Min(miny, Math.Min(item.Item1.Item2, item.Item1.Item2));
            maxx = Math.Max(maxx, Math.Max(item.Item1.Item1, item.Item2.Item1));
            maxy = Math.Max(maxy, Math.Max(item.Item1.Item2, item.Item1.Item2));
        }

        pixelsperunit = Math.Min((float)(width/(float)(maxx-minx)), (float)(height/(float)(maxy-miny)));
        boxwidth = (float)(maxx-minx)*pixelsperunit;
        boxx = ((float)x+width/2-boxwidth/2);
        boxheight = (maxy-miny)*pixelsperunit;
        boxy = (y+height/2-boxheight/2);

        lines = [];
        foreach (var item in PlatformData.All()) {
            int x1 = (int)(ComputeX(item.Item1.Item1));
            int y1 = (int)(ComputeY(item.Item1.Item2));
            int x2 = (int)(ComputeX(item.Item2.Item1));
            int y2 = (int)(ComputeY(item.Item2.Item2));
            lines.Add(((x1,y1),(x2,y2)));
        }

        UpdateRoverPos(roverx, rovery);
    }

    public void UpdateRoverPos(int rx, int ry) {
        roverx = (int)Math.Clamp(ComputeX((PlatformData.initx-rx)/PlatformData.unitSize), boxx,boxx+width);
        rovery = (int)Math.Clamp(ComputeY((PlatformData.inity-ry)/PlatformData.unitSize), boxy,boxy+height);
    }

    private int ComputeX(int px) {
        float fraction = (px-minx)/(float)(maxx-minx);
        return (int)(boxx+boxwidth-fraction*boxwidth);
    }

    private int ComputeY(int py) {
        float fraction = (py-miny)/(float)(maxy-miny);
        return (int)(boxy+boxheight-fraction*boxheight);
    }

    public void Draw(SpriteBatch spriteBatch) {
        spriteBatch.Draw(bgTexture, new Vector2(x-20,y+20), Color.White);

        // the actual platform map
        foreach (var line in lines) {
            spriteBatch.DrawLine(line.Item1.Item1, line.Item1.Item2, line.Item2.Item1, line.Item2.Item2, Color.Gray, 3);
        }

        // the rover
        float scale = 0.5f;
        spriteBatch.Draw(roverTexture, new Vector2(roverx, rovery-roverTexture.Height/2), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        // warning signs on fake platforms
        Action<((int,int),(int,int))> drawWarningSign = item => {
            int x1 = ComputeX(item.Item1.Item1);
            int y1 = ComputeY(item.Item1.Item2);
            int x2 = ComputeX(item.Item2.Item1);
            int y2 = ComputeY(item.Item2.Item2);
            int midX = (x1+x2)/2;
            int midY = (y1+y2)/2;
            spriteBatch.Draw(warningTexture, new Vector2(midX-warningTexture.Width/2, midY-warningTexture.Height/2), Color.White);
        };

        foreach (var item in PlatformData.xsysplatformsfake) {
            drawWarningSign(item);
        }

        foreach (var item in PlatformData.xsystrianglesfake) {
            drawWarningSign(item);
        }
    }
}