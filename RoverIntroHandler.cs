using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace sojourner;

public class RoverIntroHandler {
    float frame = 0;
    Texture2D introscreen;
    Texture2D spinningRover;
    public bool isActive = true;

    public RoverIntroHandler(ContentManager Content) {
        introscreen = Content.Load<Texture2D>("images/intro-screen");
        spinningRover = Content.Load<Texture2D>("images/rover-gif");
    }

    public void Draw(SpriteBatch spriteBatch) {
        spriteBatch.Draw(introscreen, Vector2.Zero, Color.White);

        frame = (frame + 0.2f) % 26;
        spriteBatch.Draw(
            spinningRover,
            new Vector2(963, 624),
            new Rectangle(0,52*(int)frame,52,52),
            Color.White
        );

        if (Kb.IsTapped(Ks.Interact)) {
            isActive = false;
        }
    }
}

// 963 624