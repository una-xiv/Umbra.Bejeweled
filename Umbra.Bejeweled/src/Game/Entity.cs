using System;
using System.Numerics;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Umbra.Common;
using Una.Drawing;

namespace Umbra.Bejeweled.Game;

internal abstract class Entity(byte entityType, Board board, Vec2 cellPosition)
{
    public byte EntityType { get; private set; } = entityType;

    /// <summary>
    /// The cell position of the entity.
    /// </summary>
    public Vec2 CellPosition { get; set; } = cellPosition;

    /// <summary>
    /// Returns the sprite position of the entity.
    /// </summary>
    public Vector2 SpritePosition { get; private set; } = new(
        board.CellSize * cellPosition.X,
        board.CellSize * cellPosition.Y
    );

    /// <summary>
    /// Moves the entity to the specified position. Once reached, the
    /// value of <see cref="OverridePosition"/> is set to null. This
    /// value should be similar to that of <see cref="SpritePosition"/>.
    /// </summary>
    public Vector2? OverridePosition { get; set; } = null;

    public bool IsDestroyed { get; set; }
    public bool IsAlive     { get; private set; } = true;
    public bool IsFalling   { get; private set; }

    private const int FallSpeed = 1500;

    private Board   Board        { get; }      = board;
    private Vector2 Velocity     { get; set; } = Vector2.Zero;
    private Vector2 Acceleration { get; set; } = new(0, FallSpeed);
    private Vector2 MinVelocity  { get; }      = new(-FallSpeed, -FallSpeed);
    private Vector2 MaxVelocity  { get; }      = new(FallSpeed, FallSpeed);

    protected ITextureProvider  TextureProvider => _textureProvider ??= Framework.Service<ITextureProvider>();
    private   ITextureProvider? _textureProvider;

    public void UpdateSwap(float deltaTime)
    {
        if (!IsAlive) return;

        if (OverridePosition != null) {
            Vector2 targetPos = OverridePosition.Value;
            Vector2 newPos    = Vector2.Lerp(SpritePosition, targetPos, 0.1f);

            if (Vector2.Distance(newPos, targetPos) < 1) {
                SpritePosition   = targetPos;
                OverridePosition = null;
                return;
            }

            SpritePosition = newPos;
        }
    }

    public void UpdatePhysics(float deltaTime)
    {
        if (!IsAlive) return;

        float multiplier = MathF.Min(2, 1 + (Board.ScoreMultiplier / 10f));

        Velocity += Acceleration * (deltaTime * multiplier);
        Velocity =  Vector2.Clamp(Velocity, MinVelocity, MaxVelocity);

        Vector2 newPos = SpritePosition + Velocity * (deltaTime * multiplier);

        if (newPos.Y > SpritePosition.Y + (Board.CellSize / 4)) {
            newPos.Y = SpritePosition.Y + (Board.CellSize / 4);
        }

        if (ClampToFloor(newPos)) {
            StopFalling();
            return;
        }

        if (ClampToCell(newPos)) {
            StopFalling();
            return;
        }

        SpritePosition = newPos;
        CellPosition   = new((int)newPos.X / Board.CellSize, (int)newPos.Y / Board.CellSize);
        IsFalling      = true;
    }

    public void Render(float deltaTime)
    {
        if (!IsAlive) return;

        if (IsDestroyed) {
            IsAlive = !OnDrawDestroyed(deltaTime);
            return;
        }

        OnDraw(deltaTime);
    }

    /// <summary>
    /// Invoked on every frame for as long as the entity is alive and not destroyed.
    /// </summary>
    /// <param name="deltaTime">The elapsed time in milliseconds since the last frame.</param>
    protected abstract void OnDraw(float deltaTime);

    /// <summary>
    /// Invoked on every frame when the entity has been marked as destroyed. The entity
    /// is considered dead when this method returns true.
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    protected abstract bool OnDrawDestroyed(float deltaTime);

    public abstract uint GetIconId();

    /// <summary>
    /// Draws an icon at the entity's position.
    /// </summary>
    protected void DrawIcon(uint iconId, int padding = 4)
    {
        Rect r = new Rect(Rect.TopLeft, Rect.BottomRight);
        r.Shrink(new((int)(padding * Node.ScaleFactor)));

        ImGui
            .GetForegroundDrawList()
            .AddImage(
                TextureProvider.GetFromGameIcon(new(iconId)).GetWrapOrEmpty().ImGuiHandle,
                r.TopLeft,
                r.BottomRight,
                Vector2.Zero,
                Vector2.One,
                0xFFFFFFFF
            );
    }

    public Rect Rect => new(Board.Viewport.TopLeft + SpritePosition, new Size(Board.CellSize));

    private float FloorY => Board.Viewport.BottomRight.Y - Board.Viewport.TopLeft.Y - Board.CellSize;

    private bool ClampToFloor(Vector2 position)
    {
        if (position.Y > FloorY) {
            position.Y = FloorY;

            SpritePosition = position;
            CellPosition   = new((int)position.X / Board.CellSize, (int)position.Y / Board.CellSize);
            return true;
        }

        return false;
    }

    private bool ClampToCell(Vector2 position)
    {
        Vec2 cellPos = new((int)position.X / Board.CellSize, (int)position.Y / Board.CellSize);

        if (Board.IsCellEmptyBelow(cellPos)) {
            return false;
        }

        // Calculate the target position.
        Vector2 targetPos = new(
            cellPos.X * Board.CellSize,
            cellPos.Y * Board.CellSize
        );

        // If the entity is close enough to the target position, snap to it.
        if (Vector2.Distance(position, targetPos) < 16) {
            SpritePosition = targetPos;
            CellPosition   = cellPos;
            return true;
        }

        return false;
    }

    private void StopFalling()
    {
        if (!IsFalling && Velocity.Y == 0) return;

        Velocity  = Vector2.Zero;
        IsFalling = false;
    }
}
