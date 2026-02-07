using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

namespace sojourner;

public enum Ks {
    Left, Right, Quit, Down, Interact
}

public static class Kb {
    private static Dictionary<Ks,Keys> keymap = new() {
        { Ks.Left, Keys.Left },
        { Ks.Right, Keys.Right },
        { Ks.Quit, Keys.Escape },
        { Ks.Down, Keys.Down },
        { Ks.Interact, Keys.Up }
    };

    private static KeyboardState prevState = Keyboard.GetState();
    private static KeyboardState currentState = Keyboard.GetState();
    private static MouseState prevMState = Mouse.GetState();
    private static MouseState currentMState = Mouse.GetState();
    
    public static void Update() {
        prevState = currentState;
        currentState = Keyboard.GetState();
        prevMState = currentMState;
        currentMState = Mouse.GetState();
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

    public static bool IsMouseClicked() {
        // Console.WriteLine($"{currentMState.LeftButton}");
        return currentMState.LeftButton.Equals(ButtonState.Pressed) && !prevMState.LeftButton.Equals(ButtonState.Pressed);
    }
}