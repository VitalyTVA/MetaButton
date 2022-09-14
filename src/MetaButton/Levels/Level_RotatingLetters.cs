using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_RotatingLetters {
/*
//04 medium
var rotations = new[] {
    new[] { 1, 0, 0, -2, 1 },
    new[] { 0, -1, 2, 0, 0 },
    new[] { -2, 0, 1, 0, 0 },
    new[] { -1, 0, 0, 1, 0 },
    new[] { 1, 0, 2, 0, -1 },
};
*/

        public static LevelContext Load_Easy(GameController game) {
            return LoadCore(
                game,
                new[] {
                    new[] { 1, 0, 0, -1, 0 },
                    new[] { 0, 1, 0, 0, -1 },
                    new[] { 0, -1, 1, 0, 0 },
                    new[] { 0, 0, 1, -1, 0 },
                    new[] { 1, 0, 0, 0, 1 },
                },
                new[] { 0, 1, 2, 3, 4 }
            );
        }

        public static LevelContext Load_Medium(GameController game) {
            return LoadCore(
                game,
                new[] {
                    new[] { 1, 0, 0, -2, 0 },
                    new[] { 0, 2, 0, -1, 1 },
                    new[] { -2, 0, 1, 0, 1 },
                    new[] { 0, -1, 0, 1, 0 },
                    new[] { 1, -2, 0, 0, 1 },
                },
                new[] { 0, 0, 2, 2, 3, 3 }
            );
        }

        public static LevelContext Load_Hard(GameController game) {
            return LoadCore(
                game,
                new[]  {
                    new[] { 1, -1, 0, -2, 0 },
                    new[] { -1, 1, 0, 0, 0 },
                    new[] { 2, 0, -1, 0, -1 },
                    new[] { 2, 0, 1, -1, 0 },
                    new[] { -1, 0, 1, 0, 1 },
                },
                new[] { 0, 1, 4, 4 }
            );
        }

        static LevelContext LoadCore(GameController game, int[][] rotations, int[] solution) {
            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.HitTestVisible = false;

            void StartRotation(Letter letter, float delta) {
                new LerpAnimation<float> {
                    Duration = TimeSpan.FromMilliseconds(300),
                    From = letter.Angle,
                    To = letter.Angle + delta,
                    SetValue = val => letter.Angle = val,
                    Lerp = MathF.Lerp,
                    End = () => {
                        letter.Angle = (letter.Angle + MathF.PI * 2) % (MathF.PI * 2);
                        VerifyPositiveAngle(letter);
                    }
                }.Start(game, blockInput: true);
            }

            Letter[] letters = null!;
            letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
                var onPress = () => {
                    Debug.WriteLine(index.ToString());
                    game.playSound(SoundKind.Rotate);
                    for(int i = 0; i < 5; i++) {
                        int rotation = rotations[index][i];
                        if(rotation != 0) {
                            StartRotation(letters[i], rotation * MathF.PI / 2);

                        }
                    }
                };
                letter.GetPressState = TapInputState.GetPressReleaseHandler(letter, onPress, () => { });
                letter.HitTestVisible = true;
                letter.Angle = MathF.PI;
            });

            new WaitConditionAnimation(
                condition: delta => letters.All(l => {
                    if(l.Value is 'O' or 'H' && (MathF.FloatsEqual(MathF.PI, l.Angle) || MathF.FloatsEqual(-MathF.PI, l.Angle))) {
                        VerifyPositiveAngle(l);
                        return true;
                    }
                    return MathF.FloatsEqual(0, l.Angle) || MathF.FloatsEqual(MathF.PI * 2, l.Angle);
                })) {
                End = () => {
                    button.HitTestVisible = true;
                    foreach(var item in letters) {
                        item.HitTestVisible = false;
                    }
                }
            }.Start(game);

            var hints = new List<HintSymbol[]>();
            hints.Add(new HintSymbol[] { SvgIcon.Reload });
            for(int i = 0; i < solution.Length; i += 2) {
                hints.Add(new HintSymbol[] { "TOUCH"[solution[i]], SvgIcon.Tap }
                    .Concat(i < solution.Length - 1 ? new HintSymbol[] { "TOUCH"[solution[i + 1]], SvgIcon.Tap } : Enumerable.Empty<HintSymbol>() ).ToArray());
            }
            hints.Add(GameControllerExtensions.TapButtonHint);
            return hints.ToArray();
        }
        static void VerifyPositiveAngle(Letter letter) {
            //TODO use logging
            Debug.Assert(MathF.GreaterOrEqual(letter.Angle, 0), "Letter's angle is negative");
        }
    }
}

