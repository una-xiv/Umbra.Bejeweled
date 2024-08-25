namespace Umbra.Bejeweled.Game.Entities;

internal class HorizontalRocket(Board board, Vec2 cellPosition, IconIds iconIds) : Entity(11, board, cellPosition)
{
    private readonly Board _board = board;

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
                _board.ClearCell(new(x, CellPosition.Y));
            }

            _board.PlaySound(78);
        }

        _shrinkSize++;
        if (_shrinkSize > 30) return true;

        DrawIcon(iconIds.HorizontalRocket, _shrinkSize);

        return false;
    }
}
