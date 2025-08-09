using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace Umbra.Bejeweled.Game.Entities;

internal class HorizontalRocket(Board board, Vec2 cellPosition, IconIds iconIds) : Entity(11, board, cellPosition)
{
    private readonly Board        _board             = board;
    private readonly List<Entity> _destroyedEntities = [];

    private int  _shrinkSize = 4;
    private int  _growSize   = -8;
    private bool _isInvoked  = false;

    public override uint GetIconId()
    {
        return iconIds.HorizontalRocket;
    }

    protected override void OnDraw(float deltaTime)
    {
        _growSize++;
        if (_growSize > 4) _growSize = 4;

        DrawIcon(iconIds.HorizontalRocket, _growSize);
    }

    protected override bool OnDrawDestroyed(float deltaTime)
    {
        if (!_isInvoked) {
            _isInvoked = true;

            for (var x = 0; x < _board.Width; x++) {
                if (CellPosition.X == x) continue;
                var entity = _board.GetEntityAt(x, CellPosition.Y);
                if (null != entity) _destroyedEntities.Add(entity);
                _board.ClearCell(new(x, CellPosition.Y));
            }

            _board.PlaySound(78);
        }

        _shrinkSize++;

        if (_shrinkSize > 30) {
            _destroyedEntities.Clear();
            return true;
        }

        DrawIcon(iconIds.HorizontalRocket, _shrinkSize);

        if (_destroyedEntities.Count > 0) {
            var     entity = _destroyedEntities[new Random().Next(_destroyedEntities.Count)];
            Vector2 p1     = new(Rect.TopLeft.X + Rect.Width / 2, Rect.TopLeft.Y + Rect.Height / 2);

            Vector2 p2 = new(
                entity.Rect.TopLeft.X + new Random().Next((int)entity.Rect.Width),
                entity.Rect.TopLeft.Y + new Random().Next((int)entity.Rect.Height)
            );

            ImGui.GetForegroundDrawList().AddLine(p1, p2, 0xFFFFFFFF, 2);
        }

        return false;
    }
}
