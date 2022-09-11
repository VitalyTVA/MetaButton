using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_RotateAroundLetter {
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
                    var leftLetter = (letters!).OrderByDescending(x => x.Rect.Left).FirstOrDefault(x => MathFEx.Less(x.Rect.Left, letter.Rect.Left));
                    var rightLetter = (letters!).OrderBy(x => x.Rect.Left).FirstOrDefault(x => MathFEx.Greater(x.Rect.Left, letter.Rect.Left));
                    if(leftLetter == null || rightLetter == null)
                        return;

                    bool isCenter = MathFEx.VectorsEqual(letter.Rect.Mid, button.Rect.Mid);
                    game.playSound(isCenter ? SoundKind.SwipeLeft : SoundKind.SwipeRight);
                    if(isCenter) {
                        game.AddRotateAnimation(letter, MathFEx.PI * 2, MathFEx.PI, rightLetter);
                        game.AddRotateAnimation(letter, MathFEx.PI, 0, leftLetter);
                    } else {
                        game.AddRotateAnimation(letter, 0, MathFEx.PI, rightLetter);
                        game.AddRotateAnimation(letter, MathFEx.PI, MathFEx.PI * 2, leftLetter);
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
                new HintSymbol[] { 'C', SvgIcon.Tap },
                new HintSymbol[] { 'H', SvgIcon.Tap },
                new HintSymbol[] { 'C', SvgIcon.Tap },
                new HintSymbol[] { 'O', SvgIcon.Tap },
                ElementExtensions.TapButtonHint,
            };
        }
    }
}

