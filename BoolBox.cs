using System;
using System.Data;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace sojourner;

public class BoolBox {
    int x,y,width,height;
    public bool status = false;
    Microsoft.Xna.Framework.Color onColour = Microsoft.Xna.Framework.Color.Green;
    Microsoft.Xna.Framework.Color offColour = Microsoft.Xna.Framework.Color.Red;
    Texture2D codeButtonGreen, codeButtonRed;

    public BoolBox(ContentManager Content, int x, int y, int width, int height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        codeButtonGreen = Content.Load<Texture2D>("images/code-button-green");
        codeButtonRed   = Content.Load<Texture2D>("images/code-button-red");
    }

    public void Update(Rover r) {
        if (new System.Drawing.Rectangle(x,y,width,height).IntersectsWith(new System.Drawing.Rectangle(r.x, r.y, r.width, r.height))
            && Kb.IsTapped(Ks.Interact)) {
                status = !status;
        }
    }

    public void Draw(SpriteBatch _spriteBatch, int xoffset) {
        // _spriteBatch.FillRectangle(new Microsoft.Xna.Framework.Rectangle(x-xoffset,y,width,height), status ? onColour : offColour);
        _spriteBatch.Draw(status ? codeButtonGreen : codeButtonRed, new Microsoft.Xna.Framework.Rectangle(x-xoffset,y,width,height), Microsoft.Xna.Framework.Color.White);
    }
}