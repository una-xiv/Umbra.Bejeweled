using System;

namespace Umbra.Bejeweled.Game;

public readonly struct Vec2(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;

    public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);

    public static bool operator ==(Vec2 a, Vec2 b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vec2 a, Vec2 b) => a.X != b.X || a.Y != b.Y;

    public override bool Equals(object? o) => o is Vec2 v && v == this;
    public override int GetHashCode() => HashCode.Combine(X, Y);
}
