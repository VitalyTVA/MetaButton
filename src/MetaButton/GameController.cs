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
        Speaker,
        SpeakerOff,
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
        readonly HintManager hintManager;

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
            this.hintManager = new HintManager(storage, () => DateTime.Now, () => LevelIndex);

            icons = Enum.GetValues(typeof(SvgIcon))
                .Cast<SvgIcon>()
                .ToDictionary(x => x, x => CreateSvg(x.ToString()));

            sounds = Enum.GetValues(typeof(SoundKind))
                .Cast<SoundKind>()
                .ToDictionary(x => x, x => createSound(Utils.GetStream(typeof(GameController), "Sound." + x + ".wav")));

            if(levelIndex == null && MaxLevel == 0) //new game
                hintManager.ResetLastHintTime();
            SetLevel(levelIndex ?? LevelIndex);
        }

        internal void playSound(SoundKind kind) {
            if(!DisableSound)
                sounds[kind].Play();
        }

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
            scene.AddElement(new SvgElement { 
                Svg = cthulhuSvg, 
                Rect = Rect.FromCenter(
                    new Vector2(scene.width / 2, scene.height / 2),
                    new Vector2(scene.width * Constants.CthulhuWidthScaleRatio)
                )
            });
            engine.StartFade(() => SetLevel(LevelIndex), Constants.FadeOutCthulhuDuration);
            playSound(SoundKind.Cthulhu);
        }

        public int LevelIndex { 
            get => storage.GetInt(nameof(LevelIndex));
            private set => storage.SetInt(nameof(LevelIndex), value);
        }
        int MaxLevel {
            get => storage.GetInt(nameof(MaxLevel));
            set => storage.SetInt(nameof(MaxLevel), value);
        }
        bool DisableSound {
            get => storage.GetBool(nameof(DisableSound));
            set => storage.SetBool(nameof(DisableSound), value);
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
                            Rect = this.GetLetterTargetRect(i + 1.5f, this.GetButtonRect(), row: -1)
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

                var nextLevel = new Letter {
                    Value = '>',
                    HitTestVisible = true,
                    Rect = this.GetLetterTargetRect(4f, this.GetButtonRect(), row: -1)
                }.AddTo(this);
                var prevLevel = new Letter {
                    Value = '<',
                    HitTestVisible = true,
                    Rect = this.GetLetterTargetRect(0f, this.GetButtonRect(), row: -1)
                }.AddTo(this);

                void ChangLevelIndex(int delta) {
                    var newIndex = LevelIndex + delta;
                    SetLevelIndex(newIndex, MaxLevel);
                    prevLevel.ActiveRatio = LevelIndex > 0 ? 1 : 0;
                    nextLevel.ActiveRatio = LevelIndex < MaxLevel ? 1 : 0;
                    var levelString = LevelIndex.ToString("00");
                    letters[0].Value = levelString[0];
                    letters[1].Value = levelString[1];
                    if(delta != 0)
                        playSound(newIndex == LevelIndex ? SoundKind.Tap : SoundKind.ErrorClick);
                }
                ChangLevelIndex(0);

                nextLevel.GetPressState = TapInputState.GetPressReleaseHandler(
                    nextLevel,
                    () => ChangLevelIndex(+1),
                    () => { }
                );

                prevLevel.GetPressState = TapInputState.GetPressReleaseHandler(
                    prevLevel,
                    () => ChangLevelIndex(-1),
                    () => { }
                );

                var disableSoundIcon = new SvgElement {
                    HitTestVisible = true,
                    Rect = this.GetLetterTargetRect(2f, this.GetButtonRect(), row: 1),
                    Style = LetterStyle.Accent1,
                    Size = letterSize * Constants.LevelLetterRatio,
                }.AddTo(this);
                void UpdateSoundIcon() {
                    disableSoundIcon.Svg = GetIcon(DisableSound ? SvgIcon.SpeakerOff : SvgIcon.Speaker);
                }
                UpdateSoundIcon();
                disableSoundIcon.GetPressState = TapInputState.GetPressReleaseHandler(
                    disableSoundIcon,
                    () => {
                        DisableSound = !DisableSound;
                        UpdateSoundIcon();
                        playSound(SoundKind.Snap);
                    },
                    () => { }
                );

                return default;
            },
            Constants.FadeOutDuration);
        }

        void SetLevel(int level) {
            engine.SetScene(() => {
                SetLevelIndexAndMaxLevel(level);
                levelNumberLeterrs.Clear();

                var (bulb, letters) = this.CreateLevelElements(hintManager.IsHintAvailable);

                foreach(var levelNumberElement in letters) {
                    levelNumberElement.GetPressState = TapInputState.GetClickHandler(
                        levelNumberElement,
                        SetSelectLevelAnimation,
                        isPressed => levelNumberLeterrs.ForEach(x => x.ActiveRatio = isPressed ? 1 : 0)
                    );
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
                        Action? removeTimer = null;
                        new LerpAnimation<float> {
                            From = 0,
                            To = 0.97f,
                            Duration = Constants.HintFadeDuration,
                            Lerp = MathF.Lerp,
                            SetValue = value => fadeElement.Opacity = value,
                            End = () => {
                                void ShowHint() {
                                    hintManager.UseHint();
                                    var hintElements = this.CreateHintElements(levelContext.hint.symbols);
                                    elements.AddRange(hintElements);
                                }
                                if(hintManager.IsHintAvailable()) {
                                    ShowHint();
                                } else {
                                    Letter[] letters = null!;
                                    (letters, removeTimer) = this.CreateTimerLetters(() => {
                                        ShowHint();
                                        foreach(var item in letters) {
                                            elements.Remove(item);
                                        }
                                    }, hintManager.GetWaitTime);
                                    elements.AddRange(letters);
                                }
                            }
                        }.Start(this);

                        fadeElement.GetPressState = TapInputState.GetPressReleaseHandler(
                            fadeElement,
                            () => {
                                removeTimer?.Invoke();
                                elements.ForEach(x => scene.RemoveElement(x));
                            },
                            () => { }
                        );
                    },
                    isPressed => bulb.Style = isPressed ? LetterStyle.Accent1 : LetterStyle.Inactive
                );

                return new SceneContext(animations.ClearAll);
            },
            Constants.FadeOutDuration);
        }

        void SetLevelIndexAndMaxLevel(int level) {
            SetLevelIndex(level, Levels.Length - 1);
            hintManager.LevelChanged(newLevelSolved: LevelIndex > MaxLevel);
            MaxLevel = Math.Max(LevelIndex, MaxLevel);
        }
        void SetLevelIndex(int level, int maxIndex) {
            LevelIndex = Math.Max(Math.Min(level, maxIndex), 0);
        }

        internal SvgDrawing GetIcon(SvgIcon icon) => icons[icon];
    }
    class HintManager {
        readonly Storage storage;
        readonly Func<DateTime> getNow;
        readonly Func<int> getLevel;

        public HintManager(Storage storage, Func<DateTime> getNow, Func<int> getLevel) {
            this.storage = storage;
            this.getNow = getNow;
            this.getLevel = getLevel;
        }

        const int MinHintInterval = 30;
        const int MaxPenalty = 6;

        DateTime now => getNow();
        int level => getLevel();
        
        int HintUsedLevel {
            get => storage.GetInt(nameof(HintUsedLevel));
            set => storage.SetInt(nameof(HintUsedLevel), value);
        }
        int CurentPenalty {
            get => storage.GetInt(nameof(CurentPenalty));
            set => storage.SetInt(nameof(CurentPenalty), value);
        }
        DateTime NextHintTime {
            get => storage.GetDateTime(nameof(NextHintTime));
            set => storage.SetDateTime(nameof(NextHintTime), value);
        }

        public void UseHint() {
            if(level > HintUsedLevel)
                CurentPenalty = Math.Min(CurentPenalty + 1, MaxPenalty);
            HintUsedLevel = Math.Max(level, HintUsedLevel);
        }
        public void ResetLastHintTime() {
            var result = MinHintInterval;
            for(int i = 0; i < CurentPenalty; i++) {
                result *= 2;
            }
            NextHintTime = now + TimeSpan.FromSeconds(result);
        }
        public void LevelChanged(bool newLevelSolved) {
            if(level > HintUsedLevel + 1) {
                CurentPenalty = Math.Max(CurentPenalty - 1, 0);
            }
            ResetLastHintTime();
        }
        public bool IsHintAvailable() => HintUsedLevel >= level || NextHintTime <= now;
        public TimeSpan GetWaitTime() => NextHintTime - now;
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

        public static float SvgIconScale = 0.65f;
    }
}

