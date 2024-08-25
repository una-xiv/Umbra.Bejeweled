using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Umbra.Bejeweled.Game.Entities;
using Umbra.Common;

namespace Umbra.Bejeweled.Game;

internal partial class Board
{
    public string Serialize()
    {
        List<EntityState> entities = [];

        foreach (var entity in Entities) {
            var state = SerializeEntity(entity);
            if (state != null) entities.Add(state);
        }

        SerializedGameState gs = new() {
            Moves    = Moves,
            Score    = Score,
            Entities = entities,
        };

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(gs)));
    }

    public void Deserialize(string data)
    {
        try {
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(data));
            SerializedGameState? gs = JsonSerializer.Deserialize<SerializedGameState>(json);

            if (gs == null) {
                Logger.Warning("Failed to deserialize game state: deserialized object is null");
                return;
            }

            Moves           = gs.Moves;
            Score           = gs.Score;
            ScoreMultiplier = 1;
            State           = GameState.Idle;

            Entities.Clear();
            Particles.Clear();
            DestroyedEntities.Clear();

            foreach (var state in gs.Entities) {
                var entity = DeserializeEntity(state);

                if (entity != null) {
                    Entities.Add(entity);
                    Grid[entity.CellPosition.X, entity.CellPosition.Y] = entity;
                }
            }
        } catch (Exception e) {
            Logger.Error($"Failed to deserialize game state: {e.Message}");
        }
    }

    private EntityState? SerializeEntity(Entity entity)
    {
        byte? type = entity switch {
            Gem              => 0,
            HorizontalRocket => 1,
            VerticalRocket   => 2,
            Bomb             => 3,
            RainbowBomb      => 4,
            _                => null
        };

        if (type == null) return null;

        return new EntityState {
            Type = type.Value,
            Id   = entity.EntityType,
            X    = entity.CellPosition.X,
            Y    = entity.CellPosition.Y
        };
    }

    private Entity? DeserializeEntity(EntityState state)
    {
        return state.Type switch {
            0 => new Gem(this, new(state.X, state.Y), state.Id, IconIds),
            1 => new HorizontalRocket(this, new(state.X, state.Y), IconIds),
            2 => new VerticalRocket(this, new(state.X, state.Y), IconIds),
            3 => new Bomb(this, new(state.X, state.Y), IconIds),
            4 => new RainbowBomb(this, new(state.X, state.Y), IconIds),
            _ => null
        };
    }

    [Serializable]
    internal class SerializedGameState
    {
        public uint              Moves    { get; init; }
        public uint              Score    { get; init; }
        public uint              HiScore  { get; init; }
        public List<EntityState> Entities { get; init; } = [];
    }

    [Serializable]
    internal class EntityState
    {
        public byte Type { get; set; }
        public byte Id   { get; set; }
        public int  X    { get; set; }
        public int  Y    { get; set; }
    }
}
