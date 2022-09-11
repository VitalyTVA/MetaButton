using MetaArt.Core;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CallerArgumentExpressionAttribute : Attribute {
        public string ParameterName {
            get; set;
        }
        public CallerArgumentExpressionAttribute(string parameterName) {
            ParameterName = parameterName;
        }
    }
}
namespace ThatButtonAgain {
    public enum SoundKind {
        Win1,
        Cthulhu,
        Tap,
        ErrorClick,
        Snap,
        SuccessSwitch,
        SwipeRight,
        SwipeLeft,
        Hover,
        BrakeBall,
        Rotate,
        Merge,
    }
    public enum SvgIcon {
        Bulb,
        BulbOff,
        Tap,
        Drag,
        DragDown,
        DragUp,
        DragLeft,
        DragRight,
        Elipsis,
        Right,
        Left,
        Up,
        Down,
        Button,
        Reload,
        Repeat,
        Timer,
        MoveToBottom,
        MoveToTop,
        MoveToRight,
        MoveToLeft,
        Alert,
        Arrows,
        Ball,
        Fast,
        Mirror,
    }
    public class GameController {
        public static readonly (Func<GameController, LevelContext> action, string name)[] Levels = new [] {
            RegisterLevel(Level_Touch.Load),
            RegisterLevel(Level_DragLetters.Load_Normal),
            RegisterLevel(Level_Capital.Load_16xClick),
            RegisterLevel(Level_RotateAroundLetter.Load),
            RegisterLevel(Level_LettersBehindButton.Load),
            RegisterLevel(Level_ClickInsteadOfTouch.Load),
            RegisterLevel(Level_RandomButton.Load_Simple),
            RegisterLevel(Level_ReflectedButton.Load),
            RegisterLevel(Level_Capital.Load_Mod2Vectors),
            RegisterLevel(Level_Matrix.Load_FindWord),
            RegisterLevel(Level_10.Load),
            RegisterLevel(Level_11.Load),
            RegisterLevel(Level_ScrollLetters.Load_Trivial),
            RegisterLevel(Level_ReorderLetters.Load_2And2),
            RegisterLevel(Level_ReflectedC.Load),
            RegisterLevel(Level_Balls.Load_KeepO),
            RegisterLevel(Level_RotatingLetters.Load_Easy),
            RegisterLevel(Level_RotatingArrow.Load),
            RegisterLevel(Level_Calculator.Load),
            RegisterLevel(Level_16Game.Load_4x4),
            RegisterLevel(Level_Balls.Load_20Level),
            RegisterLevel(Level_DecreaseLevelNumber.Load),
            RegisterLevel(Level_DragLetters.Load_Inverted),
            RegisterLevel(Level_RandomButton.Load_Hard),
            RegisterLevel(Level_16Game.Load_3x3),
            RegisterLevel(Level_Reorder.Load_MoveInLine),
            RegisterLevel(Level_Matrix.Load_3InARow),
            RegisterLevel(Level_Reorder.Load_MoveAll),
            RegisterLevel(Level_ReorderLetters.Load_3And2),
            RegisterLevel(Level_ReorderLetters.Load_2And1),
            RegisterLevel(Level_Reorder.Load_Sokoban),
            RegisterLevel(Level_ScrollLetters.Load_Hard),
            RegisterLevel(Level_ScrollLetters.Load_Extreme),
            RegisterLevel(Level_RotatingLetters.Load_Medium),
            RegisterLevel(Level_RotatingLetters.Load_Hard),
            RegisterLevel(Level_16Game.Load_3x3Hard),
            RegisterLevel(Level_16Game.Load_3x3Extreme),

        };
        static (Func<GameController, LevelContext>, string) RegisterLevel(Func<GameController, LevelContext> action, [CallerArgumentExpression("action")] string name = "") {
            return (action, name.Replace("Level_", null).Replace(".Load", null));
        }
        public readonly Scene scene;
        internal readonly AnimationsController animations = new();

