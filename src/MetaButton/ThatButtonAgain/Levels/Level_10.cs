using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_10 {
        public static LevelContext Load(GameController game) {
            game.VerifyExpectedLevelIndex(10);
            game.RemoveLastLevelLetter();

            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.IsEnabled = false;

            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
                if(index == 1) {
                    game.SetUpLevelIndexButton(letter, new Vector2(game.levelNumberElementRect.Right, game.levelNumberElementRect.Top));
                    var targetLocation = game.GetLetterTargetRect(index, button.Rect).Location;
                    var initialLocation = letter.Rect.Location;
                    float initialScale = letter.Scale.X;
                    var initialSize = letter.Rect.Size;
                    game.MakeDraggableLetter(
                        letter,
                        index,
                        button,
                        onMove: () => {
                            var amount = 1 - (targetLocation - letter.Rect.Location).Length() / (targetLocation - initialLocation).Length();
                            letter.ActiveRatio = amount;
                            letter.Scale = new Vector2(MathFEx.Lerp(initialScale, 1, amount));
                            letter.Rect = letter.Rect.SetSize(Vector2.Lerp(initialSize, new Vector2(game.letterDragBoxWidth, game.letterDragBoxHeight), amount));
                        }
                    );
                }
            });
            new WaitConditionAnimation(condition: game.GetAreLettersInPlaceCheck(button.Rect, letters)) {
                End = () => button.IsEnabled = true
            }.Start(game);

            return new[] {
                new HintSymbol[] { 'O', SvgIcon.DragDown, SvgIcon.Button },
                ElementExtensions.TapButtonHint,
            };
        }
    }
}

