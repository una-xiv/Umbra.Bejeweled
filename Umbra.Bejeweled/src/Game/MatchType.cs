namespace Umbra.Bejeweled.Game;

public enum MatchType
{
    /// <summary>
    /// A match of 5 gems in either axis.
    /// </summary>
    Rainbow = 6,

    /// <summary>
    /// A T-shape match.
    /// </summary>
    TeeShape = 5,

    /// <summary>
    /// A square 2x2 match.
    /// </summary>
    Bomb = 4,

    /// <summary>
    /// Horizontal rocket
    /// </summary>
    HorizontalRocket = 3,

    /// <summary>
    /// Vertical rocket
    /// </summary>
    VerticalRocket = 2,

    /// <summary>
    /// Default match of 3 gems.
    /// </summary>
    Default = 1,

    /// <summary>
    /// No match.
    /// </summary>
    None = 0
}
