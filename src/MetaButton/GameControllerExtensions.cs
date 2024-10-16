﻿using MetaArt.Core;
namespace ThatButtonAgain {
    public static class GameControllerExtensions {
        public static readonly HintSymbol[] TapButtonHint = new HintSymbol[] { SvgIcon.Button, SvgIcon.Tap };
        public static (int row, int col) GetOffset(this Direction direction) {
            return direction switch {
                Direction.Left => (0, -1),
                Direction.Right => (0, 1),
                Direction.Up => (-1, 0),
                Direction.Down => (1, 0),
                _ => throw new InvalidOperationException(),
            };
        }

        public static TElement AddTo<TElement>(this TElement element, GameController game) where TElement : Element {
            game.scene.AddElement(element);
            return element;
        }
        public static AnimationBase Start(this AnimationBase animation, GameController game, bool blockInput = false) {
            game.animations.AddAnimation(animation, blockInput);
            return animation;
        }
        public static LetterStyle ToStyle(char value) =>
            value switch {
                'T' => LetterStyle.Accent1,
                'O' => LetterStyle.Accent2,
                'U' => LetterStyle.Accent3,
                'C' => LetterStyle.Accent4,
                'H' => LetterStyle.Accent5,
                _ => throw new InvalidOperationException(),
            };
        public static Rect GetContainingRect(this GameController game, LetterArea area) {
            var buttonRect = game.GetButtonRect();
            var rowsOffset = area.Height / 2;
            var colsOffset = area.Width / 2 - 2;
            return buttonRect
                .ContainingRect(game.GetLetterTargetRect(-colsOffset, buttonRect, row: -rowsOffset))
                .ContainingRect(game.GetLetterTargetRect(area.Width - 1 - colsOffset, buttonRect, row: rowsOffset));
        }
        public static void ActivateInplaceLetters(this GameController game, Letter[] letters) {
            for(int i = 0; i < 5; i++) {
                letters[i].ActiveRatio =
                    MathF.VectorsEqual(game.GetLetterTargetRect(i, game.GetButtonRect()).Location, letters[i].Rect.Location)
                        ? 1 : 0;
            }
        }

        public static Rect GetButtonRect(this GameController game) {
            return new Rect(
                game.scene.width / 2 - game.buttonWidth / 2,
                game.scene.height / 2 - game.buttonHeight / 2,
                game.buttonWidth,
                game.buttonHeight
            );
        }

        public static Letter[] CreateLetters(this GameController game, Action<Letter, int> setUp, string word = "TOUCH") {
            int index = 0;
            var letters = new Letter[word.Length];
            foreach(var value in word) {
                var letter = new Letter() {
                    Value = value,
                };
                setUp(letter, index);
                game.scene.AddElement(letter);
                letters[index] = letter;
                index++;
            }
            return letters;
        }

        public static Func<TimeSpan, bool> GetAreLettersInPlaceCheck(this GameController game, Rect buttonRect, Letter[] letters) {
            var targetLocations = Enumerable.Range(0, 5)
                .Select(index => game.GetLetterTargetRect(index, buttonRect).Location)
                .ToArray();
            return deltaTime => letters.Select((l, i) => (l, i)).All(x => MathF.VectorsEqual(x.l.Rect.Location, targetLocations[x.i]));
        }

        public static void EnableButtonWhenInPlace(this GameController game, Rect buttonRect, Button dragableButton) {
            new WaitConditionAnimation(
                condition: deltaTime => MathF.VectorsEqual(dragableButton.Rect.Location, buttonRect.Location)) {
                End = () => {
                    dragableButton.IsEnabled = true;
                    dragableButton.GetPressState = game.GetClickHandler(() => game.StartNextLevelAnimation(), dragableButton);
                }
            }.Start(game);
        }
        public static Button CreateButton(this GameController game, Action click, Action? disabledClick = null) {
            var button = new Button {
                Rect = game.GetButtonRect(),
                HitTestVisible = true,
            };
            button.GetPressState = game.GetClickHandler(click, button, disabledClick);
            return button;
        }
        static Func<Vector2, InputState?> GetClickHandler(this GameController game, Action click, Button button, Action? disabledClick = null) {
            return TapInputState.GetClickHandler(
                button,
                () => {
                    if(button.IsEnabled)
                        click();
                },
                setState: isPressed => {
                    if(isPressed == button.IsPressed)
                        return;
                    button.IsPressed = isPressed;
                    if(isPressed && !button.IsEnabled) {
                        disabledClick?.Invoke();
                        game.playSound(SoundKind.ErrorClick);
                    }
                }
            );
        }

