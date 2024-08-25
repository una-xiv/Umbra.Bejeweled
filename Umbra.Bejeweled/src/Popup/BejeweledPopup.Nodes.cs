using Umbra.Bejeweled.Popup.Nodes;
using Una.Drawing;

namespace Umbra.Bejeweled.Popup;

internal sealed partial class BejeweledPopup
{
    protected override Node Node { get; } = new() {
        Id         = "Popup",
        Stylesheet = Stylesheet,
        ChildNodes = [
            new() {
                Id = "Header",
                ChildNodes = [
                    new() {
                        Id = "Title",
                        ChildNodes = [
                            new() {
                                Id = "Score",
                                ChildNodes = [
                                    new() { Id = "ScoreNumber" },
                                    new() { Id = "ScoreMultiplier" }
                                ]
                            },
                            new() {
                                Id        = "HiScore",
                                NodeValue = "Hi-Score: 0",
                            }
                        ]
                    },
                    new() {
                        Id = "Moves",
                        ChildNodes = [
                            new() { Id = "MovesNumber", NodeValue = "999" },
                            new() { Id = "MovesLabel", NodeValue  = "Moves" }
                        ]
                    },
                    new() {
                        Id = "Buttons",
                        ChildNodes = [
                            new ButtonNode("Reset", "Reset"),
                        ]
                    },
                ],
            },
            CreateGameBoard()
        ]
    };

    private static Node CreateGameBoard()
    {
        Node board = new() { Id = "Game" };

        for (int y = 0; y < 8; y++) {
            Node row = new() { ClassList = ["row"] };

            for (int x = 0; x < 10; x++) {
                Node cell = new() {
                    Id        = $"Cell-{x}-{y}",
                    ClassList = ["cell"],
                };

                row.ChildNodes.Add(cell);
            }

            board.ChildNodes.Add(row);
        }

        board.AppendChild(
            new() {
                Id = "GameOver",
                ChildNodes = [
                    new() {
                        Id        = "GameOverText",
                        NodeValue = "Game Over",
                    },
                    new() {
                        Id        = "GameOverScore",
                        NodeValue = "Score: 0",
                    },
                    new() {
                        Id        = "GameOverHiScore",
                        NodeValue = "Hi-Score: 0",
                    },
                ],
            }
        );

        return board;
    }
}
