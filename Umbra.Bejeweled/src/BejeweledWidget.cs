using System.Collections.Generic;
using Umbra.Bejeweled.Game;
using Umbra.Bejeweled.Popup;
using Umbra.Common;
using Umbra.Widgets;

namespace Umbra.Bejeweled;

[ToolbarWidget("Bejeweled", "Bejeweled", "A mini-game of Bejeweled to play while you wait for your queue to pop.")]
internal sealed class BejeweledWidget(
    WidgetInfo                  info,
    string?                     guid         = null,
    Dictionary<string, object>? configValues = null
)
    : StandardToolbarWidget(info, guid, configValues)
{
    /// <inheritdoc/>
    public override BejeweledPopup Popup { get; } = new();

    protected override StandardWidgetFeatures Features => 
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.Icon |
        StandardWidgetFeatures.CustomizableIcon;

    /// <inheritdoc/>
    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            ..base.GetConfigVariables(),
            
            new BooleanWidgetConfigVariable(
                "EnableSound",
                "Enable Sound Effects",
                "Whether to enable sound effects.",
                true
            ) { Category = "Game Options" },
            new IntegerWidgetConfigVariable(
                "Difficulty",
                "Difficulty",
                "The difficulty level of the game. Higher values increase the number of gem types. Changing this value will reset the game.",
                2,
                1,
                4
            ) { Category = "Game Options" },
            new IntegerWidgetConfigVariable(
                "GemIcon1",
                "Icon 1",
                "The icon ID for the first gem type.",
                21275,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "GemIcon2",
                "Icon 2",
                "The icon ID for the second gem type.",
                21281,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "GemIcon3",
                "Icon 3",
                "The icon ID for the third gem type.",
                21283,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "GemIcon4",
                "Icon 4",
                "The icon ID for the fourth gem type.",
                21284,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "GemIcon5",
                "Icon 5",
                "The icon ID for the fifth gem type.",
                21289,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "GemIcon6",
                "Icon 6",
                "The icon ID for the sixth gem type.",
                21293,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "BombIcon",
                "Bomb Icon",
                "The icon ID for the bomb power-up.",
                60728,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "HorizontalRocketIcon",
                "Horizontal Rocket Icon",
                "The icon ID for the horizontal rocket power-up.",
                60727,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "VerticalRocketIcon",
                "Vertical Rocket Icon",
                "The icon ID for the vertical rocket power-up.",
                60726,
                0
            ) { Category = "Gem Icons" },
            new IntegerWidgetConfigVariable(
                "RainbowBombIcon",
                "Rainbow Bomb Icon",
                "The icon ID for the Rainbow Bomb power-up.",
                60722,
                0
            ) { Category = "Gem Icons" },
            new StringWidgetConfigVariable(
                "ButtonLabel",
                "Button Label",
                "The label for the button.",
                Info.Name,
                4096,
                true
            ) { Category = I18N.Translate("Widget.ConfigCategory.WidgetAppearance") },
            new IntegerWidgetConfigVariable("HiScore", "", null, 0, 0) { IsHidden = true },
            new StringWidgetConfigVariable("Data", "", null, "", short.MaxValue) { IsHidden = true },
        ];
    }

    private uint _lastHiScore;
    private int  _lastDifficulty;

    /// <inheritdoc/>
    protected override void OnLoad()
    {
        SetText(Info.Name);

        _lastHiScore    = (uint)GetConfigValue<int>("HiScore");
        _lastDifficulty = GetConfigValue<int>("Difficulty");

        Popup.HiScore          = _lastHiScore;
        Popup.Board.ColorCount = 2 + _lastDifficulty;

        Popup.ResetGame();

        Popup.OnPopupOpen  += OnPopupOpened;
        Popup.OnPopupClose += OnPopupClosed;
    }

    protected override void OnUnload()
    {
        Popup.OnPopupOpen  -= OnPopupOpened;
        Popup.OnPopupClose -= OnPopupClosed;
    }

    public override string GetInstanceName()
    {
        return $"Bejeweled - {GetConfigValue<string>("ButtonLabel")}";
    }

    protected override void OnDraw()
    {
        SetText(GetConfigValue<string>("ButtonLabel"));

        int difficulty = GetConfigValue<int>("Difficulty");
        if (difficulty != _lastDifficulty) {
            _lastDifficulty = difficulty;
            Popup.Board.ColorCount = 2 + difficulty;
            Popup.Board.Reset();
        }

        Popup.Sound                          = GetConfigValue<bool>("EnableSound");
        Popup.Board.EnableSfx                = Popup.Sound;
        Popup.Board.IconIds.GemType1         = (uint)GetConfigValue<int>("GemIcon1");
        Popup.Board.IconIds.GemType2         = (uint)GetConfigValue<int>("GemIcon2");
        Popup.Board.IconIds.GemType3         = (uint)GetConfigValue<int>("GemIcon3");
        Popup.Board.IconIds.GemType4         = (uint)GetConfigValue<int>("GemIcon4");
        Popup.Board.IconIds.GemType5         = (uint)GetConfigValue<int>("GemIcon5");
        Popup.Board.IconIds.GemType6         = (uint)GetConfigValue<int>("GemIcon6");
        Popup.Board.IconIds.Bomb             = (uint)GetConfigValue<int>("BombIcon");
        Popup.Board.IconIds.HorizontalRocket = (uint)GetConfigValue<int>("HorizontalRocketIcon");
        Popup.Board.IconIds.VerticalRocket   = (uint)GetConfigValue<int>("VerticalRocketIcon");
        Popup.Board.IconIds.RainbowBomb      = (uint)GetConfigValue<int>("RainbowBombIcon");

        if (Popup.Board.Score > _lastHiScore) {
            _lastHiScore = Popup.Board.Score;
            SetConfigValue("HiScore", (int)Popup.HiScore);
        }
    }

    private void OnPopupOpened()
    {
        string data = GetConfigValue<string>("Data");
        if (string.IsNullOrEmpty(data)) return;

        Popup.Board.Deserialize(data);
    }

    private void OnPopupClosed()
    {
        if (Popup.Board.State == GameState.GameOver) return;
        if (string.IsNullOrEmpty(Popup.Data)) return;

        SetConfigValue("Data", Popup.Data);
    }
}
