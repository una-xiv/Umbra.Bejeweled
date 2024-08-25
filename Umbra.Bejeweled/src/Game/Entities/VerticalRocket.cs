namespace Umbra.Bejeweled.Game.Entities;

internal class VerticalRocket(Board board, Vec2 cellPosition, IconIds iconIds) : Entity(12, board, cellPosition)
{
    private readonly Board _board = board;

    private int  _shrinkSize = 4;
    private int  _growSize   = -8;
    private bool _isInvoked;

    public override uint GetIconId()
    {
        return iconIds.VerticalRocket;
    }

    protected override void OnDraw(float deltaTime)
    {
        _growSize++;
        if (_growSize > 4) _growSize = 4;

        DrawIcon(iconIds.VerticalRocket, _growSize);
    }

    protected override bool OnDrawDestroyed(float deltaTime)
    {
        if (!_isInvoked) {
            _isInvoked = true;

            for (var y = 0; y < _board.Height; y++) {
                if (CellPosition.Y == y) continue;
                _board.ClearCell(new(CellPosition.X, y));
            }

            _board.PlaySound(78);
        }

        _shrinkSize++;
        if (_shrinkSize > 30) return true;

        DrawIcon(iconIds.VerticalRocket, _shrinkSize);

        return false;
    }
}
