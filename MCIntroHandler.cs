using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace sojourner;

public class MCIntroHandler {
    Texture2D texture;
    Texture2D earthTexture;
    int x,y,width,height;
    float earthFrame = 0;

    public MCIntroHandler(ContentManager Content, int x, int y, int width, int height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.texture = Content.Load<Texture2D>("images/intro-manual");
        this.earthTexture = Content.Load<Texture2D>("images/spinning-earth");
    }

    public void Update() {
        earthFrame+=0.1f;
        if (earthFrame>=94) {
            earthFrame=0;
        }
    }

    public void Draw(SpriteBatch spriteBatch) {
        spriteBatch.Draw(texture, new Vector2(x,y), Color.White);
        
        spriteBatch.Draw(
            earthTexture,
            new Vector2(x+width-60, y+height-60),
            new Rectangle(0,48*(int)earthFrame,48,48),
            Color.White
        );
    }
}