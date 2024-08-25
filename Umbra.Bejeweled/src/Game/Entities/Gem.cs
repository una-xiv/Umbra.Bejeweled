using System.Numerics;
using ImGuiNET;

namespace Umbra.Bejeweled.Game.Entities;

internal class Gem(Board board, Vec2 cellPosition, byte type, IconIds iconIds) : Entity(type, board, cellPosition)
{
    private int _shrinkSize = 4;
    private int _growSize   = 30;

    /// <inheritdoc/>
    protected override void OnDraw(float deltaTime)
    {
        _growSize--;
        if (_growSize < 4) _growSize = 4;

        DrawIcon(GetIconId(), _growSize);

        ImGui
            .GetForegroundDrawList()
            .AddText(Rect.TopLeft + new Vector2(5, 5), 0xFFFFFFFF, $"{EntityType}");
    }

    /// <inheritdoc/>
    protected override bool OnDrawDestroyed(float deltaTime)
    {
        _shrinkSize++;
        if (_shrinkSize > 30) return true;

        DrawIcon(GetIconId(), _shrinkSize);
        return false;
    }

    public override uint GetIconId()
    {
        return EntityType switch {
            1 => iconIds.GemType1,
            2 => iconIds.GemType2,
            3 => iconIds.GemType3,
            4 => iconIds.GemType4,
            5 => iconIds.GemType5,
            6 => iconIds.GemType6,
            _ => 14u,
        };
    }
}
