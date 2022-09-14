using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_RotatingArrow {
        public static LevelContext Load(GameController game) {
            var area = new LetterArea(LetterArea.CreateArrowDirectedLetters());

            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.HitTestVisible = false;
            button.Rect = game.GetContainingRect(area);
            button.Filled = false;

            var direction = Direction.Right;

            var arrow = new Letter {
                Value = '>',
                Rect = game.GetLetterTargetRect(2, button.Rect),
                ActiveRatio = 0,
                Angle = direction.ToAngle(),
            }.AddTo(game);
            arrow.Rect = arrow.Rect.SetLocation(new Vector2(arrow.Rect.Left, game.levelNumberLeterrs.First().Rect.Top));

            //HHTTHHTTCCOO
            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(2, button.Rect, row: index - 2);
                letter.HitTestVisible = true;
                letter.GetPressState = TapInputState.GetPressReleaseHandler(
                    letter,
                    onPress: () => {
                        if(!area.Move(letter.Value, direction)) {
                            game.playSound(SoundKind.Tap);
                            return;
                        }
                        //Debug.WriteLine(letter.Value);
                        game.StartLetterDirectionAnimation(letter, direction);
                        game.playSound(direction.GetSound());
                        direction = direction.RotateCounterClockwize();
                        arrow.Angle = direction.ToAngle();
                    },
                    onRelease: () => { }
                );
            });

            var condition = game.GetAreLettersInPlaceCheck(button.Rect, letters);
            new WaitConditionAnimation(condition: deltaTime => {
                game.ActivateInplaceLetters(letters);
                return condition(deltaTime);
            }) {
                End = () => {
                    button.Rect = game.GetButtonRect();
                    button.Filled = true;
                    button.HitTestVisible = true;
                    foreach(var item in letters) {
                        item.HitTestVisible = false;
                    }
                    game.playSound(SoundKind.SuccessSwitch);
                }
            }.Start(game);

            return new[] {
                new HintSymbol[] { 'H', 'H', 'T', 'T' },
                new HintSymbol[] { 'H', 'H', 'T', 'T' },
                new HintSymbol[] { 'C', 'C', 'O', 'O' },
                GameControllerExtensions.TapButtonHint,
            };
        }
    }
}