        public static void StartLetterDirectionAnimation(this GameController game, Letter letter, Direction direction, int count = 1, float? rowStep = null, float? colStep = null) {
            rowStep = rowStep ?? game.letterDragBoxHeight;
            colStep = colStep ?? game.letterHorzStep;
            var (directionY, directionX) = direction.GetOffset();
            var from = letter.Rect.Location;
            var to = letter.Rect.Location + new Vector2(colStep.Value * directionX * count, rowStep.Value * directionY * count);
            var animation = new LerpAnimation<Vector2> {
                Duration = TimeSpan.FromMilliseconds(150),
                From = from,
                To = to,
                SetValue = val => letter.Rect = letter.Rect.SetLocation(val),
                Lerp = Vector2.Lerp,
            }.Start(game, blockInput: true);
        }

        public static void AddRotateAnimation(this GameController game, Letter centerLetter, float fromAngle, float toAngle, Letter sideLetter) {
            new RotateAnimation {
                Duration = Constants.RotateAroundLetterDuration,
                From = fromAngle,
                To = toAngle,
                Center = centerLetter.Rect.Mid,
                Radius = (sideLetter.Rect.Mid - centerLetter.Rect.Mid).Length(),
                SetLocation = center => {
                    sideLetter.Rect = Rect.FromCenter(center, sideLetter.Rect.Size);
                }
            }.Start(game, blockInput: true);
        }

        public static void MakeDraggableLetter(this GameController game, Letter letter, int index, Button button, Action? onMove = null) {
            letter.HitTestVisible = true;
            letter.GetPressState = Element.GetAnchorAndSnapDragStateFactory(
                letter,
                () => 0,
                () => (game.letterSize * Constants.LetterSnapDistanceRatio, game.GetLetterTargetRect(index, button.Rect).Location),
                onElementSnap: element => {
                    element.HitTestVisible = false;
                    game.playSound(SoundKind.Snap);
                },
                onMove: onMove,
                coerceRectLocation: rect => rect.GetRestrictedLocation(game.scene.Bounds)
            );
        }

        public static Rect GetLetterTargetRect(this GameController game, float index, Rect buttonRect, bool squared = false, float row = 0) {
            float height = squared ? game.letterDragBoxWidth : game.letterDragBoxHeight;
            return Rect.FromCenter(
                buttonRect.Mid + new Vector2((index - 2) * game.letterHorzStep, 0),
                new Vector2(game.letterDragBoxWidth, height)
            ).Offset(new Vector2(0, row * height));
        }

        public static void VerifyExpectedLevelIndex(this GameController game, int expectedLevel) {
            if(game.LevelIndex != expectedLevel)
                throw new InvalidOperationException();
        }
        public static Rect LevelNumberElementRect(this GameController game) => game.levelNumberLeterrs.First().Rect;


        public static float GetSnapDistance(this GameController game) {
            return game.buttonHeight * Constants.ButtonAnchorDistanceRatio;
        }

        public static void SetUpLevelIndexButton(this GameController game, Letter letter, Vector2 location) {
            letter.Rect = new Rect(
                location,
                new Vector2(game.letterDragBoxWidth * Constants.LevelLetterRatio, game.letterDragBoxHeight * Constants.LevelLetterRatio)
            );
            letter.ActiveRatio = 0;
            letter.Scale = new Vector2(Constants.LevelLetterRatio);

        }

