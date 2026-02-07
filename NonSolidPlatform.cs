using System;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace sojourner;

public class NonSolidPlatform : SolidPlatform {
    int leeway = 0;

    public NonSolidPlatform(int x, int y, int width, int leeway) : base(x,y,width) {
        this.leeway = leeway;
    }
}