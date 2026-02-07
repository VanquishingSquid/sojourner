using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace sojourner;

public class Rover {
    public int width, height, x, y, lastx, lasty;
    const int speed = 2;
    List<SolidRect> solids;
    List<SolidTriangle> triangles;
    List<bool> trianglesColl;
    List<SolidPlatform> platforms;
    float fallaccel = 0.5f;
    float fallspeed = 0f;
    int leeway = 3;
    int maxdeactivationtime = 5;
    int deactivationtime = 0;

    public Rover(List<SolidRect> solids, List<SolidTriangle> triangles, List<SolidPlatform> platforms) {
        x = lastx = 0;
        y = lasty = 0;
        width=32;
        height=width;
        this.solids = solids;
        this.triangles = triangles;
        this.platforms = platforms;
        trianglesColl = new bool[triangles.Count].ToList();
    }

    private void Move() {
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
                if (px<=p.X && p.X<=px+width /* intersection point is within x-bounds of rover */
                 && py<=p.Y && p.Y<=py+height /* intersection point is within y-bounds of rover */) {
                    if (new Rectangle(px,py,width,height).Intersects(new Rectangle(t.x,t.y,t.width,t.height))) {
                        return true;
                    }
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

    private float GetGradient(int x1, int y1, int x2, int y2) {
        return x1==x2 ? 0 : (y2-y1)/(x1-x2);
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
            for (int i = 0; i < triangles.Count; i++) {
                SolidTriangle t = triangles[i];
                // if (t.isslopeleft)
                //     Console.WriteLine($"leftslope: {t.isslopeleft} | colliding: {CollideWithSlope(x,y,t)} | coords: ({x},{y}) | prevcoords: ({lastx},{lasty}) | t_rect: ({t.x},{t.y},{t.width},{t.height}) | tgrad: {t.gradient} | mgrad: {GetGradient(lastx, lasty, x, y)}");
                if (CollideWithSlope(x,y,t) /* slope intersects rover */) {
                    float movementGradient = GetGradient(lastx, lasty, x, y);
                    if (!trianglesColl[i] /* we are not already colliding with the triangle */) {
                        if ((y+height<t.y+leeway) || (
                            !t.isslopeleft && (
                                (lastx<=x /* moving left->right */)
                             || (movementGradient > t.gradient /* falling faster than the slope */)
                            )
                         ) || (
                            t.isslopeleft && (
                                (lastx>=x /* moving right->left */)
                             || (movementGradient < t.gradient /* falling faster than the slope */)
                            )
                        )) {
                            // hence we shift to rest rover on triangle
                            trianglesColl[i]=false; // change variable to show we haven't collided
                            fallspeed=0;
                            y=t.GetCorrespondingY(x,x+width)-height;
                        } else {
                            trianglesColl[i]=true;
                        }
                    } else /* slope intersects rover but functions as nonsolid */ {
                        trianglesColl[i]=true;
                    }
                } else /* slope doesn't intersect rover */ {
                    trianglesColl[i]=false;
                }
            }

            for (int i = 0; i < platforms.Count; i++) {
                SolidPlatform p = platforms[i];
                if (CollideWithPlatform(x,y,p) && (
                    y+height<p.y+leeway
                 || lasty+height<p.y && y+height>p.y
                )) {
                        fallspeed=0;
                        y=p.y-height;
                }
            }
        } else {
            trianglesColl = [.. Enumerable.Repeat(true,triangles.Count)];
        }
    }

    public int Update() {
        Move();
        Collide();

        deactivationtime = (int)Math.Max(deactivationtime-1, 0);
        lasty = y;
        lastx = x;

        return x+width/2;
    }

    public void Draw(SpriteBatch _spriteBatch, int screenWidth) {
        // Console.WriteLine($"{x}, {y}");
        _spriteBatch.FillRectangle(new Rectangle(screenWidth/2-width/2,y,width,height), Color.Red);
    }
}