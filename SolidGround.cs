using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace sojourner;

public class SolidGround : SolidRect {
    Texture2D texture;

    public SolidGround(int x, int y, int width, int height, ContentManager Content) : base(Content, x,y,width,height) {
        texture = Content.Load<Texture2D>("images/ground");
    }

    public new void Draw(SpriteBatch sb, int xoffset) {
        int xi = x-xoffset;
        while (xi<x-xoffset+width) {
            sb.Draw(texture, new Vector2(xi,y-2), Color.White);
            xi += texture.Width;
        }
    }
}