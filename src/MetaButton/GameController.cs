using MetaArt.Core;
using System.Numerics;
using System.Runtime.CompilerServices;

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

        readonly Engine engine;

        public Scene scene => engine.scene;
        internal AnimationsController animations => engine.animations;

        public readonly float letterVerticalOffset;
        internal readonly float buttonWidth;
        internal readonly float buttonHeight;
        public readonly float letterSize;
        internal readonly float letterDragBoxHeight;
        internal readonly float letterDragBoxWidth;
        internal readonly float letterHorzStep;
        readonly SvgDrawing cthulhuSvg;
        readonly Func<Stream, SvgDrawing> createSvg;
        private readonly Storage storage;
        readonly Dictionary<SvgIcon, SvgDrawing> icons;
        readonly Dictionary<SoundKind, Sound> sounds;

        public GameController(float width, float height, Func<Stream, Sound> createSound, Func<Stream, SvgDrawing> createSvg, Storage storage, int? levelIndex) {
            engine = new Engine(width, height);

            buttonWidth = scene.width * Constants.ButtonRelativeWidth;
            buttonHeight = buttonWidth * Constants.ButtonHeightRatio;
            letterSize = buttonHeight * Constants.LetterHeightRatio;
            letterDragBoxHeight = buttonHeight * Constants.LetterDragBoxHeightRatio;
            letterDragBoxWidth = buttonHeight * Constants.LetterDragBoxWidthRatio;
            letterVerticalOffset = letterSize * Constants.LetterVerticalOffsetRatio;
            letterHorzStep = buttonWidth * Constants.LetterHorizontalStepRatio;

            this.createSvg = createSvg;
            this.storage = storage;
            cthulhuSvg = CreateSvg("Cthulhu");

            icons = Enum.GetValues(typeof(SvgIcon))
                .Cast<SvgIcon>()
                .ToDictionary(x => x, x => CreateSvg(x.ToString()));

            sounds = Enum.GetValues(typeof(SoundKind))
                .Cast<SoundKind>()
                .ToDictionary(x => x, x => createSound(Utils.GetStream(typeof(GameController), "Sound." + x + ".wav")));

            SetLevel(levelIndex ?? LevelIndex);
        }

        internal void playSound(SoundKind kind) => sounds[kind].Play();

        SvgDrawing CreateSvg(string name) => createSvg(Utils.GetStream(typeof(GameController), "Svg." + name + ".svg"));

        public void NextFrame(float deltaTime) {
            engine.NextFrame(TimeSpan.FromMilliseconds(deltaTime));
        }

        internal void StartNextLevelAnimation(bool nextLevel = true) {
            engine.StartFade(() => SetLevel(LevelIndex + (nextLevel ? 1 : 0)), Constants.FadeOutDuration);
            playSound(SoundKind.Win1);
        }
        internal void StartReloadLevelAnimation() {
            engine.StartFade(() => SetLevel(LevelIndex), Constants.FadeOutDuration);
        }
        internal void StartCthulhuReloadLevelAnimation() {
            scene.ClearElements();
            scene.AddElement(new SvgElement(cthulhuSvg) {
                Rect = Rect.FromCenter(
                    new Vector2(scene.width / 2, scene.height / 2),
                    new Vector2(scene.width * Constants.CthulhuWidthScaleRatio)
                )
            });
            engine.StartFade(() => SetLevel(LevelIndex), Constants.FadeOutCthulhuDuration);
            playSound(SoundKind.Cthulhu);
        }

        internal int LevelIndex { 
            get => storage.GetInt(nameof(LevelIndex));
            private set => storage.SetInt(nameof(LevelIndex), value);
        }
        internal void RemoveLastLevelLetter() {
            scene.RemoveElement(levelNumberLeterrs.Last());
            levelNumberLeterrs.RemoveAt(levelNumberLeterrs.Count - 1);
        }

        internal List<Letter> levelNumberLeterrs { get; private set; } = new();

        void SetSelectLevelAnimation() {
            engine.SetScene(() => {
                var letters = Enumerable.Range(0, 2)
                    .Select(i => {
                        var letter = new Letter {
                            ActiveRatio = 1,
                            HitTestVisible = true,
                            Rect = this.GetLetterTargetRect(i + 1.5f, this.GetButtonRect())
                        }.AddTo(this);
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
                    var newIndex = LevelIndex + delta;
                    SetLevelIndex(newIndex);
                    var levelString = LevelIndex.ToString("00");
                    letters[0].Value = levelString[0];
                    letters[1].Value = levelString[1];
                    if(delta != 0)
                        playSound(newIndex == LevelIndex ? SoundKind.Tap : SoundKind.ErrorClick);
                }
                ChangLevelIndex(0);

                var nextLevel = new Letter {
                    Value = '>',
                    HitTestVisible = true,
                    Rect = this.GetLetterTargetRect(4f, this.GetButtonRect())
                }.AddTo(this);
                nextLevel.GetPressState = TapInputState.GetPressReleaseHandler(
                    nextLevel,
                    () => ChangLevelIndex(+1),
                    () => { }
                );

                var prevLevel = new Letter {
                    Value = '<',
                    HitTestVisible = true,  
                    Rect = this.GetLetterTargetRect(0f, this.GetButtonRect())
                }.AddTo(this);
                prevLevel.GetPressState = TapInputState.GetPressReleaseHandler(
                    prevLevel,
                    () => ChangLevelIndex(-1),
                    () => { }
                );

                return default;
            },
            Constants.FadeOutDuration);
        }

        void SetLevel(int level) {
            engine.SetScene(() => {
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
                }.AddTo(this);

                foreach(var digit in LevelIndex.ToString()) {
                    var levelNumberElement = new Letter {
                        Value = digit,
                        HitTestVisible = true,
                    };
                    this.SetUpLevelIndexButton(
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
                var levelContext = Levels[LevelIndex].action(this);

                bulb.GetPressState = TapInputState.GetClickHandler(
                    bulb,
                    () => {
                        var elements = new List<Element>();
                        var fadeElement = new InputHandlerElement {
                            HitTestVisible = true,
                            Rect = scene.Bounds
                        }.AddTo(this);
                        elements.Add(fadeElement);

                        new LerpAnimation<float> {
                            From = 0,
                            To = 0.97f,
                            Duration = Constants.HintFadeDuration,
                            Lerp = MathF.Lerp,
                            SetValue = value => fadeElement.Opacity = value,
                            End = () => {
#if DEBUG
                                if(levelContext.hint.symbols == null)
                                    throw new InvalidOperationException(); //use log instead
#endif
                                var symbols = levelContext.hint.symbols ?? new[] { new HintSymbol[] { SvgIcon.Elipsis } };
                                var buttonRect = this.GetButtonRect();
                                var button = new Button {
                                }.AddTo(this);

                                //var containingRect = buttonRect;
                                for(int row = 0; row < symbols.Length; row++) {
                                    for(int col = 0; col < symbols[row].Length; col++) {
                                        var hint = symbols[row][col];
                                        var rect = this.GetLetterTargetRect(col, buttonRect, row: -3 + row);
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
                                        element.AddTo(this);
                                        elements.Add(element);
                                    }
                                }
                            }
                        }.Start(this);

                        fadeElement.GetPressState = TapInputState.GetPressReleaseHandler(
                            fadeElement,
                            () => elements.ForEach(x => scene.RemoveElement(x)),
                            () => { }
                        );
                    },
                    isPressed => bulb.Style = isPressed ? LetterStyle.Accent1 : LetterStyle.Inactive
                );

                return new SceneContext(animations.ClearAll);
            },
            Constants.FadeOutDuration);
        }

        void SetLevelIndex(int level) {
            LevelIndex = Math.Max(Math.Min(level, Levels.Length - 1), 0);
        }
    }

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
                Direction.Left => MathF.PI,
                Direction.Right => 0,
                Direction.Up => 3 * MathF.PI / 2,
                Direction.Down => MathF.PI / 2,
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

