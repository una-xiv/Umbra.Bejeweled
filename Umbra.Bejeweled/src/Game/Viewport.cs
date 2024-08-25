using System.Numerics;

namespace Umbra.Bejeweled.Game;

public class Viewport()
{
    public Vector2 TopLeft     { get; set; } = Vector2.Zero;
    public Vector2 BottomRight { get; set; } = Vector2.One;

    public void Update(Vector2 topLeft, Vector2 bottomRight)
    {
        TopLeft     = topLeft;
        BottomRight = bottomRight;
    }
}
