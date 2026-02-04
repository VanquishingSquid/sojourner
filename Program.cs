using System;
using System.Numerics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Program {
    static void Main(string[] args) {
        Game game;
        if (int.Parse(args[0])==0) {
            game = new sojourner.GameRover();
        } else {
            game = new sojourner.GameControl();
        }
        game.Run();
    }
}
