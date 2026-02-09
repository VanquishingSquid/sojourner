using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace sojourner;

class Word {
    public int x,y,width,height;
    public string w;

    public Word(int x, int y, SpriteFont font, string w) {
        var m = font.MeasureString(w);
        this.x = x;
        this.y = y;
        this.w = w;

        this.width = (int)m.X;
        this.height = (int)m.Y;
    }
}

public class WordContainer {
    int x,y,width,height;
    int xw,yw;
    int xdelta = 5, ydelta = 5;
    List<Word> words;
    SpriteFont font;

    public WordContainer(int x, int y, int width, int height, SpriteFont font) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.font = font;

        this.xw = x;
        this.yw = y;

        words = [];
    }

    public void NewMessage(List<string> ws) {
        words = [];
        yw = y;
        xw = x;
        foreach (var item in ws) {
            Word w = new Word(xw,yw,font,item);

            if (xw+w.width>x+width) {
                if (yw+2*ydelta>y+height) {
                    return;
                }
                yw+=ydelta+w.height;
                xw=x;
                w.x=xw;
                w.y=yw;
                xw+=w.width+xdelta;
            } else {
                xw+=w.width+xdelta;
            }

            words.Add(w);
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        foreach (var item in words) {
            spriteBatch.DrawString(font,item.w,new Vector2(item.x,item.y), Color.Black);
        }
    }
}