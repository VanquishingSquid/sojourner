using System;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace sojourner;

public class SolidPlatform {
    public int width, height, x, y;
    public bool fake;
    Texture2D texture;
    Vector2 textureOffset;

    public SolidPlatform(ContentManager Content, int x, int y, int width, bool fake=false, Texture2D texture=null, Vector2? textureOffset=null) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = 8;
        this.fake = fake;

        if (texture==null) {
            this.texture = Content.Load<Texture2D>("images/platform");
            this.textureOffset = new Vector2(0,5-this.texture.Height);
        } else {
            this.texture = texture;
            if (textureOffset is Vector2 tO) {
                this.textureOffset = tO;
            } else {
                this.textureOffset = Vector2.Zero;
            }
        }
    }

    public void Update() {
    }

    public void Draw(SpriteBatch _spriteBatch, int xoffset) {
        int xi  = (int)(x+textureOffset.X-xoffset);
        int end = (int)(xi+width);
        while (xi<end) {
            _spriteBatch.Draw(
                texture,
                new Vector2(xi, y+textureOffset.Y),
                new Rectangle(0,0,Math.Min(texture.Width,end-xi),texture.Height),
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );

            xi += texture.Width;
        }
        
    }
}