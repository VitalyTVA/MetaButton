using MetaArt.Core;
using NUnit.Framework;
using System.Numerics;
using ThatButtonAgain;

namespace MetaButton.Tests {
    public static class TestExtensions {
        public static void SkipAnimations(this GameController game) {
            game.NextFrame(10000);
        }

        public static void AssertLevelSolution(int levelIndex, Action<GameController> solution) {
            var game = CreateController(levelIndex);
            game.SolveLevel(levelIndex, solution);
            game.WaitFadeIn();
        }

        public static void SolveLevel(this GameController game, int levelIndex, Action<GameController> solution) {
            AssertLevelShown(game, levelIndex);
            solution(game);
            AssertLevelSwitched(game, levelIndex);
        }

        public static void AssertLevelSwitched(this GameController game, int levelIndex) {
            game.WaitFadeOut();
            Assert.AreEqual(levelIndex + 1, game.LevelIndex);
        }

        public static void AssertLevelShown(this GameController game, int levelIndex) {
            Assert.AreEqual(levelIndex, game.LevelIndex);
            game.WaitFadeIn();
        }

        public static void DragElement(this GameController game, Element element, Rect to, float snapRadius = 5) {
            var from = element.Rect.Mid;
            game.scene.Press(from);
            var distance = (to.Mid - from).Length();
            for(int i = 0; i <= distance - snapRadius; i++) {
                game.scene.Drag(Vector2.Lerp(from, to.Mid, i / distance));

            }
            game.scene.Release(from);
            Assert.AreEqual(to, element.Rect);
        }

        //public static void Press(this GameController game, Vector2 point) {
        //    game.scene.Press(point);
        //}
        //public static void Drag(this GameController game, Vector2 point) {
        //    game.scene.Drag(point);
        //    game.NextFrame(1);
        //}
        //public static void Release(this GameController game, Vector2 point) {
        //    game.scene.Release(point);
        //    game.NextFrame(1);
        //}

        public static void TapElement(this GameController game, Element element) {
            game.PressElement(element);
            game.ReleaseElement(element);
        }
        public static void PressElement(this GameController game, Element element) {
            game.scene.Press(element.Rect.Mid);
        }
        public static void ReleaseElement(this GameController game, Element element) {
            game.scene.Release(element.Rect.Mid);
        }

        public static void TapButton(this GameController game, Button button) {
            Assert.True(button.IsEnabled);
            Assert.False(button.IsPressed);
            game.scene.Press(button.Rect.Mid);
            Assert.True(button.IsPressed);
            game.scene.Release(button.Rect.Mid);
        }

        public static Letter GetLetter(this GameController game, char letter) {
            return game.GetElement<Letter>(x => x.Value == letter);
        }

        public static T GetElement<T>(this GameController game, Predicate<T>? condition = null) where T : Element {
            return game.TryGetElement(condition)!;
        }
        public static T? TryGetElement<T>(this GameController game, Predicate<T>? condition = null) where T : Element {
            return game.scene.VisibleElements.OfType<T>().SingleOrDefault(x => condition?.Invoke(x) ?? true);
        }
        public static T[] GetElements<T>(this GameController game, Predicate<T>? condition = null) where T : Element {
            return game.scene.VisibleElements.OfType<T>().Where(x => condition?.Invoke(x) ?? true).ToArray();
        }

        class TestSound : Sound {
            public override void Play() {
            }
        }
        class TestSvg : SvgDrawing {
            public readonly string Name;

            public TestSvg(string name) {
                this.Name = name;
            }
        }
        public static string GetSvgName(this SvgElement element) 
            => ((TestSvg)element.Svg).Name;

