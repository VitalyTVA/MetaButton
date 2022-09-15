using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_ReflectedButton {
        public static LevelContext Load(GameController game) {
            Action<bool, bool> syncButtons = null!;

            (Button button, Letter[] letters, Action syncLettersOnMoveAction) CreateButtonAndLetters(
                Vector2 offset,
                string word,
                bool flipV,
                bool flipH
            ) {
                Rect buttonRect = game.GetButtonRect();
                var button = new Button {
                    Rect = buttonRect.Offset(offset),
                    IsEnabled = false,
                    HitTestVisible = true,
                }.AddTo(game);
                var letters = game.CreateLetters((letter, index) => {
                    letter.Rect = game.GetLetterTargetRect(index, button.Rect);
                    letter.Scale = new Vector2(flipH ? -1 : 1, flipV ? -1 : 1);
                }, word);
                Action syncLettersOnMoveAction = Element.CreateSetOffsetAction(button, letters);
                button.GetPressState = Element.GetAnchorAndSnapDragStateFactory(
                    button,
                    () => flipH && flipV ? game.GetSnapDistance() : 0,
                    () => flipH || flipV ? null : (game.GetSnapDistance(), buttonRect.Location),
                    onElementSnap: x => game.playSound(SoundKind.Snap),
                    onMove: () => {
                        syncLettersOnMoveAction();
                        syncButtons(flipV, flipH);
                    },
                    coerceRectLocation: rect => {
                        if(rect.Left < 0 && rect.Top < 0) {
                            if(rect.Left < rect.Top)
                                return new Vector2(rect.Left, 0);
                            else
                                return new Vector2(0, rect.Top);
                        }
                        return rect.Location;
                    }
                );
                return (button, letters, syncLettersOnMoveAction);
            }

            var invisiblePosition = new Vector2(-3000, -3000);
            var flippedHV = CreateButtonAndLetters(new Vector2(0, 0), "HCUOT", flipV: true, flipH: true);
            var flippedH = CreateButtonAndLetters(invisiblePosition, "HCUOT", flipV: false, flipH: true);
            var flippedV = CreateButtonAndLetters(invisiblePosition, "TOUCH", flipV: true, flipH: false);

            var normal = CreateButtonAndLetters(invisiblePosition, "TOUCH", flipV: false, flipH: false);

            void SyncLocations(
                Button movingButton,
                (Button button, Letter[] letters, Action syncLettersOnMoveAction) horizontal,
                (Button button, Letter[] letters, Action syncLettersOnMoveAction) vertical
            ) {
                horizontal.button.Rect = new Rect(invisiblePosition, horizontal.button.Rect.Size);
                vertical.button.Rect = new Rect(invisiblePosition, vertical.button.Rect.Size);

                if(movingButton.Rect.Left + movingButton.Rect.Width > game.scene.width)
                    horizontal.button.Rect = movingButton.Rect.Offset(new Vector2(-game.scene.width, 0));
                else if(movingButton.Rect.Left < 0)
                    horizontal.button.Rect = movingButton.Rect.Offset(new Vector2(game.scene.width, 0));

                else if(movingButton.Rect.Top + movingButton.Rect.Height > game.scene.height)
                    vertical.button.Rect = movingButton.Rect.Offset(new Vector2(0, -game.scene.height));
                else if(movingButton.Rect.Top < 0)
                    vertical.button.Rect = movingButton.Rect.Offset(new Vector2(0, game.scene.height));

                horizontal.syncLettersOnMoveAction();
                vertical.syncLettersOnMoveAction();
            }

            syncButtons = (flipV, flipH) => {
                switch((flipV, flipH)) {
                    case (true, true):
                        SyncLocations(flippedHV.button, flippedH, flippedV);
                        break;
                    case (true, false):
                        SyncLocations(flippedV.button, normal, flippedHV);
                        break;
                    case (false, true):
                        SyncLocations(flippedH.button, flippedHV, normal);
                        break;
                    case (false, false):
                        SyncLocations(normal.button, flippedV, flippedH);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            };

            game.EnableButtonWhenInPlace(flippedHV.button.Rect, normal.button);

            return new[] {
                new HintSymbol[] { SvgIcon.Button, SvgIcon.MoveToBottom },
                new HintSymbol[] { SvgIcon.Button, SvgIcon.MoveToRight },
                GameControllerExtensions.TapButtonHint,
            };
        }
    }
}