        public const string Tag_HintSymbol = "HintSymbol";
        public static IEnumerable<Element> CreateHintElements(this GameController game, HintSymbol[][]? symbols) {
#if DEBUG
            if(symbols == null)
                throw new InvalidOperationException(); //use log instead
#endif
            symbols = symbols ?? new[] { new HintSymbol[] { SvgIcon.Elipsis } };
            var buttonRect = game.GetButtonRect();

            for(int row = 0; row < symbols.Length; row++) {
                for(int col = 0; col < symbols[row].Length; col++) {
                    var hint = symbols[row][col];
                    var rect = game.GetLetterTargetRect(col, buttonRect, row: -3 + row);
                    Element element = hint switch {
                        (SvgIcon icon, null) =>
                            new SvgElement {
                                Svg = game.GetIcon(icon),
                                Rect = rect,
                                Size = game.letterSize * Constants.SvgIconScale,
                                Style = LetterStyle.Accent1,
                                Tag = Tag_HintSymbol,
                            },

                        (null, char letter) =>
                            new Letter {
                                Value = letter,
                                Rect = rect,
                                Scale = new Vector2(Constants.SvgIconScale),
                                Tag = Tag_HintSymbol,
                            },
                        _ => throw new InvalidOperationException()
                    };
                    element.AddTo(game);
                    yield return element;
                }
            }
        }

        public const string Tag_TimerLetter = "TimerLetter";
        public static (Letter[] letters, Action remove) CreateTimerLetters(this GameController game, Action onFinishTimer, Func<TimeSpan> getWaitTime) {
            var letters = game.CreateLetters((letter, index) => {
                letter.ActiveRatio = 0;
                letter.Rect = game.GetLetterTargetRect(index, game.GetButtonRect());
                letter.Tag = Tag_TimerLetter;
            }, "00:00");
            AnimationBase timerTimer = null!;
            void UpdateLetters() {
                var time = getWaitTime();
                if(time >= TimeSpan.Zero) {
                    letters[0].Value = (char)('0' + (time.Minutes / 10));
                    letters[1].Value = (char)('0' + (time.Minutes % 10));
                    letters[3].Value = (char)('0' + (time.Seconds / 10));
                    letters[4].Value = (char)('0' + (time.Seconds % 10));
                } else {
                    game.animations.RemoveAnimation(timerTimer!);
                    onFinishTimer();
                    foreach(var letter in letters) {
                        game.scene.RemoveElement(letter);
                    }
                }
            }
            UpdateLetters();
            timerTimer = DelegateAnimation.Timer(TimeSpan.FromMilliseconds(200), UpdateLetters).Start(game);
            return (letters, () => game.animations.RemoveAnimation(timerTimer));
        }

        public const string Tag_LevelNumberLetter = "LevelNumberLetter";
        public static (SvgElement bulb, List<Letter> levelNumberLetters) CreateLevelElements(this GameController game, Func<bool> isHintAvailable) {
            int digitIndex = 0;
            float offsetX = game.letterSize * Constants.LetterIndexOffsetRatioX;
            float offsetY = game.letterSize * Constants.LetterIndexOffsetRatioY;

            var letters = new List<Letter>();
            foreach(var digit in game.LevelIndex.ToString()) {
                var levelNumberElement = new Letter {
                    Value = digit,
                    HitTestVisible = true,
                    Tag = Tag_LevelNumberLetter,
                };
                game.SetUpLevelIndexButton(
                    levelNumberElement,
                    new Vector2(
                        offsetX + digitIndex * game.letterDragBoxWidth * Constants.LevelLetterRatio,
                        offsetY
                    )
                );
                game.scene.AddElement(levelNumberElement);
                digitIndex++;
                letters.Add(levelNumberElement);
            }

            var bulb = new SvgElement {
                HitTestVisible = true,
                Rect = new Rect(
                    game.scene.width - offsetX - game.letterDragBoxWidth * Constants.LevelLetterRatio,
                    offsetY,
                    game.letterDragBoxWidth * Constants.LevelLetterRatio,
                    game.letterDragBoxHeight * Constants.LevelLetterRatio
                ),
                Size = game.letterSize * Constants.LevelLetterRatio,
                Style = LetterStyle.Inactive,
            }.AddTo(game);
            void UpdateHintBulb() {
                bulb.Svg = game.GetIcon(isHintAvailable() ? SvgIcon.Bulb : SvgIcon.BulbOff);
            }
            UpdateHintBulb();
            DelegateAnimation.Timer(TimeSpan.FromSeconds(1), UpdateHintBulb).Start(game);
            return (bulb, letters);
        }

    }
}