        public readonly float letterVerticalOffset;
        internal readonly float buttonWidth;
        internal readonly float buttonHeight;
        public readonly float letterSize;
        internal readonly float letterDragBoxHeight;
        internal readonly float letterDragBoxWidth;
        readonly float letterHorzStep;
        internal readonly Action<SoundKind> playSound;
        readonly SvgDrawing cthulhuSvg;
        readonly Func<Stream, SvgDrawing> createSvg;
        readonly Dictionary<SvgIcon, SvgDrawing> icons;

        public GameController(float width, float height, Action<SoundKind> playSound, Func<Stream, SvgDrawing> createSvg) {
            scene = new Scene(width, height, () => animations.AllowInput);

            buttonWidth = scene.width * Constants.ButtonRelativeWidth;
            buttonHeight = buttonWidth * Constants.ButtonHeightRatio;
            letterSize = buttonHeight * Constants.LetterHeightRatio;
            letterDragBoxHeight = buttonHeight * Constants.LetterDragBoxHeightRatio;
            letterDragBoxWidth = buttonHeight * Constants.LetterDragBoxWidthRatio;
            letterVerticalOffset = letterSize * Constants.LetterVerticalOffsetRatio;
            letterHorzStep = buttonWidth * Constants.LetterHorizontalStepRatio;

            this.playSound = playSound;
            this.createSvg = createSvg;

            cthulhuSvg = CreateSvg("Cthulhu");

            icons = Enum.GetValues(typeof(SvgIcon))
                .Cast<SvgIcon>()
                .ToDictionary(x => x, x => CreateSvg(x.ToString()));
        }

        SvgDrawing CreateSvg(string name) => createSvg(Utils.GetStream(typeof(GameController), name + ".svg"));

        public void NextFrame(float deltaTime) {
            animations.Next(TimeSpan.FromMilliseconds(deltaTime));
        }

        GameController game => this;

        internal float GetSnapDistance() {
            return buttonHeight * Constants.ButtonAnchorDistanceRatio;
        }

        internal void StartLetterDirectionAnimation(Letter letter, Direction direction, int count = 1, float? rowStep = null, float? colStep = null) {
            rowStep = rowStep ?? letterDragBoxHeight;
            colStep = colStep ?? letterHorzStep;
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

        internal void AddRotateAnimation(Letter centerLetter, float fromAngle, float toAngle, Letter sideLetter) {
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

        internal void MakeDraggableLetter(Letter letter, int index, Button button, Action? onMove = null) {
            letter.HitTestVisible = true;
            letter.GetPressState = Element.GetAnchorAndSnapDragStateFactory(
                letter,
                () => 0,
                () => (letterSize * Constants.LetterSnapDistanceRatio, GetLetterTargetRect(index, button.Rect).Location),
                onElementSnap: element => {
                    element.HitTestVisible = false;
                    OnElementSnap(element);
                },
                onMove: onMove,
                coerceRectLocation: rect => rect.GetRestrictedLocation(scene.Bounds)
            );
        }

        internal void OnElementSnap(Element element) {
            playSound(SoundKind.Snap);
        }

        internal void EnableButtonWhenInPlace(Rect buttonRect, Button dragableButton) {
            new WaitConditionAnimation(
                condition: deltaTime => MathFEx.VectorsEqual(dragableButton.Rect.Location, buttonRect.Location)) {
                    End = () => {
                        dragableButton.IsEnabled = true;
                        dragableButton.GetPressState = GetClickHandler(StartNextLevelAnimation, dragableButton);
                    }
            }.Start(game);
        }

        internal Func<TimeSpan, bool> GetAreLettersInPlaceCheck(Rect buttonRect, Letter[] letters) {
            var targetLocations = GetLettersTargetLocations(buttonRect);
            return deltaTime => letters.Select((l, i) => (l, i)).All(x => MathFEx.VectorsEqual(x.l.Rect.Location, targetLocations[x.i]));
        }

        Vector2[] GetLettersTargetLocations(Rect buttonRect) => 
            Enumerable.Range(0, 5)
                .Select(index => GetLetterTargetRect(index, buttonRect).Location)
                .ToArray();

        internal Rect GetLetterTargetRect(float index, Rect buttonRect, bool squared = false, float row = 0) {
            float height = squared ? letterDragBoxWidth : letterDragBoxHeight;
            return Rect.FromCenter(
                buttonRect.Mid + new Vector2((index - 2) * letterHorzStep, 0),
                new Vector2(letterDragBoxWidth, height)
            ).Offset(new Vector2(0, row * height));
        }

        internal Button CreateButton(Action click, Action? disabledClick = null) {
            var button = new Button {
                Rect = GetButtonRect(),
                HitTestVisible = true,
            };
            button.GetPressState = GetClickHandler(click, button, disabledClick);
            return button;
        }

        Func<Vector2, NoInputState, InputState> GetClickHandler(Action click, Button button, Action? disabledClick = null) {
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
                        playSound(SoundKind.ErrorClick);
                    }
                }
            );
        }

