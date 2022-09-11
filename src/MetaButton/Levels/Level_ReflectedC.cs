using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_ReflectedC {
        public static LevelContext Load(GameController game) {
            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.IsEnabled = false;

            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
            }, "TCUCHC");
            letters.Last().HitTestVisible = true;
            letters.Last().Scale = new Vector2(-1, 1);
            letters[1].Offset = new Vector2(-Constants.ReflectedCOffset / 2, 0);
            letters[3].Offset = new Vector2(-Constants.ReflectedCOffset / 2, 0);
            letters.Last().Offset = new Vector2(-Constants.ReflectedCOffset / 2, 0);


            void ResetReflectedLetterRect() {
                letters.Last().Rect = letters![1].Rect;
            }
            ResetReflectedLetterRect();
            letters.Last().GetPressState = Element.GetAnchorAndSnapDragStateFactory(
                letters.Last(),
                () => game.GetSnapDistance(),
                () => (game.GetSnapDistance(), letters![3].Rect.Location),
                onElementSnap: element => {
                    game.playSound(SoundKind.SuccessSwitch);
                    letters.Last().HitTestVisible = false;

                    letters[2].GetPressState = TapInputState.GetPressReleaseHandler(
                        letters[2],
                        () => {
                            letters[2].HitTestVisible = false;
                            game.playSound(SoundKind.SwipeLeft);
                            game.AddRotateAnimation(letters[2], MathF.PI * 2, MathF.PI, letters[3]);
                            game.AddRotateAnimation(letters[2], MathF.PI * 2, MathF.PI, letters.Last());
                            game.AddRotateAnimation(letters[2], MathF.PI, 0, letters[1]);
                        },
                        () => {
                        }
                    );
                    letters[2].HitTestVisible = true;
                },
                onMove: () => {
                },
                coerceRectLocation: rect => rect.GetRestrictedLocation(game.scene.Bounds).SetY(letters[0].Rect.Top),
                onRelease: anchored => {
                    if(anchored)
                        game.playSound(SoundKind.ErrorClick);
                    else
                        game.playSound(SoundKind.Snap);
                    ResetReflectedLetterRect();
                    return true;
                }
            );

            var expectedLettersOrder = new[] { 0, 3, 2, 1, 4 }.Select(i => letters[i]).ToArray();

            new WaitConditionAnimation(
                condition: game.GetAreLettersInPlaceCheck(button.Rect, expectedLettersOrder)) {
                End = () => {
                    button.IsEnabled = true;
                }
            }.Start(game);

            return new[] {
                new HintSymbol[] { 'O', SvgIcon.DragRight, 'C' },
                new HintSymbol[] { 'U', SvgIcon.Tap },
                ElementExtensions.TapButtonHint,
            };
        }
    }
}

