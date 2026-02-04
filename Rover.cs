using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace sojourner;

public class Rover {
    int width, height, x, y, lastx, lasty;
    const int speed = 1;
    List<SolidRect> solids;
    List<SolidTriangle> triangles;
    List<SolidPlatform> platforms;
    float fallaccel = 0.5f;
    float fallspeed = 0f;
    int leeway = 1;
    int maxdeactivationtime = 30;
    int deactivationtime = 0;

    public Rover(List<SolidRect> solids, List<SolidTriangle> triangles, List<SolidPlatform> platforms) {
        x = lastx = 0;
        y = lasty = 0;
        width=16;
        height=16;
        this.solids = solids;
        this.triangles = triangles;
        this.platforms = platforms;
    }

    private void Move() {
        lasty = y;
        lastx = x;

        // player controlled movement
        if (Kb.IsDown(Ks.Left)) {
            x-=speed;
        } else if (Kb.IsDown(Ks.Right)) {
            x+=speed;
        }
        
        if (Kb.IsDown(Ks.Down)) {
            deactivationtime=maxdeactivationtime;
        }

        // falling
        y+=(int)fallspeed;
        fallspeed+=fallaccel;
    }

    private bool CollideWithSlope(int px, int py, SolidTriangle t) {
        Vector2[][] lines = [
            [new(px, py), new(px + width, py)],
            [new(px, py), new(px, py + height)],
            [new(px + width, py), new(px + width, py + height)],
            [new(px, py + height), new(px + width, py + height)]
        ];
        foreach (Vector2[] vs in lines) {
            Vector2? point = t.IntersectionPoint(vs[0], vs[1]);
            if (point is Vector2 p) {
                if (px<=p.X && p.X<=px+width && py<=p.Y && p.Y<=py+height) {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CollideWithPlatform(int x, int y, SolidPlatform p) {
        if (p.x<=x+width && x<=p.x+p.width) {
            return y<=p.y && p.y<=y+height;
        }
        return false;
    }

    private void Collide() {
        foreach (SolidRect s in solids) {
            // Console.WriteLine($"testing collision | rover y: {y}->{y+height} | shape y: {s.y}->{s.y+s.height} | fallspeed: {fallspeed} | fallaccel: {fallaccel}");
            bool horizontalcol = (s.x<x && x<s.x+s.width) || (s.x<x+width && x+width<s.x+s.width);
            bool verticalcol   = (s.y<y && y<s.y+s.height) || (s.y<y+height && y+height<s.y+s.height);
            if (horizontalcol && verticalcol) {
                // Console.WriteLine("colliding");
                // figure out how far in u r from each dir
                int fromleft  = x+width-s.x;
                int fromright = s.x+s.width-x;
                int fromtop   = y+height-s.y;
                int frombot   = s.y+s.height-y;

                if (fromleft<=fromright && fromleft<=fromtop && fromleft<=fromright) {
                    // left
                    // Console.WriteLine("left");
                    x = s.x-width;
                } else if (fromright<=fromtop && fromright<=frombot) {
                    // right
                    // Console.WriteLine("right");
                    x = s.x+s.width;
                } else if (fromtop<=frombot) {
                    // top
                    // Console.WriteLine("top");
                    y = s.y-height;
                    fallspeed=0f;
                } else {
                    // bot
                    // Console.WriteLine("bot");
                    y = s.y+s.height;
                }
            }
        }

        if (deactivationtime == 0) {
            foreach (SolidTriangle s in triangles) {
                // Console.WriteLine($"now collide? {CollideWithSlope(x,y,s)} | earlier collide? {CollideWithSlope(lastx-leeway,lasty-leeway,s)} | y: {y} | lasty: {lasty}");
                if (CollideWithSlope(x,y,s) && !CollideWithSlope(lastx-leeway,lasty-leeway,s) && y>=lasty-leeway) {
                    y=s.GetCorrespondingY(x,x+width)-height;
                    fallspeed=0;
                    // Console.WriteLine($"right after: {x}, {y}");
                }
            }

            foreach (SolidPlatform s in platforms) {
                if (y>=lasty-leeway && CollideWithPlatform(x,y,s) && !CollideWithPlatform(lastx-leeway,lasty-leeway,s)) {
                    fallspeed=0;
                    y=s.y-height;
                }
            }
        }
    }

    public int Update() {
        Move();
        Collide();

        deactivationtime = (int)Math.Max(deactivationtime-1, 0);
        return x+width/2;
    }

    public void Draw(SpriteBatch _spriteBatch, int screenWidth) {
        // Console.WriteLine($"{x}, {y}");
        _spriteBatch.FillRectangle(new Rectangle(screenWidth/2-width/2,y,width,height), Color.Red);
    }
}