        internal Rect GetButtonRect() {
            return new Rect(
                                        scene.width / 2 - buttonWidth / 2,
                                        scene.height / 2 - buttonHeight / 2,
                                        buttonWidth,
                                        buttonHeight
                                    );
        }

        void StartFade(float from, float to, Action end, TimeSpan duration) {
            var element = new FadeOutElement() { Rect = new Rect(0, 0, scene.width, scene.height), Opacity = from };
            var animation = new LerpAnimation<float> {
                Duration = duration,
                From = from,
                To = to,
                SetValue = value => element.Opacity = value,
                Lerp = MathFEx.Lerp,
                End = () => {
                    scene.RemoveElement(element);
                    end();
                }
            }.Start(game);
            scene.AddElement(element);
        }
        internal void StartNextLevelAnimation() {
            StartFade(0, 255, () => SetLevel(levelIndex + 1), Constants.FadeOutDuration);
            playSound(SoundKind.Win1);
        }
        internal void StartNextLevelFalseAnimation() {
            StartFade(0, 255, () => SetLevel(levelIndex), Constants.FadeOutDuration);
            playSound(SoundKind.Win1);
        }
        internal void StartReloadLevelAnimation() {
            StartFade(0, 255, () => SetLevel(levelIndex), Constants.FadeOutDuration);
            //playSound(SoundKind.BrakeBall);
        }
        internal void StartCthulhuReloadLevelAnimation() {
            game.scene.ClearElements();
            game.scene.AddElement(new SvgElement(cthulhuSvg) {
                Rect = Rect.FromCenter(
                    new Vector2(game.width / 2, game.height / 2),
                    new Vector2(game.width * Constants.CthulhuWidthScaleRatio)
                )
            });
            StartFade(0, 255, () => SetLevel(levelIndex), Constants.FadeOutCthulhuDuration);
            playSound(SoundKind.Cthulhu);
        }

        internal Letter[] CreateLetters(Action<Letter, int> setUp, string word = "TOUCH") {
            int index = 0;
            var letters = new Letter[word.Length];
            foreach(var value in word) {
                var letter = new Letter() {
                    Value = value,
                };
                setUp(letter, index);
                scene.AddElement(letter);
                letters[index] = letter;
                index++;
            }
            return letters;
        }

        internal int levelIndex { get; private set; } = 0;
        internal void VerifyExpectedLevelIndex(int expectedLevel) {
            if(levelIndex != expectedLevel)
                throw new InvalidOperationException();
        }
        internal Rect levelNumberElementRect => levelNumberLeterrs.First().Rect;
        internal void RemoveLastLevelLetter() {
            scene.RemoveElement(levelNumberLeterrs.Last());
            levelNumberLeterrs.RemoveAt(levelNumberLeterrs.Count - 1);
        }

        internal List<Letter> levelNumberLeterrs = new();

