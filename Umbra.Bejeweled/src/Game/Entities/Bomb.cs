using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace Umbra.Bejeweled.Game.Entities;

internal class Bomb(Board board, Vec2 cellPosition, IconIds iconIds) : Entity(10, board, cellPosition)
{
    private readonly Board        _board             = board;
    private readonly List<Entity> _destroyedEntities = [];

    private int  _shrinkSize = 4;
    private int  _growSize   = -8;
    private bool _isInvoked;

    public override uint GetIconId()
    {
        return iconIds.Bomb;
    }

    protected override void OnDraw(float deltaTime)
    {
        _growSize++;
        if (_growSize > 4) _growSize = 4;

        DrawIcon(iconIds.Bomb, _growSize);
    }

    protected override bool OnDrawDestroyed(float deltaTime)
    {
        if (!_isInvoked) {
            _isInvoked = true;

            int x1 = CellPosition.X - 1;
            int x2 = CellPosition.X + 1;
            int y1 = CellPosition.Y - 1;
            int y2 = CellPosition.Y + 1;

            x1 = Math.Max(x1, 0);
            x2 = Math.Min(x2, _board.Width);
            y1 = Math.Max(y1, 0);
            y2 = Math.Min(y2, _board.Height);

            for (var y = y1; y <= y2; y++) {
                for (var x = x1; x <= x2; x++) {
                    if (x == CellPosition.X && y == CellPosition.Y) continue;
                    var entity = _board.GetEntityAt(x, y);
                    if (null != entity) _destroyedEntities.Add(entity);
                    _board.ClearCell(new(x, y));
                }
            }

            _board.PlaySound(78);
        }

        _shrinkSize++;

        if (_shrinkSize > 30) {
            _destroyedEntities.Clear();
            return true;
        }

        DrawIcon(iconIds.Bomb, _shrinkSize);

        // Grab a random entity from the destroyed entities list and draw a
        // little lightning bolt to indicate that the bomb has exploded.
        if (_destroyedEntities.Count > 0) {
            var     entity = _destroyedEntities[new Random().Next(_destroyedEntities.Count)];
            Vector2 p1     = new(Rect.TopLeft.X + Rect.Width / 2, Rect.TopLeft.Y + Rect.Height / 2);

            Vector2 p2 = new(
                entity.Rect.TopLeft.X + entity.Rect.Width / 2,
                entity.Rect.TopLeft.Y + entity.Rect.Height / 2
            );

            ImGui.GetForegroundDrawList().AddLine(p1, p2, 0xFFFFFFFF, 2);
        }

        return false;
    }
}