        public static SvgElement GetHintBulb(this GameController game, bool on) 
            => game.GetElement<SvgElement>(x => x.GetSvgName() == SvgIcon.Bulb + (on ? null : "Off"));
        public static GameController CreateController(int? levelIndex) {
            return CreateControllerWithTimeInfo(levelIndex).game;
        }
        public static (GameController game, DateTime startTime, Func<TimeSpan> getTotalTime) CreateControllerWithTimeInfo(
            int? levelIndex,
            Storage? storage = null,
            DateTime? startTimeIn = null
        ) {
            var startTime = startTimeIn ?? DateTime.Today;
            var totalTime = TimeSpan.Zero;
            var game = new GameController(
                width: 400,
                height: 700,
                createSound: stream => {
                    Assert.NotNull(stream);
                    return new TestSound();
                },
                createSvg: (name, stream) => {
                    Assert.NotNull(stream);
                    return new TestSvg(name);
                },
                onNextFrame: deltaTime => totalTime += TimeSpan.FromMilliseconds(deltaTime),
                getNow: () => startTime + totalTime,
                storage: storage ?? StorageExtensions.CreateInMemoryStorage(),
                levelIndex: levelIndex
            );
            return (game, startTime, () => totalTime);
        }
        public static void WaitFadeIn(this GameController game) {
            var fadeInElement = (FadeOutElement)game.scene.HitTest(game.scene.Bounds.Mid)!;
            Assert.AreEqual(255, fadeInElement.Opacity);
            game.NextFrame((float)Constants.FadeOutDuration.TotalMilliseconds - 1);
            Assert.AreSame(fadeInElement, game.scene.HitTest(game.scene.Bounds.Mid));
            game.NextFrame(2);
            Assert.AreEqual(0, fadeInElement.Opacity);
            Assert.AreNotSame(fadeInElement, game.scene.HitTest(game.scene.Bounds.Mid));
            Assert.False(game.scene.ContainsElement(fadeInElement));
        }
        public static void WaitFadeOut(this GameController game) {
            var fadeInElement = (FadeOutElement)game.scene.HitTest(game.scene.Bounds.Mid)!;
            Assert.AreEqual(0, fadeInElement.Opacity);
            game.NextFrame((float)Constants.FadeOutDuration.TotalMilliseconds - 1);
            Assert.AreSame(fadeInElement, game.scene.HitTest(game.scene.Bounds.Mid));
            game.NextFrame(2);
            Assert.AreEqual(255, fadeInElement.Opacity);
            Assert.AreNotSame(fadeInElement, game.scene.HitTest(game.scene.Bounds.Mid));
            Assert.False(game.scene.ContainsElement(fadeInElement));
        }

        public static void AssertHintTime(this GameController game, TimeSpan expectedTime) {
            var letters = game.GetElements<Letter>(x => x.Tag == GameControllerExtensions.Tag_TimerLetter).ToArray();
            Assert.AreEqual(expectedTime.Minutes / 10, letters[0].Value - '0');
            Assert.AreEqual(expectedTime.Minutes % 10, letters[1].Value - '0');
            Assert.AreEqual(':', letters[2].Value);
            Assert.AreEqual(expectedTime.Seconds / 10, letters[3].Value - '0');
            Assert.AreEqual(expectedTime.Seconds % 10, letters[4].Value - '0');
        }
        public static void AssertHintShown(this GameController game) {
            var letters = game.GetElements<Element>(x => x.Tag == GameControllerExtensions.Tag_HintSymbol).ToArray();
            Assert.LessOrEqual(2, letters.Length);
        }

        public static void ShowLevelSelector(this GameController game) {
            var letters = game.GetElements<Letter>(x => x.Tag == GameControllerExtensions.Tag_LevelNumberLetter).ToArray();
            game.TapElement(letters[0]);
            game.WaitFadeIn();
        }

        public static void TapLoadLevelLetter(this GameController game) {
            var letters = game.GetElements<Letter>(x => x.Tag == GameController.Tag_SelectLevel).ToArray();
            game.TapElement(letters[0]);
            game.WaitFadeOut();
        }
        public static void TapPrevLevelLetter(this GameController game) {
            var letter = game.GetElement<Letter>(x => x.Value == '<');
            game.TapElement(letter);
        }
        public static void TapNextLevelLetter(this GameController game) {
            var letter = game.GetElement<Letter>(x => x.Value == '>');
            game.TapElement(letter);
        }
    }
}