        Action? clearScene;
        void SetScene(Func<SceneContext> start) {
            clearScene?.Invoke();
            scene.ClearElements();
#if DEBUG
            animations.VerifyEmpty();//TODO call log function instead
#endif
            animations.ClearAll();
            clearScene = start().clear;
            StartFade(255, 0, () => { }, Constants.FadeOutDuration);
        }

        void SetSelectLevelAnimation() {
            SetScene(() => {
                var letters = Enumerable.Range(0, 2)
                    .Select(i => {
                        var letter = new Letter {
                            ActiveRatio = 1,
                            HitTestVisible = true,
                            Rect = GetLetterTargetRect(i + 1.5f, GetButtonRect())
                        }.AddTo(game);
                        letter.GetPressState = TapInputState.GetPressReleaseHandler(
                            letter,
                            () => {
                                playSound(SoundKind.Snap);
                                StartReloadLevelAnimation();
                            },
                            () => { }
                        );
                        return letter;
                    })
                    .ToArray();

                void ChangLevelIndex(int delta) {
                    var newIndex = levelIndex + delta;
                    SetLevelIndex(newIndex);
                    var levelString = levelIndex.ToString("00");
                    letters[0].Value = levelString[0];
                    letters[1].Value = levelString[1];
                    if(delta != 0)
                        playSound(newIndex == levelIndex ? SoundKind.Tap : SoundKind.ErrorClick);
                }
                ChangLevelIndex(0);

                var nextLevel = new Letter {
                    Value = '>',
                    HitTestVisible = true,
                    Rect = GetLetterTargetRect(4f, GetButtonRect())
                }.AddTo(game);
                nextLevel.GetPressState = TapInputState.GetPressReleaseHandler(
                    nextLevel,
                    () => ChangLevelIndex(+1),
                    () => { }
                );

                var prevLevel = new Letter {
                    Value = '<',
                    HitTestVisible = true,  
                    Rect = GetLetterTargetRect(0f, GetButtonRect())
                }.AddTo(game);
                prevLevel.GetPressState = TapInputState.GetPressReleaseHandler(
                    prevLevel,
                    () => ChangLevelIndex(-1),
                    () => { }
                );

                return default;
            });
        }

