using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using Umbra.Bejeweled.Game;
using Umbra.Widgets;
using Una.Drawing;

namespace Umbra.Bejeweled.Popup;

internal sealed partial class BejeweledPopup : WidgetPopup
{
    public  uint  Score        { get; set; }
    public  uint  HiScore      { get; set; }
    public  bool  Sound        { get; set; } = true;
    public  Board Board        { get; private set; }

    private Vec2? SelectedCell { get; set; }
    private Node? SelectedNode { get; set; }
    private Vec2? TargetedCell { get; set; }
    private Node? TargetedNode { get; set; }
    private uint  TargetScore  { get; set; }

    private Viewport Viewport { get; set; }

    public BejeweledPopup()
    {
        Viewport = new();
        Board    = new(Viewport, new(10, 8));

        Node.QuerySelector("#Reset")!.OnMouseUp += _ => ResetGame();

        foreach (Node cell in Node.QuerySelectorAll(".cell")) {
            cell.OnMouseUp    += OnCellClicked;
            cell.OnMouseEnter += OnCellMouseEnter;
        }

        ResetGame();
    }

    /// <inheritdoc/>
    protected override void OnOpen()
    {
        Board.Active = true;
    }

    /// <inheritdoc/>
    protected override void OnClose()
    {
        Board.Active = false;
    }

    /// <inheritdoc/>
    protected override unsafe void OnUpdate()
    {
        if (Board.State == GameState.GameOver) {
            foreach (var cell in Node.QuerySelectorAll(".cell")) cell.Style.IsVisible = false;
            Node.QuerySelector("#GameOver")!.Style.IsVisible  = true;
            Node.QuerySelector("#GameOverScore")!.NodeValue   = $"Score: {Board.Score:N0}";
            Node.QuerySelector("#GameOverHiScore")!.NodeValue = $"Hi-Score: {HiScore:N0}";
            return;
        }

        float   deltaTime   = Framework.Instance()->FrameDeltaTime;
        Vector2 topLeft     = Node.QuerySelector("#Cell-0-0")!.Bounds.MarginRect.TopLeft;
        Vector2 bottomRight = Node.QuerySelector("#Cell-9-7")!.Bounds.MarginRect.BottomRight;

        for (var y = 0; y < 8; y++) {
            for (var x = 0; x < 10; x++) {
                var cell = Node.QuerySelector($"#Cell-{x}-{y}")!;
                cell.ToggleClass("selected", SelectedNode == cell);
                cell.ToggleClass("targeted", TargetedNode == cell);

                cell.IsDisabled = Board.State != GameState.Idle;

                if (cell.IsDisabled && cell.TagsList.Contains("hover")) {
                    cell.TagsList.Remove("hover");
                }
            }
        }

        Viewport.Update(topLeft, bottomRight);
        Board.Update(deltaTime);

        TargetScore = Board.Score;

        if (Score < TargetScore) {
            Score += Math.Max(1, (uint)((TargetScore - Score) * deltaTime * 10));
        }

        if (Score > HiScore) {
            HiScore = Score;
        }

        Node.QuerySelector("#GameOver")!.Style.IsVisible  = false;
        Node.QuerySelector("#ScoreNumber")!.NodeValue     = $"Score: {Score:N0}";
        Node.QuerySelector("#ScoreMultiplier")!.NodeValue = $"x{Board.ScoreMultiplier}";
        Node.QuerySelector("#HiScore")!.NodeValue         = $"Hi-Score: {HiScore:N0}";
        Node.QuerySelector("#MovesNumber")!.NodeValue     = $"{Board.Moves}";
    }

    private void ResetSelection()
    {
        SelectedCell = null;
        SelectedNode = null;
        TargetedCell = null;
        TargetedNode = null;
    }

    private void OnCellClicked(Node node)
    {
        if (Board.State != GameState.Idle) return;

        byte x = byte.Parse(node.Id!.Split('-')[1]);
        byte y = byte.Parse(node.Id!.Split('-')[2]);

        if (SelectedNode == node) {
            ResetSelection();
            PlaySound(15);
            return;
        }

        if (SelectedCell == null) {
            if (Board.TryInvokePowerUp(new(x, y))) {
                return;
            }

            SelectedCell = new(x, y);
            SelectedNode = node;
            PlaySound(5);
            return;
        }

        // Make sure the selected cell is adjacent to the clicked cell.
        if (Math.Abs(SelectedCell.Value.X - x) + Math.Abs(SelectedCell.Value.Y - y) != 1) {
            SelectedCell = new(x, y);
            SelectedNode = node;
            PlaySound(5);
            return;
        }

        // Swap the gems
        if (SelectedCell != null && TargetedCell != null) {
            Board.TrySwap(SelectedCell.Value, TargetedCell.Value);
            PlaySound(10);
            ResetSelection();
        }
    }

    private void PlaySound(uint soundId)
    {
        if (Sound) UIModule.PlaySound(soundId);
    }

    private void OnCellMouseEnter(Node node)
    {
        if (Board.State != GameState.Idle) return;

        if (SelectedCell == null || node == SelectedNode) {
            // There is no target cell if there is no selected (source) cell or
            // if the mouse is over the selected cell.
            TargetedCell = null;
            TargetedNode = null;
            return;
        }

        byte x = byte.Parse(node.Id!.Split('-')[1]);
        byte y = byte.Parse(node.Id!.Split('-')[2]);

        // Make sure the target cell is adjacent to the selected cell.
        if (Math.Abs(SelectedCell.Value.X - x) + Math.Abs(SelectedCell.Value.Y - y) != 1) {
            TargetedCell = null;
            TargetedNode = null;
            return;
        }

        TargetedCell = new(x, y);
        TargetedNode = node;
    }

    /// <summary>
    /// Resets the game.
    /// </summary>
    public void ResetGame()
    {
        Score       = 0;
        TargetScore = 0;
        Board.Reset();

        foreach (var cell in Node.QuerySelectorAll(".cell")) cell.Style.IsVisible = true;
    }
}
