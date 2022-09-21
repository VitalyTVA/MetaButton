using MetaArt.Core;
namespace ThatButtonAgain {
    public static class Level_RotateAroundLetter {
        public static LevelContext Load(GameController game) {
            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.HitTestVisible = false;

            //01234
            //21034
            //21430
            //41230
            //43210
            var indices = new[] { 4, 3, 2, 1, 0 };
            Letter[] letters = null!;
            letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(indices[index], button.Rect);
                var onPress = () => {
                    var leftLetter = (letters!).OrderByDescending(x => x.Rect.Left).FirstOrDefault(x => MathF.Less(x.Rect.Left, letter.Rect.Left));
                    var rightLetter = (letters!).OrderBy(x => x.Rect.Left).FirstOrDefault(x => MathF.Greater(x.Rect.Left, letter.Rect.Left));
                    if(leftLetter == null || rightLetter == null)
                        return;

                    bool isCenter = MathF.VectorsEqual(letter.Rect.Mid, button.Rect.Mid);
                    game.playSound(isCenter ? SoundKind.SwipeLeft : SoundKind.SwipeRight);
                    if(isCenter) {
                        game.AddRotateAnimation(letter, MathF.PI * 2, MathF.PI, rightLetter);
                        game.AddRotateAnimation(letter, MathF.PI, 0, leftLetter);
                    } else {
                        game.AddRotateAnimation(letter, 0, MathF.PI, rightLetter);
                        game.AddRotateAnimation(letter, MathF.PI, MathF.PI * 2, leftLetter);
                    }
                };
                letter.GetPressState = TapInputState.GetPressReleaseHandler(letter, onPress, () => { });
                letter.HitTestVisible = true;
            });

            new WaitConditionAnimation(
                condition: game.GetAreLettersInPlaceCheck(button.Rect, letters)) {
                End = () => {
                    button.HitTestVisible = true;
                    foreach(var item in letters) {
                        item.HitTestVisible = false;
                    }
                }
            }.Start(game);

            return new[] {
                new HintSymbol[] { SvgIcon.Reload },
                new HintSymbol[] { Solution[0], SvgIcon.Tap },
                new HintSymbol[] { Solution[1], SvgIcon.Tap },
                new HintSymbol[] { Solution[2], SvgIcon.Tap },
                new HintSymbol[] { Solution[3], SvgIcon.Tap },
                GameControllerExtensions.TapButtonHint,
            };
        }
        public const string Solution = "CHCO";
    }
}