        public void SetLevel(int level) {
            SetScene(() => {
                SetLevelIndex(level);
                int digitIndex = 0;
                levelNumberLeterrs.Clear();

                float offsetX = letterSize * Constants.LetterIndexOffsetRatioX;
                float offsetY = letterSize * Constants.LetterIndexOffsetRatioY;

                var bulb = new SvgElement(icons[SvgIcon.Bulb]) {
                    HitTestVisible = true,
                    Rect = new Rect(
                        scene.width - offsetX - letterDragBoxWidth * Constants.LevelLetterRatio, 
                        offsetY, 
                        letterDragBoxWidth * Constants.LevelLetterRatio, 
                        letterDragBoxHeight * Constants.LevelLetterRatio
                    ),
                    Size = letterSize * Constants.LevelLetterRatio,
                    Style = LetterStyle.Inactive,
                }.AddTo(game);

                foreach(var digit in levelIndex.ToString()) {
                    var levelNumberElement = new Letter {
                        Value = digit,
                        HitTestVisible = true,
                    };
                    SetUpLevelIndexButton(
                        levelNumberElement,
                        new Vector2(
                            offsetX + digitIndex * letterDragBoxWidth * Constants.LevelLetterRatio,
                            offsetY
                        )
                    );
                    levelNumberElement.GetPressState = TapInputState.GetClickHandler(
                        levelNumberElement,
                        SetSelectLevelAnimation,
                        isPressed => levelNumberLeterrs.ForEach(x => x.ActiveRatio = isPressed ? 1 : 0)
                    );
                    scene.AddElement(levelNumberElement);
                    digitIndex++;
                    levelNumberLeterrs.Add(levelNumberElement);
                }
                var levelContext = Levels[levelIndex].action(this);

                bulb.GetPressState = TapInputState.GetClickHandler(
                    bulb,
                    () => {
                        var elements = new List<Element>();
                        var fadeElement = new InputHandlerElement {
                            HitTestVisible = true,
                            Rect = scene.Bounds
                        }.AddTo(game);
                        elements.Add(fadeElement);

                        new LerpAnimation<float> {
                            From = 0,
                            To = 0.97f,
                            Duration = Constants.HintFadeDuration,
                            Lerp = MathFEx.Lerp,
                            SetValue = value => fadeElement.Opacity = value,
                            End = () => {
#if DEBUG
                                if(levelContext.hint.symbols == null)
                                    throw new InvalidOperationException(); //use log instead
#endif
                                var symbols = levelContext.hint.symbols ?? new[] { new HintSymbol[] { SvgIcon.Elipsis } };
                                var buttonRect = GetButtonRect();
                                var button = new Button {
                                }.AddTo(game);

                                //var containingRect = buttonRect;
                                for(int row = 0; row < symbols.Length; row++) {
                                    for(int col = 0; col < symbols[row].Length; col++) {
                                        var hint = symbols[row][col];
                                        var rect = GetLetterTargetRect(col, buttonRect, row: -3 + row);
                                        const float scale = 0.65f;
                                        Element element = hint switch {
                                            (SvgIcon icon, null) =>
                                                new SvgElement(icons[icon]) {
                                                    Rect = rect,
                                                    Size = letterSize * scale,
                                                    Style = LetterStyle.Accent1,
                                                },

                                            (null, char letter) =>
                                                new Letter {
                                                    Value = letter,
                                                    Rect = rect,
                                                    Scale = new Vector2(scale),
                                                },
                                            _ => throw new InvalidOperationException()
                                        };
                                        element.AddTo(game);
                                        elements.Add(element);
                                    }
                                }
                            }
                        }.Start(game);

                        fadeElement.GetPressState = TapInputState.GetPressReleaseHandler(
                            fadeElement,
                            () => elements.ForEach(x => scene.RemoveElement(x)),
                            () => { }
                        );
                    },
                    isPressed => bulb.Style = isPressed ? LetterStyle.Accent1 : LetterStyle.Inactive
                );

                return new SceneContext(animations.ClearAll);
            });
        }

        private void SetLevelIndex(int level) {
            levelIndex = Math.Max(Math.Min(level, Levels.Length - 1), 0);
        }

        internal void SetUpLevelIndexButton(Letter letter, Vector2 location) {
            letter.Rect = new Rect(
                location,
                new Vector2(letterDragBoxWidth * Constants.LevelLetterRatio, letterDragBoxHeight * Constants.LevelLetterRatio)
            );
            letter.ActiveRatio = 0;
            letter.Scale = new Vector2(Constants.LevelLetterRatio);

        }

