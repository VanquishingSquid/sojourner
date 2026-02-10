using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace sojourner;

public class MovingBlock : SolidRect {
    Texture2D symbol, block;
    public MovingBlock(ContentManager Content, Texture2D symbol, int x, int y, int width, int height) : base(Content, x, y, width, height) {
        this.symbol = symbol;
        this.block = Content.Load<Texture2D>("images/moving-block");
    }

    public void Draw(SpriteBatch sb, int xoffset) {
        sb.Draw(block, new Vector2(x-xoffset,y), Color.White);
        sb.Draw(symbol, new Vector2(x-xoffset + block.Width/2 - symbol.Width/2, y + block.Height/2 - symbol.Height/2), Color.White);
    }
}
