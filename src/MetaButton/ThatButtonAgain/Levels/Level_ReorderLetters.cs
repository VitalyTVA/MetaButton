using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_ReorderLetters {
        public static LevelContext Load_2And1(GameController game) {
            return LoadCore(
                game,
                LetterArea.CreateSwapPlusShapeArea(),
                (button, letters) => new[] {
                    button.Rect.Location,

                    new Vector2(letters[2].Rect.Left, button.Rect.Top),
                    new Vector2(letters[2].Rect.Left, letters[2].Rect.Top - game.letterDragBoxHeight * 2),
                    new Vector2(letters[2].Rect.Right, letters[2].Rect.Top - game.letterDragBoxHeight * 2),
                    new Vector2(letters[2].Rect.Right, button.Rect.Top),

                    button.Rect.TopRight,
                    button.Rect.BottomRight,

                    new Vector2(letters[2].Rect.Right, button.Rect.Bottom),
                    new Vector2(letters[2].Rect.Right, letters[2].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[2].Rect.Left, letters[2].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[2].Rect.Left, button.Rect.Bottom),

                    button.Rect.BottomLeft,
                },
                new[] {
                    new HintSymbol[] { 'U', SvgIcon.Up, SvgIcon.Up },
                    new HintSymbol[] { 'O', SvgIcon.Left, SvgIcon.Down },
                    new HintSymbol[] { 'T', SvgIcon.Left, SvgIcon.Left, SvgIcon.Up },
                    new HintSymbol[] { 'C', SvgIcon.Right, SvgIcon.Right, SvgIcon.Right }, 
                }
            );
        }
        public static LevelContext Load_3And2(GameController game) {
            return LoadCore(
                game, 
                LetterArea.CreateSwapWShapeArea(),
                (button, letters) => new[] {
                    button.Rect.Location,

                    new Vector2(letters[4].Rect.Left, button.Rect.Top),
                    new Vector2(letters[4].Rect.Left, letters[4].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[4].Rect.Right, letters[4].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[4].Rect.Right, button.Rect.Top),

                    new Vector2(letters[2].Rect.Left, button.Rect.Top),
                    new Vector2(letters[2].Rect.Left, letters[2].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[2].Rect.Right, letters[2].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[2].Rect.Right, button.Rect.Top),

                    new Vector2(letters[0].Rect.Left, button.Rect.Top),
                    new Vector2(letters[0].Rect.Left, letters[0].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[0].Rect.Right, letters[0].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[0].Rect.Right, button.Rect.Top),

                    button.Rect.TopRight,
                    button.Rect.BottomRight,

                    new Vector2(letters[1].Rect.Right, button.Rect.Bottom),
                    new Vector2(letters[1].Rect.Right, letters[1].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[1].Rect.Left, letters[1].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[1].Rect.Left, button.Rect.Bottom),

                    new Vector2(letters[3].Rect.Right, button.Rect.Bottom),
                    new Vector2(letters[3].Rect.Right, letters[1].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[3].Rect.Left, letters[1].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[3].Rect.Left, button.Rect.Bottom),

                    button.Rect.BottomLeft,
                },
                new[] {
                    new HintSymbol[] { 'O', SvgIcon.Down, 'U', SvgIcon.Up },
                    new HintSymbol[] { 'C', SvgIcon.Down, 'H', SvgIcon.Up },
                    new HintSymbol[] { 'T', SvgIcon.Left,  SvgIcon.Repeat, '4' },
                }
            );
        }
        public static LevelContext Load_2And2(GameController game) {
            return LoadCore(
                game, 
                LetterArea.CreateSwapHShapeArea(),
                (button, letters) => new[] {
                    button.Rect.Location,

                    new Vector2(letters[3].Rect.Left, button.Rect.Top),
                    new Vector2(letters[3].Rect.Left, letters[3].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[3].Rect.Right, letters[3].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[3].Rect.Right, button.Rect.Top),

                    new Vector2(letters[1].Rect.Left, button.Rect.Top),
                    new Vector2(letters[1].Rect.Left, letters[1].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[1].Rect.Right, letters[1].Rect.Top - game.letterDragBoxHeight),
                    new Vector2(letters[1].Rect.Right, button.Rect.Top),

                    button.Rect.TopRight,
                    button.Rect.BottomRight,

                    new Vector2(letters[1].Rect.Right, button.Rect.Bottom),
                    new Vector2(letters[1].Rect.Right, letters[1].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[1].Rect.Left, letters[1].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[1].Rect.Left, button.Rect.Bottom),

                    new Vector2(letters[3].Rect.Right, button.Rect.Bottom),
                    new Vector2(letters[3].Rect.Right, letters[1].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[3].Rect.Left, letters[1].Rect.Bottom + game.letterDragBoxHeight),
                    new Vector2(letters[3].Rect.Left, button.Rect.Bottom),

                    button.Rect.BottomLeft,
                },
                new[] { 
                    new HintSymbol[] { 'O', SvgIcon.Down }, 
                    new HintSymbol[] { 'U', SvgIcon.Right, SvgIcon.Up }, 
                    new HintSymbol[] { 'C', SvgIcon.Down }, 
                    new HintSymbol[] { 'H', SvgIcon.Right, SvgIcon.Up }, 
                    //new HintSymbol[] { 'T', SvgIcon.Left,  SvgIcon.Repeat, '4' },
                }
            );
        }
        static LevelContext LoadCore(GameController game, char[][] chars, Func<Button, Letter[], Vector2[]> getPoints, IEnumerable<HintSymbol[]> solution) {
            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.HitTestVisible = true;
            button.IsVisible = false;

            var area = new LetterArea(chars);
            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(4 - index, button.Rect);
                letter.HitTestVisible = true;
                letter.GetPressState = (startPoint, releaseState) => {
                    return new DragInputState(
                        startPoint,
                        onDrag: delta => {
                            var direction = DirectionExtensions.GetSwipeDirection(ref delta, game.GetSnapDistance());

                            if(direction == null)
                                return true;

                            if(!area.Move(letter.Value, direction.Value))
                                return true;

                            game.StartLetterDirectionAnimation(letter, direction.Value);
                            game.playSound(direction.Value.GetSound());
                            return false;
                        },
                        onRelease: delta => {
                        },
                        releaseState);

                };
            });

            var pathElement = new PathElement(getPoints(button, letters));
            game.scene.AddElementBehind(pathElement);

            new WaitConditionAnimation(
                condition: game.GetAreLettersInPlaceCheck(button.Rect, letters)) {
                End = () => {
                    pathElement.IsVisible = false;
                    button.IsVisible = true;
                    foreach(var item in letters) {
                        item.HitTestVisible = false;
                    }
                    game.playSound(SoundKind.SuccessSwitch);
                }
            }.Start(game);

            var hints = new List<HintSymbol[]>();
            hints.Add(new HintSymbol[] { SvgIcon.Reload });
            hints.AddRange(solution);
            hints.Add(new HintSymbol[] { SvgIcon.Elipsis });
            hints.Add(ElementExtensions.TapButtonHint);
            return hints.ToArray();
        }
    }
}

