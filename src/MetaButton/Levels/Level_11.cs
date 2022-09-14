using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_11 {
        public static LevelContext Load(GameController game) {
            game.VerifyExpectedLevelIndex(11);
            bool win = false;
            var button = game.CreateButton(() => {
                if(win)
                    game.StartNextLevelAnimation();
                else
                    game.StartNextLevelFalseAnimation();
            }).AddTo(game);

            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
            }, "TOUCH0");

            void SetZeroDigit() {
                letters.Last().Rect = letters![1].Rect;
                letters.Last().HitTestVisible = true;
                letters[1].Opacity = 0;
            }
            SetZeroDigit();
            var maxDistance = game.buttonWidth * Constants.ZeroDigitMaxDragDistance;
            var minDistance = game.buttonHeight;
            letters.Last().GetPressState = Element.GetAnchorAndSnapDragStateFactory(
                letters.Last(),
                () => game.GetSnapDistance(),
                () => null,
                onElementSnap: element => { },
                onMove: () => {
                    var amount = win
                        ? 1
                        : Math.Min(maxDistance, (letters[1].Rect.Location - letters.Last().Rect.Location).Length() - minDistance) / maxDistance;
                    amount = Math.Max(0, amount);
                    letters[1].Opacity = amount;
                    if(MathF.FloatsEqual(amount, 1)) {
                        if(!win)
                            game.playSound(SoundKind.SuccessSwitch);
                        win = true;
                        game.scene.RemoveElement(letters.Last());
                    }
                },
                coerceRectLocation: rect => rect.GetRestrictedLocation(game.scene.Bounds),
                onRelease: anchored => {
                    if(win)
                        return false;
                    if(!anchored)
                        game.playSound(SoundKind.Snap);
                    SetZeroDigit();
                    return true;
                },
                onClick: () => game.StartNextLevelFalseAnimation()
            );

            return new[] {
                new HintSymbol[] { '0', SvgIcon.MoveToTop },
                GameControllerExtensions.TapButtonHint,
            };
        }
    }
}

