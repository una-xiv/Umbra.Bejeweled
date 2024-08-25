using System.Collections.Generic;
using System.Linq;
using Umbra.Bejeweled.Game.Entities;
using Umbra.Common;

namespace Umbra.Bejeweled.Game;

internal sealed partial class Board
{
    /// <summary>
    /// The game grid.
    /// <para>
    /// This represents the actual state of the game, ignoring any animations.
    /// This means that empty cells are immediately filled with new entities,
    /// even though the entities may still be falling to their final position.
    /// </para>
    /// </summary>
    private Entity?[,] Grid { get; } = new Entity?[size.X, size.Y];

    /// <summary>
    /// Clears the cell at the given position.
    /// </summary>
    public void ClearCell(Vec2 position)
    {
        int x = position.X;
        int y = position.Y;

        if (x < 0 || x >= Width || y < 0 || y >= Height) {
            return;
        }

        Entity? entity = Grid[x, y];

        if (entity != null) {
            entity.IsDestroyed = true;
            Grid[x, y] = null;
        }
    }

    /// <summary>
    /// Returns true if there are empty cells on the board.
    /// </summary>
    private bool HasEmptyCells()
    {
        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                if (Grid[x, y] == null) {
                    return true;
                }
            }
        }

        return false;
    }

    private void UpdateGrid()
    {
        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                Grid[x, y] = null;
            }
        }

        List<Entity> entities = Entities.ToList();
        entities.Sort((a, b) => a.EntityType.CompareTo(b.EntityType));

        foreach (var entity in entities) {
            if (entity is { IsDestroyed: false, IsAlive: true, IsFalling: false, CellPosition.Y: >= 0 }) {
                if (Grid[entity.CellPosition.X, entity.CellPosition.Y] != null) {
                    Logger.Info($"Removed entity at {entity.CellPosition.X}, {entity.CellPosition.Y} with type {entity.EntityType}.");
                    Entities.Remove(Grid[entity.CellPosition.X, entity.CellPosition.Y]!);
                }

                Grid[entity.CellPosition.X, entity.CellPosition.Y] = entity;
            }
        }
    }

    /// <summary>
    /// Returns true if the cell below the given position is empty.
    /// </summary>
    public bool IsCellEmptyBelow(Vec2 position)
    {
        return position.Y + 1 < Height && Grid[position.X, position.Y + 1] == null;
    }

    /// <summary>
    /// Returns the entity type at the given position. Returns 0 if the
    /// given position is out of bounds or if there is no entity there.
    /// </summary>
    public byte GetGemAt(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) {
            return 0;
        }

        Entity? entity = Grid[x, y];
        return entity == null || entity.IsDestroyed ? (byte)0 : entity.EntityType;
    }

    /// <summary>
    /// Returns the entity type at the given position. Returns NULL if
    /// the given position is out of bounds or empty.
    /// </summary>
    public Entity? GetEntityAt(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) {
            return null;
        }

        Entity? entity = Grid[x, y];
        return entity == null || entity.IsDestroyed ? null : entity;
    }
}
