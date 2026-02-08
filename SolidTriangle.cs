using System;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace sojourner;

public class SolidTriangle {
    public int width, height, x, y;
    public bool isslopeleft;
    public float gradient;
    public Vector2 toppoint, botpoint;
    public bool fake;

    public SolidTriangle(int x, int y, int width, int height, bool isslopeleft, bool fake=false) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.isslopeleft = isslopeleft;
        this.toppoint = new Vector2(isslopeleft ? x : x+width, y);
        this.botpoint = new Vector2(isslopeleft ? x+width : x, y+height);
        this.gradient = isslopeleft ? -height/width : height/width;
        this.fake = fake;
    }

    public void Update() {
    }

    public void Draw(SpriteBatch _spriteBatch, int xoffset) {
        // Vector2 toppointlocal = isslopeleft ? new Vector2(0, 0) : new Vector2(width, 0);
        // Vector2[] trianglePoints = [
        //     new Vector2(width, height),
        //     new Vector2(0, height),
        //     toppointlocal,
        // ];
        int x1=x;
        int y1=isslopeleft ? y : y+height;
        int x2=x+width;
        int y2=isslopeleft ? y+height : y;

        // _spriteBatch.DrawPolygon(new Vector2(x-xoffset, y), trianglePoints, fake ? Color.LightGray : Color.LightGray, 3f);
        _spriteBatch.DrawLine(x1-xoffset,y1,x2-xoffset,y2,Color.LightGray,6f,0f);
    }

    public Vector2? IntersectionPoint(Vector2 p1, Vector2 p2) {
        float A1 = toppoint.Y-botpoint.Y;
        float B1 = botpoint.X-toppoint.X;
        float A2 = p1.Y-p2.Y;
        float B2 = p2.X-p1.X;
        
        float delta = A1*B2-A2*B1;
        if (delta == 0) { return null; }

        float C1 = A1*botpoint.X+B1*botpoint.Y;
        float C2 = A2*p2.X+B2*p2.X;
        return new Vector2((B2*C1-B1*C2)/delta, (A1*C2-A2*C1)/delta);
    }

    public int GetCorrespondingY(int x1, int x2) {
        float xrange = toppoint.X-botpoint.X;
        float yrange = toppoint.Y-botpoint.Y;        
        float y1 = botpoint.Y + (x1-botpoint.X)/xrange * yrange;
        float y2 = botpoint.Y + (x2-botpoint.X)/xrange * yrange;
        
        if (isslopeleft) {
            return x1<x ? y : (int)y1;
        } else {
            return x2>x+width ? y : (int)y2;
        }
    }
}