using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using Umbra.Common;

namespace Umbra.Bejeweled.Game;

internal sealed partial class Board(Viewport viewport, Vec2 size)
{
    public const int CellSize = 64;

    public bool Active     { get; set; }
    public bool EnableSfx  { get; set; } = true;
    public int  ColorCount { get; set; } = 6;

    public uint Moves           { get; private set; } = 10;
    public uint ScoreMultiplier { get; private set; } = 1;
    public uint Score           { get; private set; } = 0;

    public int       Width    { get; private set; } = size.X;
    public int       Height   { get; private set; } = size.Y;
    public Viewport  Viewport { get; private set; } = viewport;
    public GameState State    { get; private set; } = GameState.Idle;
    public IconIds   IconIds  { get; private set; } = new();

    private Entity?  _swapSource;
    private Entity?  _swapTarget;
    private Vector2? _swapSourcePosition;
    private Vector2? _swapTargetPosition;
    private bool     _hasSwapped;
    private long     _lastMatchAt;
    private long     _lastActivityAt;

    public void Reset()
    {
        _lastMatchAt        = 0;
        _swapSource         = null;
        _swapTarget         = null;
        _swapSourcePosition = null;
        _swapTargetPosition = null;
        _hasSwapped         = false;

        Moves           = 10;
        Score           = 0;
        ScoreMultiplier = 1;

        Entities.Clear();
        DestroyedEntities.Clear();
        FillBoard();

        State = GameState.Idle;
    }

    public void Update(float deltaTime)
    {
        if (State == GameState.GameOver) {
            return;
        }

        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        if (now - _lastMatchAt > 2000) {
            ScoreMultiplier = 1;
        }

        if (State != GameState.Swapping) {
            CreateNewGems();
            UpdateGrid();

            if (HasEmptyCells()) {
                State           = GameState.Falling;
                _lastActivityAt = now;
            } else {
                if (now - _lastActivityAt < 500) {
                    uint score = ProcessMatches();

                    if (score > 0) {
                        _lastMatchAt = now;
                    }
                }

                State = HasFallingEntities() ? GameState.Falling : GameState.Idle;

                if (State == GameState.Idle && now - _lastActivityAt > 500) {
                    if (Moves == 0) {
                        State = GameState.GameOver;
                    }
                }
            }
        }

        switch (State) {
            case GameState.Falling:
                UpdatePhysics(deltaTime);
                _lastActivityAt = now;
                break;
            case GameState.Swapping:
                UpdateSwapPosition(deltaTime);
                _lastActivityAt = now;
                break;
        }

        foreach (var e in Entities) {
            e.Render(deltaTime);

            if (e.IsDestroyed) {
                if (!DestroyedEntities.Contains(e)) {
                    DestroyedEntities.Add(e);
                    SpawnParticlesFor(e, new Random().Next(25, 50));
                }
            }
        }

        UpdateParticles(deltaTime);
    }

    public byte GetDominantGemType()
    {
        Dictionary<byte, int> counts = [];

        foreach (var entity in Entities) {
            if (entity.EntityType == 0) continue;

            counts.TryAdd(entity.EntityType, 0);
            counts[entity.EntityType]++;
        }

        byte dominantType = 0;
        var  maxCount     = 0;

        foreach ((byte type, int count) in counts) {
            if (count > maxCount) {
                dominantType = type;
                maxCount     = count;
            }
        }

        return dominantType;
    }

    public bool TryInvokePowerUp(Vec2 pos)
    {
        Entity? entity = GetEntityAt(pos.X, pos.Y);
        if (entity == null) return false;

        if (entity.EntityType < 10) return false;

        entity.IsDestroyed = true;
        Moves--;

        return true;
    }

    public void TrySwap(Vec2 src, Vec2 dst)
    {
        Entity? source = GetEntityAt(src.X, src.Y);
        Entity? target = GetEntityAt(dst.X, dst.Y);

        if (source == null || target == null) {
            return;
        }

        if (source.IsFalling
            || target.IsFalling
            || source.IsDestroyed
            || target.IsDestroyed
            || source.EntityType == target.EntityType) {
            return;
        }

        State = GameState.Swapping;

        _swapSource = source;
        _swapTarget = target;

        _swapSourcePosition = source.SpritePosition;
        _swapTargetPosition = target.SpritePosition;

        _swapSource.OverridePosition = target.SpritePosition;
        _swapTarget.OverridePosition = source.SpritePosition;
    }

    private void UpdateSwapPosition(float deltaTime)
    {
        if (_swapSource?.OverridePosition == null && _swapTarget?.OverridePosition == null) {
            if (_hasSwapped) {
                _hasSwapped         = false;
                _swapSource         = null;
                _swapTarget         = null;
                _swapSourcePosition = null;
                _swapTargetPosition = null;
                State               = GameState.Idle;
                return;
            }

            // Test for a match.
            Vec2 tP = _swapTarget!.CellPosition;
            Vec2 sP = _swapSource!.CellPosition;

            Grid[tP.X, tP.Y] = _swapSource;
            Grid[sP.X, sP.Y] = _swapTarget;

            var match1 = GetMatchType(tP.X, tP.Y);
            var match2 = GetMatchType(sP.X, sP.Y);

            if (match1.Type != MatchType.None || match2.Type != MatchType.None) {
                Moves--;

                _swapSource.CellPosition = tP;
                _swapTarget.CellPosition = sP;

                State               = GameState.Idle;
                _hasSwapped         = false;
                _swapSource         = null;
                _swapTarget         = null;
                _swapSourcePosition = null;
                _swapTargetPosition = null;

                ProcessMatch(match1);
                ProcessMatch(match2);
                return;
            }

            Grid[sP.X, sP.Y] = _swapSource;
            Grid[tP.X, tP.Y] = _swapTarget;

            _hasSwapped                   = true;
            _swapSource!.OverridePosition = _swapSourcePosition;
            _swapTarget!.OverridePosition = _swapTargetPosition;
            return;
        }

        _swapSource?.UpdateSwap(deltaTime);
        _swapTarget?.UpdateSwap(deltaTime);
    }

    private readonly Dictionary<uint, long> _lastSfxPlayedAt = [];

    public void PlaySound(uint id)
    {
        if (!EnableSfx || !Active) return;

        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (_lastSfxPlayedAt.TryGetValue(id, out long lastPlayedAt) && now - lastPlayedAt < 500) return;
        _lastSfxPlayedAt[id] = now;

        UIModule.PlaySound(id);
    }
}