        internal float width => scene.width;
        internal float height => scene.height;
    }

    public record struct SceneContext(Action? clear);
    public record struct LevelContext(Hint hint) {
        public static implicit operator LevelContext(Hint hint)
            => new LevelContext(hint);
        public static implicit operator LevelContext(HintSymbol[] symbols)
            => new[] { symbols };
        public static implicit operator LevelContext(HintSymbol[][] symbols)
            => new Hint(symbols);
        //public static implicit operator LevelContext((Action clear, Hint hint) values)
        //    => new LevelContext(values.clear, values.hint);
    }
    public record struct Hint(HintSymbol[][]? symbols);
    public record struct HintSymbol(SvgIcon? icon, char? letter) {
        public static implicit operator HintSymbol(SvgIcon icon)
            => new HintSymbol(icon, null);
        public static implicit operator HintSymbol(char letter)
            => new HintSymbol(null, letter);
    }

    public static class ElementExtensions {
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
                    MathFEx.VectorsEqual(game.GetLetterTargetRect(i, game.GetButtonRect()).Location, letters[i].Rect.Location)
                        ? 1 : 0;
            }
        }

        //public static Action GetRemoveAnimationAction(this AnimationBase animation, GameController game) {
        //    return () => game.animations.RemoveAnimation(animation);
        //}
    }
    public enum Direction { Left, Right, Up, Down }

    public static class DirectionExtensions {
        public static SoundKind GetSound(this Direction direction) 
            => direction is Direction.Right or Direction.Down ? SoundKind.SwipeRight : SoundKind.SwipeLeft;

        public static Direction RotateClockwize(this Direction direction) {
            return direction switch {
                Direction.Left => Direction.Up,
                Direction.Right => Direction.Down,
                Direction.Up => Direction.Right,
                Direction.Down => Direction.Left,
                _ => throw new InvalidOperationException(),
            };
        }
        public static Direction RotateCounterClockwize(this Direction direction) {
            return direction switch {
                Direction.Left => Direction.Down,
                Direction.Right => Direction.Up,
                Direction.Up => Direction.Left,
                Direction.Down => Direction.Right,
                _ => throw new InvalidOperationException(),
            };
        }
        public static float ToAngle(this Direction direction) {
            return direction switch {
                Direction.Left => MathFEx.PI,
                Direction.Right => 0,
                Direction.Up => 3 * MathFEx.PI / 2,
                Direction.Down => MathFEx.PI / 2,
                _ => throw new InvalidOperationException(),
            };
        }

        public static Direction? GetSwipeDirection(ref Vector2 delta, float minLength) {
            Direction direction;
            if(Math.Abs(delta.X) > Math.Abs(delta.Y)) {
                delta.Y = 0;
                direction = delta.X > 0 ? Direction.Right : Direction.Left;
            } else {
                delta.X = 0;
                direction = delta.Y > 0 ? Direction.Down : Direction.Up;
            }
            if(delta.Length() < minLength)
                return null;
            return direction;
        }
    }
    public static class Constants {
        public static float ButtonRelativeWidth => 0.6f;
        public static float ButtonHeightRatio => 1f / 3f;

        public static float LetterHeightRatio => 3f / 4f;
        public static float LetterVerticalOffsetRatio => 0.16f;
        public static float LetterDragBoxHeightRatio => 0.9f;
        public static float LetterDragBoxWidthRatio => 0.57f;
        public static float LetterHorizontalStepRatio => 0.18f;

        public static float LevelLetterRatio => 0.75f;

        //public static Color FadeOutColor = new Color(0, 0, 0);
        public static TimeSpan FadeOutDuration => TimeSpan.FromMilliseconds(500);
        public static TimeSpan HintFadeDuration => TimeSpan.FromMilliseconds(150);
        public static TimeSpan FadeOutCthulhuDuration => TimeSpan.FromMilliseconds(3000);
        public static TimeSpan RotateAroundLetterDuration => TimeSpan.FromMilliseconds(500);
        public static TimeSpan InflateButtonDuration => TimeSpan.FromMilliseconds(1500);

        public static float LetterIndexOffsetRatioX => .2f;
        public static float LetterIndexOffsetRatioY => .2f;

        public static float LetterSnapDistanceRatio => .2f;
        public static float ButtonAnchorDistanceRatio => .2f;

        public static float MinButtonInvisibleInterval => 1000;
        public static float MaxButtonInvisibleInterval => 5000;
        public static float MinButtonAppearInterval => 200;
        public static float MaxButtonAppearInterval => 500;
        public static float ButtonAppearIntervalIncrease => 25;

        public static float ButtonOutOfBoundDragRatio => 0.7f;

        public static float FindWordLetterScale => .75f;
        public static float ContainingButtonInflateValue => 2;

        public static float CthulhuWidthScaleRatio = .7f;

        public static float ZeroDigitMaxDragDistance => 0.75f;

        public static float ScrollLettersLetterScale => .9f;

        public static float ButtonBorderWeight => 3f;
        public static float ButtonCornerRadius => 5f;

        public static float ReflectedCOffset => 6f;


    }
}

