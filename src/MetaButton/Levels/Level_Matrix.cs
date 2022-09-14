using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_Matrix {
        public static LevelContext Load_3InARow(GameController game) {
            //bool winLevel = false;
            var button = game.CreateButton(() => {
                game.StartNextLevelAnimation();
            }).AddTo(game);
            button.Rect = Rect.Empty;
            button.HitTestVisible = false;

            var chars = new[] {
                "OTOHCTOCU",
                "CCUCCHOUT",
                "OTHUUUHUH",
                "CUHHCUTUH",
                "OOOUHHOTH",
                "TTUHHUUCT",
                "TOCHOUOCH",
                "OOTHCUHUU",
                "TOOCCCHCH"
            };

            Letter?[,] letters = null!;
            letters = CreateLetters(game, button, chars, hovered => {
                if(hovered.Select(x => x.letter.Value).SequenceEqual("TOUCH".Select(x => x))) {
                    foreach(var item in hovered) {
                        item.letter.Style = LetterStyle.Accent1;
                    }
                    game.playSound(SoundKind.SuccessSwitch);
                    return false;
                }

                if(hovered.Count < 3 || hovered.Skip(1).Any(x => x.letter.Value != hovered[0].letter.Value))
                    return true;
                if(hovered[0].row == hovered[1].row) {
                    foreach(var (letter, row, col) in hovered) {
                        game.scene.RemoveElement(letters[row, col]!);
                        for(int i = row; i > 0; i--) {
                            letters[i, col] = letters[i - 1, col];
                            if(letters[i, col] != null)
                                game.StartLetterDirectionAnimation(letters[i, col]!, Direction.Down, count: 1, rowStep: game.letterDragBoxWidth);
                        }
                        letters[0, col] = null; 
                    }
                } else {
                    var col = hovered[0].col;
                    for(int i = hovered.Last().row; i >= hovered.Count; i--) {
                        letters[i, col] = letters[i - hovered.Count, col];
                        if(letters[i, col] != null)
                            game.StartLetterDirectionAnimation(letters[i, col]!, Direction.Down, count: hovered.Count, rowStep: game.letterDragBoxWidth);
                    }
                    foreach(var (letter, row, _) in hovered) {
                        game.scene.RemoveElement(letter);
                        letters[row - hovered[0].row, col] = null;
                    }
                }
                game.playSound(Direction.Down.GetSound());
                return true;
            });
            foreach(var item in letters) {
                item!.Style = GameControllerExtensions.ToStyle(item!.Value);
            }

            return new[] {
                new HintSymbol[] { '9', 'C', SvgIcon.DragRight, 'C' },
                new HintSymbol[] { '2', 'O', SvgIcon.DragDown, 'O' },
                new HintSymbol[] { '4', 'H', SvgIcon.DragDown, 'H' },
                new HintSymbol[] { '6', 'U', SvgIcon.DragDown, 'U' },
                new HintSymbol[] { '9', 'T', SvgIcon.DragRight, 'H' },
                GameControllerExtensions.TapButtonHint,
            };
        }

        public static LevelContext Load_FindWord(GameController game) {
            bool winLevel = false;
            var button = game.CreateButton(() => {
                if(winLevel)
                    game.StartNextLevelAnimation();
                else {
                    game.StartCthulhuReloadLevelAnimation();
                }
            }).AddTo(game);
            button.Rect = Rect.Empty;
            button.HitTestVisible = false;

            var chars = new[] {
                "LOKUHTOKU",
                "HCLICKOUT",
                "CTHLUOLUT",
                "KUKHIUTKL",
                "OUOLHKOTH",
                "LTUHKHULT",
                "TICHIOCLH",
                "KCTHULHUI",
                "TOUKCILCH"
            };

            Letter?[,] letters = null!;
            letters = CreateLetters(game, button, chars, hovered => {
                var result = hovered.Select(x => x.letter.Value);
                bool win = result.SequenceEqual("TOUCH".Select(x => x));
                bool fail = result.SequenceEqual("CTHULHU".Select(x => x));
                if(win || fail) {
                    winLevel = win;
                    return false;
                } else {
                    hovered.ForEach(x => x.letter.ActiveRatio = 0);
                    return true;
                }
            });
            foreach(var item in letters) {
                item!.ActiveRatio = 0;
            }

            return new[] {
                new HintSymbol[] { 'C', SvgIcon.DragRight, 'U', SvgIcon.Alert },
                new HintSymbol[] { 'T', SvgIcon.DragDown, 'H' },
                GameControllerExtensions.TapButtonHint,
            };
        }

        static Letter?[,] CreateLetters(GameController game, Button button, string[] chars, Func<List<(Letter letter, int row, int col)>, bool> onHoverComplete) {
            int letterCount = chars.Length;
            var letters = new Letter[letterCount, letterCount];
            (int row, int col) GetLetterPosition(Letter letter) {
                for(int row = 0; row < letterCount; row++) {
                    for(int col = 0; col < letterCount; col++) {
                        if(letters![row, col] == letter) {
                            return (row, col);
                        }
                    }
                }
                throw new InvalidOperationException();
            }

            var buttonRect = game.GetButtonRect();
            for(int row = 0; row < letterCount; row++) {
                for(int col = 0; col < letterCount; col++) {
                    var value = chars[row][col];
                    var letter = new Letter {
                        Value = value,
                        Rect = game.GetLetterTargetRect(col - (letterCount - 5) / 2, buttonRect, squared: true, row: row - letterCount / 2),
                        HitTestVisible = true,
                        Scale = new Vector2(Constants.FindWordLetterScale),
                    };
                    var hovered = new List<(Letter letter, int row, int col)>();
                    letter.GetPressState = HoverInputState.GetHoverHandler(
                        game.scene,
                        letter,
                        element => {
                            if(element is Letter l && !hovered.Any(x => x.letter == l)) {
                                var (row, col) = GetLetterPosition(l);
                                if(hovered.Count == 0
                                || hovered.Count == 1
                                || (col == hovered.Last().col + 1 || col == hovered.Last().col - 1) && row == hovered[0].row && row == hovered.Last().row
                                || (row == hovered.Last().row + 1 || row == hovered.Last().row - 1) && col == hovered[0].col && col == hovered.Last().col
                                ) {
                                    hovered.Add((l, row, col));
                                    l.ActiveRatio = 1;
                                    game.playSound(SoundKind.Hover);
                                }
                                button.Rect = hovered
                                    .Skip(1)
                                    .Aggregate(hovered[0].letter.Rect, (rect, x) => rect.ContainingRect(x.letter.Rect))
                                    .Inflate(new Vector2(Constants.ContainingButtonInflateValue));
                            }
                        },
                        onRelease: () => {
                            hovered.Sort((x, y) => {
                                var row = Comparer<int>.Default.Compare(x.row, x.row);
                                if(row != 0) return row;
                                return Comparer<int>.Default.Compare(x.col, x.col);
                            });
                            if(onHoverComplete(hovered)) {
                                button.Rect = Rect.Empty;
                            } else {
                                for(int row = 0; row < chars.Length; row++) {
                                    for(int col = 0; col < chars[0].Length; col++) {
                                        if(!hovered.Any(x => x.letter == letters[row, col])) {
                                            game.scene.RemoveElement(letters[row, col]!);
                                        } else {
                                            letters[row, col]!.HitTestVisible = false;
                                        }
                                    }
                                }
                                button.HitTestVisible = true;
                            }
                            hovered.Clear();
                        }
                    );
                    letters[row, col] = letter;
                    letter.AddTo(game);
                }
            }
            return letters;
        }
    }
}

