using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_DragLetters {
        public static LevelContext Load_Normal(GameController game) {
            var points = new[] {
                (-2.1f, 2.3f),
                (-1.5f, -2.7f),
                (.7f, -1.5f),
                (1.3f, 3.4f),
                (2.3f, -3.4f),
            };
            LoadCore(
                game, 
                points, 
                buttonOffset: 0,
                onElementsCreated: (buttonRect, letters) => { },
                getOnMoveHandler: (letter, index) => null
            );
            return new[] {
                new HintSymbol[] { 'T', SvgIcon.Drag, SvgIcon.Button },
                new HintSymbol[] { SvgIcon.Elipsis },
                GameControllerExtensions.TapButtonHint,
            };
        }

        public static LevelContext Load_Inverted(GameController game) {
            Rect ReflectRect(Rect rect) => MathF.Reflect(rect, game.scene.Bounds.Mid);

            new Line { From = new Vector2(0, game.scene.height / 2), To = new Vector2(game.scene.width, game.scene.height / 2) }.AddTo(game);
            foreach(var letter in game.levelNumberLeterrs) {
                letter.Rect = ReflectRect(letter.Rect);
                letter.Scale = new Vector2(-1, -1);
            }

            var points = new[] {
                (-2.1f, 2.9f),
                (-1.5f, 1.7f),
                (.7f, 1.5f),
                (.8f, 3.1f),
                (2.3f, 3.4f),
            };
            Letter[] letters2 = null!;
            Rect buttonRectStore = default;
            LoadCore(
                game,
                points,
                buttonOffset: game.buttonHeight * .7f,
                onElementsCreated: (buttonRect, letters) => {
                    buttonRectStore = buttonRect;
                    letters2 = letters.Select(letter => {
                        letter.Opacity = 0;
                        return new Letter {
                            Value = letter.Value,
                            Rect = ReflectRect(letter.Rect),
                            Scale = new Vector2(-1, -1),
                        }.AddTo(game);
                    })
                    .ToArray();
                },
                getOnMoveHandler: (letter, index) => {
                    return () => {
                        letters2[index].Rect = ReflectRect(letter.Rect);
                        if(MathF.RectssEqual(letter.Rect, game.GetLetterTargetRect(index, buttonRectStore))) {
                            var startLocation = letters2[index].Rect.Location;
                            new LerpAnimation<float> {
                                From = 0,
                                To = 1,
                                Lerp = MathF.Lerp,
                                Duration = TimeSpan.FromMilliseconds(150),
                                SetValue = value => {
                                    letters2[index].Rect = letters2[index].Rect.SetLocation(Vector2.Lerp(startLocation, letter.Rect.Location, value));
                                    letters2[index].Scale = Vector2.Lerp(new Vector2(-1), new Vector2(1), value);
                                }
                            }.Start(game);
                            //letters2[index].IsVisible = false;
                            //letter.Opacity = 1;
                        }
                    };
                }
            );
            return new[] {
                new HintSymbol[] {SvgIcon.Mirror },
                new HintSymbol[] { '?', SvgIcon.Drag, 'T', SvgIcon.Button },
                new HintSymbol[] { SvgIcon.Elipsis },
                GameControllerExtensions.TapButtonHint,
            };
        }

        static void LoadCore(
            GameController game,
            (float, float)[] points,
            float buttonOffset,
            Action<Rect, Letter[]> onElementsCreated,
            Func<Letter, int, Action?> getOnMoveHandler
        ) {
            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.Rect = button.Rect.Offset(new Vector2(0, buttonOffset));
            button.IsEnabled = false;


            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = Rect.FromCenter(
                    new Vector2(button.Rect.MidX + game.letterDragBoxWidth * points[index].Item1, button.Rect.MidY + game.letterDragBoxHeight * points[index].Item2),
                    new Vector2(game.letterDragBoxWidth, game.letterDragBoxHeight)
                ).GetRestrictedRect(game.scene.Bounds);
                game.MakeDraggableLetter(letter, index, button, onMove: getOnMoveHandler(letter, index));
            });

            onElementsCreated(button.Rect, letters);

            new WaitConditionAnimation(condition: game.GetAreLettersInPlaceCheck(button.Rect, letters)) {
                End = () => button.IsEnabled = true
            }.Start(game);
        }
    }
}

