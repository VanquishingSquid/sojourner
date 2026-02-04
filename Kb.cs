using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

namespace sojourner;

public enum Ks {
    Left, Right, Quit, Down
}

public static class Kb {
    private static Dictionary<Ks,Keys> keymap = new() {
        { Ks.Left, Keys.Left },
        { Ks.Right, Keys.Right },
        { Ks.Quit, Keys.Escape },
        { Ks.Down, Keys.Down }
    };

    private static KeyboardState prevState = Keyboard.GetState();
    private static KeyboardState currentState = Keyboard.GetState();
    
    public static void Update() {
        prevState = currentState;
        currentState = Keyboard.GetState();
    }

    public static bool IsHeld(Ks key) {
        Keys k = keymap[key];
        return prevState.IsKeyDown(k) && currentState.IsKeyDown(k);
    }

    public static bool IsTapped(Ks key) {
        Keys k = keymap[key];
        return !prevState.IsKeyDown(k) && currentState.IsKeyDown(k);
    }

    public static bool IsDown(Ks key) {
        return currentState.IsKeyDown(keymap[key]);
    }
}