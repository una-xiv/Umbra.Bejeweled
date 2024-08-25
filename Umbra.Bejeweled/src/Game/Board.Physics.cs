using System.Collections.Generic;
using System.Linq;

namespace Umbra.Bejeweled.Game;

internal sealed partial class Board
{
    public void UpdatePhysics(float deltaTime)
    {
        // Sort entities with highest Y position first
        List<Entity> entities = [..Entities];
        entities.Sort((a, b) => a.CellPosition.Y.CompareTo(b.CellPosition.Y));

        // Update physics
        foreach (Entity entity in entities)
        {
            entity.UpdatePhysics(deltaTime);

            if (false == entity.IsAlive) {
                Entities.Remove(entity);
                DestroyedEntities.Remove(entity);
                Score += 1 * ScoreMultiplier;
            }
        }
    }

    /// <summary>
    /// Returns true if any entity is falling.
    /// </summary>
    private bool HasFallingEntities()
    {
        return Entities.Any(e => e.IsFalling);
    }
}
