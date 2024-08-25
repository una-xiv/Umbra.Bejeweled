using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;

namespace Umbra.Bejeweled.Game.Entities;

internal sealed class Particle(
    byte     entityType,
    Board    board,
    Vec2     cellPosition,
    float    lifeTime     = 1000,
    uint     iconId       = 14,
    int      size         = 0,
    Vector2? acceleration = null
) : Entity(
    entityType,
    board,
    cellPosition
)
{
    private readonly Board _board = board;

    private float   _elapsedTime;
    private float   _size         = size;
    private float   _opacity      = 1.0f;
    private uint    _color        = 0xFFFFFFFF;
    private Vector2 _acceleration = acceleration ?? new(0, 0);
    private Vector2 _velocity     = new(0, 0);
    private Vector2 _position     = new(0, 0);
    private Vector2 _targetPos    = new(0, 0);
    private Vector2 _uv1          = new(0, 0);
    private Vector2 _uv2          = new(1, 1);

    protected override void OnDraw(float deltaTime)
    {
        if (_elapsedTime == 0) {
            int h = _board.CellSize / 2;
            int x = new Random().Next(-h, h);
            int y = new Random().Next(-h, h);

            if (_size == 0) {
                _size = new Random().Next(1, 4);
            }

            float uvOffset = ((float)new Random().NextDouble() * 0.25f) + 0.2f;
            _uv1 = new(uvOffset, uvOffset);
            _uv2 = new(1 - uvOffset, 1 - uvOffset);

            _targetPos = _board.Viewport.TopLeft + new Vector2(_board.CellSize,        -_board.CellSize);
            _position  = _board.Viewport.TopLeft + SpritePosition + new Vector2(x + h, y + h);

            Vector2 accelPos1 = Vector2.Normalize(_targetPos - _position);
            Vector2 accelPos2 = new(
                (float)(new Random().NextDouble() - 0.5f),
                (float)(new Random().NextDouble() - 0.5f)
            );

            if (_acceleration == Vector2.Zero) {
                _acceleration = (new Random().Next(10) < 5 ? accelPos1 * 350 : accelPos2 * 100);
            }
        }

        _elapsedTime += (deltaTime * 1000);

        if (_elapsedTime >= lifeTime) {
            IsDestroyed = true;
            return;
        }

        float dist = Math.Max(0, Math.Min(1, Vector2.Distance(_position, _targetPos) / 100));

        _opacity =  1.0f - (_elapsedTime / lifeTime);
        _opacity *= dist;

        _velocity += _acceleration * deltaTime;
        _velocity *= dist;
        _position += _velocity * deltaTime;

        float size = MathF.Max(2, _size * (1 - _opacity));

        if (_opacity > 0.05f && size >= 1) {
            ImGui
                .GetForegroundDrawList()
                .AddImageRounded(
                    TextureProvider.GetFromGameIcon(new(iconId)).GetWrapOrEmpty().ImGuiHandle,
                    _position - new Vector2(size),
                    _position + new Vector2(size),
                    _uv1,
                    _uv2,
                    GetColorWithOpacity(),
                    _size / 2f
                );
        }
    }

    protected override bool OnDrawDestroyed(float deltaTime)
    {
        return true;
    }

    private uint GetColorWithOpacity()
    {
        var a = (byte)((_color & 0xFF000000) >> 24);
        var b = (byte)((_color & 0x00FF0000) >> 16);
        var g = (byte)((_color & 0x0000FF00) >> 8);
        var r = (byte)((_color & 0x000000FF) >> 0);

        a = (byte)(a * _opacity);

        return (uint)((a << 24) | (b << 16) | (g << 8) | r);
    }

    public override uint GetIconId()
    {
        return 0;
    }
}
