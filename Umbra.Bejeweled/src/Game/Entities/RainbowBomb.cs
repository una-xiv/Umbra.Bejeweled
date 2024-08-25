namespace Umbra.Bejeweled.Game.Entities;

internal class RainbowBomb(Board board, Vec2 cellPosition, IconIds iconIds) : Entity(13, board, cellPosition)
{
    private readonly Board _board = board;

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
                _board.PlaySound(70);
            }

            if (_frameCounter == 60) {
                byte type = _board.GetDominantGemType();

                for (var y = 0; y < _board.Height; y++) {
                    for (var x = 0; x < _board.Width; x++) {
                        var gem = _board.GetEntityAt(x, y);
                        if (gem == null) continue;
                        if (gem.EntityType == type || gem.EntityType >= 10) _board.ClearCell(new(x, y));
                    }
                }

                _board.PlaySound(78);
                return true;
            }
        }

        _shrinkSize++;
        if (_shrinkSize > 30) _shrinkSize = 30;

        DrawIcon(iconIds.RainbowBomb, _shrinkSize);

        return false;
    }
}
