using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Umbra.Bejeweled.Game.Entities;
using Umbra.Common;
using Una.Drawing;

namespace Umbra.Bejeweled.Game;

internal sealed partial class Board
{
    private List<Entity>   Entities          { get; } = [];
    private List<Entity>   DestroyedEntities { get; } = [];
    private List<Particle> Particles         { get; } = [];

    private void SpawnParticlesFor(Entity entity)
    {
        int amount = new Random().Next(10, 20);

        for (var i = 0; i < amount; i++) {
            Particles.Add(new(0, this, entity.CellPosition, new Random().Next(500, 1500), entity.GetIconId()));
        }
    }

    private void UpdateParticles(float deltaTime)
    {
        foreach (var particle in Particles.ToArray()) {
            particle.Render(deltaTime);

            if (particle.IsDestroyed) {
                Particles.Remove(particle);
            }
        }
    }

    /// <summary>
    /// Creates new gems at the top of the board if necessary.
    /// </summary>
    private void CreateNewGems()
    {
        for (var x = 0; x < Width; x++) {
            Vector2 p1 = new(Viewport.TopLeft.X + x * CellSize, Viewport.TopLeft.Y - CellSize);
            Vector2 p2 = p1 + new Vector2(CellSize, CellSize * 1.2f);

            Rect cellRect = new(p1, p2);

            if (Entities.Any(e => e.Rect.Intersects(cellRect))) {
                continue;
            }

            AddGem(x, -1, GetNewGemTypeAt(x));
            PlaySound(30);
        }
    }

    /// <summary>
    /// Fills the board with gems.
    /// </summary>
    private void FillBoard(int iteration = 0)
    {
        if (iteration > 100) {
            Logger.Warning($"Failed to fill the board after {iteration} iterations.");
            return;
        }

        for (var y = 0; y < Height; y++) {
            for (var x = 0; x < Width; x++) {
                byte type = GetInitialTypeGemAt(x, y);

                if (type == 0) {
                    Logger.Info($"Failed to get initial type for gem at {x}, {y}. Retrying...");
                    FillBoard(iteration + 1);
                    return;
                }

                AddGem(x, y, type, true);
            }
        }

        int px = new Random().Next(0, Width);
        int py = new Random().Next(0, Height);

        // AddPowerUp<RainbowBomb>(px, py);
    }

    private void AddGem(int x, int y, byte type, bool addToGrid = false)
    {
        var entity                = new Gem(this, new(x, y), type, IconIds);
        if (addToGrid) Grid[x, y] = entity;
        Entities.Add(entity);
    }
}
