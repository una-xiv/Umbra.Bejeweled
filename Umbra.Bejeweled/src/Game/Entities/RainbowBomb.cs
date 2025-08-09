using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace Umbra.Bejeweled.Game.Entities;

internal class RainbowBomb(Board board, Vec2 cellPosition, IconIds iconIds) : Entity(13, board, cellPosition)
{
    private readonly Board        _board             = board;
    private readonly List<Entity> _destroyedEntities = [];

    private int _shrinkSize   = 4;
    private int _growSize     = -8;
    private int _frameCounter;

    public override uint GetIconId()
    {
        return iconIds.RainbowBomb;
    }

    protected override void OnDraw(float deltaTime)
    {
        _growSize++;
        if (_growSize > 4) _growSize = 4;

        DrawIcon(iconIds.RainbowBomb, _growSize);
    }

    protected override bool OnDrawDestroyed(float deltaTime)
    {
        if (_frameCounter < 60) {
            _frameCounter++;

            if (_frameCounter == 1) {
                byte type = _board.GetDominantGemType();
                for (var y = 0; y < _board.Height; y++) {
                    for (var x = 0; x < _board.Width; x++) {
                        var gem = _board.GetEntityAt(x, y);
                        if (gem == null) continue;
                        if (gem.EntityType == type || (gem.EntityType >= 10 && gem.EntityType != EntityType)) {
                            _destroyedEntities.Add(gem);
                            _board.ClearCell(new(x, y));
                        }
                    }
                }

                _board.PlaySound(70);
            }

            if (_destroyedEntities.Count > 0) {
                var     entity = _destroyedEntities[new Random().Next(_destroyedEntities.Count)];
                Vector2 p1     = new(Rect.TopLeft.X + Rect.Width / 2, Rect.TopLeft.Y + Rect.Height / 2);

                Vector2 p2 = new(
                    entity.Rect.TopLeft.X + entity.Rect.Width / 2,
                    entity.Rect.TopLeft.Y + entity.Rect.Height / 2
                );

                ImGui.GetForegroundDrawList().AddLine(p1, p2, 0xFFFFFFFF, 2);
            }

            if (_frameCounter == 60) {
                _board.PlaySound(78);
                _destroyedEntities.Clear();
                return true;
            }
        }

        _shrinkSize++;
        if (_shrinkSize > 30) _shrinkSize = 30;

        DrawIcon(iconIds.RainbowBomb, _shrinkSize);

        return false;
    }
}
