using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_LettersBehindButton {
        public static LevelContext Load(GameController game) {
            var buttonRect = game.GetButtonRect();

            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, buttonRect);
            });
            (float, Vector2)? snapInfo = default;
            float snapDistance = game.GetSnapDistance();
            var dragableButton = new Button {
                Rect = buttonRect,
                IsEnabled = false,
                HitTestVisible = true,
            }.AddTo(game);
            dragableButton.GetPressState = Element.GetAnchorAndSnapDragStateFactory(
                dragableButton,
                () => snapDistance,
                () => snapInfo,
                x => game.playSound(SoundKind.Snap),
                coerceRectLocation: rect => rect.GetRestrictedLocation(game.scene.Bounds.Inflate(dragableButton.Rect.Size * Constants.ButtonOutOfBoundDragRatio))
            );

            new WaitConditionAnimation(
                condition: deltaTime => letters.All(l => !l.Rect.Intersects(dragableButton.Rect))) {
                End = () => {
                    game.scene.SendToBack(dragableButton);
                    snapInfo = (snapDistance, buttonRect.Location);
                    game.EnableButtonWhenInPlace(buttonRect, dragableButton);

                }
            }.Start(game);

            return new[] {
                new HintSymbol[] { SvgIcon.Button, SvgIcon.DragDown },
                new HintSymbol[] { SvgIcon.Button, SvgIcon.DragUp },
                GameControllerExtensions.TapButtonHint,
            };

        }
    }
}

