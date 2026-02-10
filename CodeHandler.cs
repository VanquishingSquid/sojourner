using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace sojourner;

public class CodeHandler {
    Texture2D texture;
    Vector2 origin = new Vector2(0,80);
    
    public CodeHandler(ContentManager Content) {
        texture = Content.Load<Texture2D>("images/code-manual");
    }

    public void Draw(SpriteBatch sb) {
        sb.Draw(texture, origin, Color.White);

        List<Vector2> startpos = [
            new(150,208),
            new(150,285),
            new(150,350),
            new(150,415),
            new(150,484),
            new(548,208),
            new(548,285),
            new(548,350),
            new(548,415),
            new(548,484),
        ];

        const int width = 20;
        for (int i = 0; i < Codes.codeObjects.Count; i++) {
            Vector2 pos = origin+startpos[i];
            foreach (var item in Codes.codeObjects[i]) {
                sb.FillRectangle(new RectangleF(pos.X, pos.Y, width, width), item ? Color.Green : Color.Red);
                pos.X += width+5;
            }
        }
    }
}