using MetaArt.Core;
using NUnit.Framework;
using System.Numerics;
using System.Runtime.CompilerServices;
using ThatButtonAgain;

namespace MetaButton.Tests {
    [TestFixture]
    public class SolutionTests {
        [TestCaseSource(nameof(Levels))]
        public void LevelSolutionTest(int index, Action<GameController> action) { 
            TestExtensions.AssertLevelSolution(index, action);

        }
        static Dictionary<string, Action<GameController>> SolutionMethods = new [] {
            RegisterSolution(Solutions.Touch),
            RegisterSolution(Solutions.DragLetters_Normal),
            RegisterSolution(Solutions.Capital_16xClick),
            RegisterSolution(Solutions.RotateAroundLetter),
            RegisterSolution(Solutions.LettersBehindButton),
            RegisterSolution(Solutions.RandomButton_Simple),
            RegisterSolution(Solutions.ClickInsteadOfTouch),
        }.ToDictionary(x => x.name, x => x.action);

        static (Action<GameController> action, string name) RegisterSolution(Action<GameController> action, [CallerArgumentExpression("action")] string name = "") {
            return (action, name.Replace(nameof(Solutions) + '.', null));
        }
        static object[] Levels = GameController
            .Levels
            .Take(SolutionMethods.Count)
            .Select((x, index) => new object[] { index, SolutionMethods[x.name] })
            .ToArray();

        [Test, Explicit]
        public void Explicit() {
            TestExtensions.AssertLevelSolution(5, Solutions.ClickInsteadOfTouch);
        }
    }
    public static class Solutions {
        public static void Touch(GameController game) {
            var button = game.GetElement<Button>();
            game.TapButton(button);
        }
        public static void DragLetters_Normal(GameController game) {
            var button = game.GetElement<Button>();
            foreach(var (letter, index) in "TOUCH".Select((x, i) => (x, i))) {
                game.DragElement(game.GetLetter(letter), game.GetLetterTargetRect(index, button.Rect));
            }
            game.NextFrame(1);
            game.TapButton(button);
        }
        public static void Capital_16xClick(GameController game) {
            var button = game.GetElement<Button>();
            var letters = "Touch".Select(x => game.GetLetter(x)).ToArray();
            for(int i = 0; i < 15; i++) {
                game.TapElement(letters[i % 5]);
            }
            game.TapButton(button);
        }
        public static void RotateAroundLetter(GameController game) {
            var button = game.GetElement<Button>();
            foreach(var letter in Level_RotateAroundLetter.Solution) {
                Assert.False(button.HitTestVisible);
                game.TapElement(game.GetLetter(letter));
                game.SkipAnimations();
                Assert.False(button.HitTestVisible);
            }
            game.SkipAnimations();
            game.TapButton(button);
        }
        public static void LettersBehindButton(GameController game) {
            var button = game.GetElement<Button>();
            Assert.False(button.IsEnabled);
            game.DragElement(button, button.Rect.Offset(new Vector2(0, button.Rect.Height * 2)), snapRadius: 0);
            game.SkipAnimations();

            Assert.False(button.IsEnabled);
            game.DragElement(button, game.GetButtonRect());
            game.SkipAnimations();

            game.TapButton(button);
        }
        public static void RandomButton_Simple(GameController game) {
            Assert.Null(game.TryGetElement<Button>());
            game.NextFrame(Constants.FirstButtonInvisibleInterval - 10 - (float)Constants.FadeOutDuration.TotalMilliseconds);
            Assert.Null(game.TryGetElement<Button>());
            game.NextFrame(20);
            var button = game.GetElement<Button>();
            game.TapButton(button);
        }
        public static void ClickInsteadOfTouch(GameController game) {
            var letters = game.GetElements<Letter>(x => Level_ClickInsteadOfTouch.Click.Contains(x.Value));
            Assert.AreEqual(5, letters.Length);
            var button = game.GetElement<Button>();
            foreach(var index in Level_ClickInsteadOfTouch.Solution) {
                Assert.False(button.HitTestVisible);
                game.PressElement(letters[index]);
                game.SkipAnimations();
                game.ReleaseElement(letters[index]);
                Assert.False(button.HitTestVisible);
            }
            game.SkipAnimations();
            game.TapButton(button);
        }
    }
    public static class TestExtensions {
        public static void SkipAnimations(this GameController game) {
            game.NextFrame(10000);
        }

        public static void AssertLevelSolution(int levelIndex, Action<GameController> solution) {
            var controller = TestExtensions.CreateController(levelIndex);
            Assert.AreEqual(levelIndex, controller.LevelIndex);
            controller.WaitFadeIn();

            solution(controller);

            controller.WaitFadeOut();
            Assert.AreEqual(levelIndex + 1, controller.LevelIndex);
            controller.WaitFadeIn();
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

        public static T GetElement<T>(this GameController game, Predicate<T>? condition = null) where T: Element {
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
        }
        public static GameController CreateController(int levelIndex) {
            return new GameController(
                            width: 400,
                            height: 700,
                            createSound: stream => {
                                Assert.NotNull(stream);
                                return new TestSound();
                            },
                            createSvg: stream => {
                                Assert.NotNull(stream);
                                return new TestSvg();
                            },
                            storage: StorageExtensions.CreateInMemoryStorage(),
                            levelIndex: levelIndex
                        );
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

    }
}
