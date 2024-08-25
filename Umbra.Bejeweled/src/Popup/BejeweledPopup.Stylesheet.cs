using Una.Drawing;

namespace Umbra.Bejeweled.Popup;

internal sealed partial class BejeweledPopup
{
    private static Stylesheet Stylesheet { get; } = new(
        [
            new(
                "#Popup",
                new() {
                    Flow = Flow.Vertical,
                }
            ),
            new(
                "#Header",
                new() {
                    Flow        = Flow.Horizontal,
                    Size        = new(656, 50),
                    BorderColor = new() { Bottom = new("Window.Border") },
                    BorderWidth = new() { Bottom = 1 },
                }
            ),
            new(
                "#Title",
                new() {
                    Flow    = Flow.Vertical,
                    Size    = new(500, 50),
                    Padding = new(8),
                }
            ),
            new(
                "#Moves",
                new() {
                    Anchor = Anchor.MiddleCenter,
                    Flow   = Flow.Vertical,
                    Size   = new(0, 36),
                }
            ),
            new(
                "#MovesNumber",
                new() {
                    Anchor       = Anchor.TopCenter,
                    TextAlign    = Anchor.TopCenter,
                    FontSize     = 20,
                    Color        = new("Widget.PopupMenuText"),
                    OutlineColor = new("Widget.PopupMenuTextOutline"),
                    OutlineSize  = 1,
                    Padding      = new(0, 8),
                    Stretch      = true,
                    Margin       = new() { Top = 2 },
                }
            ),
            new(
                "#MovesLabel",
                new() {
                    Anchor    = Anchor.TopCenter,
                    TextAlign = Anchor.TopCenter,
                    FontSize  = 11,
                    Color     = new("Widget.PopupMenuTextMuted"),
                    Padding   = new(0, 8),
                    Stretch   = true,
                    Margin    = new() { Top = -2 },
                }
            ),
            new(
                "#Score",
                new() {
                    Flow = Flow.Horizontal,
                    Gap  = 2,
                }
            ),
            new(
                "#ScoreNumber",
                new() {
                    TextAlign    = Anchor.TopLeft,
                    FontSize     = 20,
                    Color        = new("Widget.PopupMenuText"),
                    OutlineColor = new("Widget.PopupMenuTextOutline"),
                    OutlineSize  = 1,
                }
            ),
            new(
                "#ScoreMultiplier",
                new() {
                    TextAlign    = Anchor.TopLeft,
                    FontSize     = 12,
                    Color        = new("Widget.PopupMenuTextMuted"),
                    OutlineColor = new("Widget.PopupMenuTextOutline"),
                    OutlineSize  = 1,
                    Padding      = new() { Top = 6 },
                }
            ),
            new(
                "#HiScore",
                new() {
                    Flow         = Flow.Vertical,
                    Size         = new(500, 0),
                    TextAlign    = Anchor.MiddleLeft,
                    FontSize     = 12,
                    Color        = new("Widget.PopupMenuTextMuted"),
                    OutlineColor = new("Widget.PopupMenuTextOutline"),
                    OutlineSize  = 1,
                }
            ),
            new(
                "#Buttons",
                new() {
                    Anchor  = Anchor.MiddleRight,
                    Padding = new(8),
                    Gap     = 8,
                }
            ),
            new(
                "#Game",
                new() {
                    Flow    = Flow.Vertical,
                    Size    = new(656, 530),
                    Padding = new(8),
                }
            ),
            new(
                ".row",
                new() { }
            ),
            new(
                ".cell",
                new() {
                    Size        = new(64, 64),
                    StrokeColor = new("Window.Border"),
                    StrokeWidth = 1,
                }
            ),
            new(
                ".cell:hover",
                new() {
                    StrokeColor = new(0xFFFFFFFF),
                    StrokeWidth = 1,
                }
            ),
            new(
                ".cell.selected",
                new() {
                    StrokeColor = new(0xFFFFFFFF),
                    StrokeWidth = 3,
                }
            ),
            new(
                ".cell.targeted",
                new() {
                    StrokeColor = new(0xA0FFFFFF),
                    StrokeWidth = 2,
                }
            ),
            new(
                "#GameOver",
                new() {
                    Flow   = Flow.Vertical,
                    Anchor = Anchor.MiddleCenter,
                }
            ),
            new(
                "#GameOverText",
                new() {
                    Anchor       = Anchor.TopCenter,
                    TextAlign    = Anchor.TopCenter,
                    FontSize     = 32,
                    Color        = new("Widget.PopupMenuText"),
                    OutlineColor = new("Widget.PopupMenuTextOutline"),
                    OutlineSize  = 1,
                    Padding      = new(8),
                    BorderColor  = new() { Bottom = new("Window.Border") },
                    BorderWidth  = new() { Bottom = 2 },
                    Size         = new(250, 0),
                    Margin       = new() { Bottom = 16 },
                }
            ),
            new(
                "#GameOverScore",
                new() {
                    Anchor       = Anchor.TopCenter,
                    TextAlign    = Anchor.TopCenter,
                    FontSize     = 18,
                    Color        = new("Widget.PopupMenuText"),
                    OutlineColor = new("Widget.PopupMenuTextOutline"),
                    OutlineSize  = 1,
                    Padding      = new(2),
                    Size         = new(250, 0),
                }
            ),
            new(
                "#GameOverHiScore",
                new() {
                    Anchor       = Anchor.TopCenter,
                    TextAlign    = Anchor.TopCenter,
                    FontSize     = 14,
                    Color        = new("Widget.PopupMenuTextMuted"),
                    OutlineColor = new("Widget.PopupMenuTextOutline"),
                    OutlineSize  = 1,
                    Padding      = new(2) { Bottom = 16 },
                    Size         = new(250, 0),
                }
            )
        ]
    );
}
