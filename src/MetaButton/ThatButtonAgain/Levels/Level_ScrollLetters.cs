using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_ScrollLetters {
        /*
        //hardest, irregular increase, 3 steps, C letter stays inplace
        //2 +12
        //1 +3
        //0  -3 or 4 +3
        var changes = new[] {
            new [] { 1, 0, 0, 0, -1 },
            new [] { 2, 1, 0, 0, -2 },
            new [] { -1, 0, 1, 0, 0 },
            new [] { 1, 0, -1, 1, 0 }, //confusing
            new [] { -1, 0, 0, 0, 1 },
        };

        //very hard, regular increase, 3 steps, C letter moves
        //2 +12
        //3 -3
        //4 -3
        var changes = new[] {
            new [] { 1, 0, -1, 0, 1 }, //confusing
            new [] { -1, 1, 0, 1, 0 }, //confusing
            new [] { -1, 0, 1, 0, 0 },
            new [] { -1, 0, 0, 1, 0 },
            new [] { 0, -1, 0, -1, 1 },
        };

        */
        public static LevelContext Load_Trivial(GameController game) {
            //trivial, 2 steps, C leter stays inplace
            //2 +12
            //1 +3
            return LoadCore(
                game,
                new[] {
                    new [] { 1, 0, 0, -1, 0 }, //confusing
                    new [] { 1, 1, 0, 0, -1 },
                    new [] { -1, 0, 1, 0, 0 },
                    new [] { 1, 0, -1, 1, 0 }, //confusing
                    new [] { 0, 1, -1, 0, 1 },
                },
                new[] { (2, +12), (1, +3) }
            );
        }
        public static LevelContext Load_Hard(GameController game) {
            //hard, regular increase, 3 steps, C letter stays inplace
            //2 +12
            //4 -6
            //1 -3
            return LoadCore(
                game,
                new[] {
                    new [] { 1, 0, -1, 0, 1 }, //confusing
                    new [] { -1, 1, 0, 0, -1 },
                    new [] { -1, 0, 1, 0, 0 },
                    new [] { 1, 0, -1, 1, 0 }, //confusing
                    new [] { 0, -1, 0, 0, 1 },
                },
                new[] { (2, +12), (4, -6), (1, -3) }
            );
        }
        public static LevelContext Load_Extreme(GameController game) {
            //extreme, irregular increase, 4 steps, C letter moves
            //3 +3
            //2 +6
            //1 +3
            //4 -3
            return LoadCore(
                game,
                new[] {
                    new [] { 1, 0, -1, 0, 1 }, //confusing
                    new [] { 0, 1, 0, -1, 0 },
                    new [] { -2, 0, 1, 0, 0 },
                    new [] { 0, 0, 2, 1, 0 },
                    new [] { -1, 0, 0, 0, 1 },
                },
                new[] { (3, +3), (2, +6), (1, +3), (4, -3) }
            );
        }
        static LevelContext LoadCore(GameController game, int[][] changes, (int index, int offset)[] solution) {
            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.HitTestVisible = false;
            var lineHeight = game.letterDragBoxHeight;
            var linesCount = (int)Math.Ceiling(game.height / lineHeight + 1) + 1;
            var positions = "CLICK".Select(x => (float)((byte)x - (byte)'A')).ToArray();
            var expectedPositions = "TOUCH".Select(x => (float)((byte)x - (byte)'A')).ToArray();
            var offsets = new float[5];
            var letters = new Letter[linesCount, 5];

            for(int line = 0; line < linesCount; line++) {
                for(int column = 0; column < 5; column++) {
                    var letter = new Letter {
                        //Rect = GetLetterTargetRect(i , button.Rect)
                        //    .Offset(new Vector2(0, (j - letterCount / 2) * lineHeight)),
                        HitTestVisible = true,
                        Scale = new Vector2(Constants.ScrollLettersLetterScale),
                    };

                    var actualColumn = column;

                    void SetOffsetAndArrange(float offset) {
                        for(int col = 0; col < 5; col++) {
                            offsets![col] = changes![actualColumn][col] * offset;
                        }
                        ArrangeLetters();
                    }

                    float GetOffset(Vector2 delta) => -delta.Y / lineHeight;

                    letter.GetPressState = (starPoint, releaseState) => {
                        return new DragInputState(
                            starPoint,
                            onDrag: delta => {
                                SetOffsetAndArrange(GetOffset(delta));
                                return true;
                            },
                            onRelease: delta => {
                                var from = GetOffset(delta);
                                var to = (float)Math.Round(from);
                                var animation = new LerpAnimation<float> {
                                    Duration = TimeSpan.FromMilliseconds(300 * (float)Math.Abs(from - to)),
                                    From = from,
                                    To = to,
                                    SetValue = val => SetOffsetAndArrange(val),
                                    Lerp = MathFEx.Lerp,
                                    End = () => {
                                        for(int i = 0; i < 5; i++) {
                                            positions[i] = GetNormalizedPosition(positions[i] + offsets[i]);
                                            offsets[i] = 0;
                                        }
                                        if(positions.Zip(expectedPositions, (x, y) => MathFEx.FloatsEqual(x, y)).All(x => x)) {
                                            game.playSound(SoundKind.SuccessSwitch);
                                            button.HitTestVisible = true;
                                            for(int line = 0; line < linesCount; line++) {
                                                for(int column = 0; column < 5; column++) {
                                                    var letter = letters[line, column];
                                                    letter.HitTestVisible = false;
                                                    if(!button.Rect.Contains(letter.Rect))
                                                        game.scene.RemoveElement(letter);
                                                }
                                            }
                                        } else {
                                            game.playSound(SoundKind.Snap);
                                        }
                                    }
                                }.Start(game, blockInput: true);
                            },
                            releaseState);

                    };

                    letters[line, column] = letter;
                    letter.AddTo(game);
                }
            }
            void ArrangeLetters() {
                for(int line = 0; line < linesCount; line++) {
                    for(int column = 0; column < 5; column++) {
                        var letter = letters[line, column];

                        var position = positions[column] + offsets[column];
                        var letterIndex = GetNormalizedPosition(line + (float)Math.Truncate(position) - linesCount / 2);

                        letter.Value = (char)((byte)'A' + (byte)letterIndex);
                        letter.ActiveRatio = "TOUCH"[column] == letter.Value ? 1 : 0;

                        var fractionalOffset = (float)(position - Math.Truncate(position));
                        letter.Rect = game.GetLetterTargetRect(column, button.Rect)
                            .Offset(new Vector2(0, (line - linesCount / 2 - fractionalOffset) * lineHeight));
                    }
                }
            }
            ArrangeLetters();
            //var letters = CreateLetters((letter, index) => {
            //    letter.Rect = GetLetterTargetRect(index, button.Rect);
            //});

            var hints = new List<HintSymbol[]>();
            hints.Add(new HintSymbol[] { SvgIcon.Reload });
            hints.AddRange(solution
                .Select(x => {
                    return new HintSymbol[] { "TOUCH"[x.index], x.offset > 0 ? SvgIcon.DragUp : SvgIcon.DragUp }
                        .Concat(Math.Abs(x.offset).ToString().Select(c => new HintSymbol(null, c))).ToArray();
                })
            );
            hints.Add(ElementExtensions.TapButtonHint);
            return hints.ToArray();
        }
        static float GetNormalizedPosition(float position) {
            if(position < 0) position += 26;
            if(position >= 26) position -= 26;
            return position;
        }
    }
}

