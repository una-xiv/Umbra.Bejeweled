using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Umbra.Bejeweled.Game.Entities;
using Umbra.Common;

namespace Umbra.Bejeweled.Game;

internal struct Match
{
    public MatchType       Type     { get; set; }
    public int             X        { get; set; }
    public int             Y        { get; set; }
    public HashSet<Entity> Entities { get; set; }
}

internal sealed partial class Board
{
    private void ProcessMatch(Match match)
    {
        if (match.Type == MatchType.None) return;

        foreach (var ent in match.Entities) {
            ent.IsDestroyed = true;
        }

        switch (match.Type) {
            case MatchType.HorizontalRocket:
                AddPowerUp<HorizontalRocket>(match.X, match.Y);
                break;
            case MatchType.VerticalRocket:
                AddPowerUp<VerticalRocket>(match.X, match.Y);
                break;
            case MatchType.Bomb:
            case MatchType.TeeShape:
                AddPowerUp<Bomb>(match.X, match.Y);
                break;
            case MatchType.Rainbow:
                AddPowerUp<RainbowBomb>(match.X, match.Y);
                break;
            case MatchType.Default:
                if (match.Entities.Count >= 5) AddPowerUp<Bomb>(match.X, match.Y);
                break;
            case MatchType.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(match));
        }
    }

    private void AddPowerUp<T>(int x, int y) where T : Entity
    {
        ClearCell(new(x, y));

        Moves += 2;

        var entity = (T)Activator.CreateInstance(typeof(T), this, new Vec2(x, y), IconIds)!;
        Grid[x, y] = entity;
        Entities.Add(entity);

        PlaySound(60);
    }

    public Match GetMatchType(int x, int y)
    {
        byte t = GetGemAt(x, y);

        return t == 0 ? new() { Type = MatchType.None } : GetMatchType(t, x, y);
    }

    public Match GetMatchType(byte t, int sx = -1, int sy = -1)
    {
        List<Match> matches = [];

        bool isAutoParam = sx == -1 && sy == -1;

        // Check for horizontal match of 5
        for (var x = 0; x <= Width; x++) {
            for (var y = 0; y < Height; y++) {
                if (isAutoParam) {
                    sx = x;
                    sy = y;
                }

                // Rainbow match horizontal
                var rbh = GetMatchedEntitiesAt(t, [(x, y), (x + 1, y), (x + 2, y), (x + 3, y), (x + 4, y)]);
                if (rbh.Count > 0) matches.Add(new() { Type = MatchType.Rainbow, X = sx, Y = sy, Entities = rbh });

                // Rainbow match vertical
                var rbv = GetMatchedEntitiesAt(t, [(x, y), (x, y + 1), (x, y + 2), (x, y + 3), (x, y + 4)]);
                if (rbv.Count > 0) matches.Add(new() { Type = MatchType.Rainbow, X = sx, Y = sy, Entities = rbv });

                // T-shape match up
                var tsu = GetMatchedEntitiesAt(t, [(x, y), (x - 1, y), (x + 1, y), (x, y + 1), (x, y + 2)]);
                if (tsu.Count > 0) matches.Add(new() { Type = MatchType.TeeShape, X = sx, Y = sy, Entities = tsu });

                // T-shape match down
                var tsd = GetMatchedEntitiesAt(t, [(x, y), (x, y + 1), (x, y + 2), (x - 1, y + 2), (x + 1, y + 2)]);
                if (tsd.Count > 0) matches.Add(new() { Type = MatchType.TeeShape, X = sx, Y = sy, Entities = tsd });

                // Square block match
                var sqb = GetMatchedEntitiesAt(t, [(x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1)]);
                if (sqb.Count > 0) matches.Add(new() { Type = MatchType.Bomb, X = sx, Y = sy, Entities = sqb });

                // Horizontal match of 4
                var h4 = GetMatchedEntitiesAt(t, [(x, y), (x + 1, y), (x + 2, y), (x + 3, y)]);
                if (h4.Count > 0) matches.Add(new() { Type = MatchType.VerticalRocket, X = sx, Y = sy, Entities = h4 });

                // Vertical match of 4
                var v4 = GetMatchedEntitiesAt(t, [(x, y), (x, y + 1), (x, y + 2), (x, y + 3)]);

                if (v4.Count > 0)
                    matches.Add(new() { Type = MatchType.HorizontalRocket, X = sx, Y = sy, Entities = v4 });

                // Horizontal match of 3
                var h3 = GetMatchedEntitiesAt(t, [(x, y), (x + 1, y), (x + 2, y)]);
                if (h3.Count > 0) matches.Add(new() { Type = MatchType.Default, X = sx, Y = sy, Entities = h3 });

                // Vertical match of 3
                var v3 = GetMatchedEntitiesAt(t, [(x, y), (x, y + 1), (x, y + 2)]);
                if (v3.Count > 0) matches.Add(new() { Type = MatchType.Default, X = sx, Y = sy, Entities = v3 });
            }
        }

        if (matches.Count == 0) {
            return new() { Type = MatchType.None };
        }

        Match           bestMatch   = matches[0];
        HashSet<Entity> allEntities = [];

        foreach (var match in matches) {
            if (bestMatch.Type < match.Type) {
                bestMatch = match;
            }

            allEntities.UnionWith(match.Entities);
        }

        bestMatch.Entities = allEntities;

        PlaySound(20);

        return bestMatch;
    }

    private HashSet<Entity> GetMatchedEntitiesAt(byte type, List<(int, int)> positions)
    {
        HashSet<Entity> entities = [];

        foreach ((int x, int y) in positions) {
            Entity? entity = GetEntityAt(x, y);
            if (entity == null || entity.EntityType == 0) return [];

            entities.Add(entity);
        }

        if (entities.Count == 0) return [];

        return entities.All(e => type == e.EntityType) ? entities : [];
    }

    private uint ProcessMatches()
    {
        uint score = 0;
        uint count = 0;

        for (byte t = 1; t < 16; t++) {
            Match match = GetMatchType(t);

            if (match.Type != MatchType.None) {
                ProcessMatch(match);

                score += (uint)(ScoreMultiplier * match.Entities.Count);
                count++;
            }
        }

        if (count > 0) ScoreMultiplier += count;

        return score;
    }

    /// <summary>
    /// Returns a gem type that would be appropriate for the given
    /// horizontal position. This is used to fill the board with new gems
    /// from the top.
    /// </summary>
    private byte GetNewGemTypeAt(int x)
    {
        // while (true) {
        //     byte gem = PickRandomGemType();
        //
        //     byte below  = GetGemAt(x,     0);
        //     byte below2 = GetGemAt(x,     1);
        //     byte left   = GetGemAt(x - 1, 0);
        //     byte left2  = GetGemAt(x - 2, 0);
        //     byte right  = GetGemAt(x + 1, 0);
        //     byte right2 = GetGemAt(x + 2, 0);
        //
        //     if (gem == below && gem == below2) {
        //         continue;
        //     }
        //
        //     if (x >= 2 && gem == left && gem == left2) {
        //         continue;
        //     }
        //
        //     if (x < Width - 2 && gem == right && gem == right2) {
        //         continue;
        //     }
        //
        //     return gem;
        // }
        return PickRandomGemType();
    }

    /// <summary>
    /// Returns a gem type that would be appropriate for the initial board state
    /// at the given position. The gem type should not be part of any matches.
    ///
    /// A blank board is filled from top to bottom, left to right.
    /// </summary>
    private byte GetInitialTypeGemAt(int x, int y)
    {
        int iteration = 0;

        while (true) {
            iteration++;

            if (iteration > (Width * Height) * 2) {
                Logger.Warning($"Failed to pick a gem type for ({x}, {y}) after {iteration} iterations.");
                return 0;
            }

            byte type = PickRandomGemType();

            if (x >= 1) {
                byte left = GetGemAt(x - 1, y);

                if (type == left) {
                    continue;
                }
            }

            if (y >= 1) {
                byte above = GetGemAt(x, y - 1);

                if (type == above) {
                    continue;
                }
            }

            return type;
        }
    }

    public byte PickRandomGemType()
    {
        // Buffer to hold the random byte
        var randomNumber = new byte[1];

        // Generate a cryptographically secure random number
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();

        do {
            rng.GetBytes(randomNumber);
        } while (randomNumber[0] > 250); // This avoids bias in the distribution

        // Ensure the random number is in the range 1 to 6 (inclusive)
        return (byte)(randomNumber[0] % (ColorCount) + 1);
    }
}
