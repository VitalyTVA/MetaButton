using MetaArt.Core;
using NUnit.Framework;
using System.Numerics;
using System.Runtime.CompilerServices;
using ThatButtonAgain;

namespace MetaButton.Tests {
    [TestFixture]
    public class SolutionTests {
        static object[] Levels => Solutions.LevelSolutions.Select((x, i) => new object[] { i, x }).ToArray();

        [TestCaseSource(nameof(Levels))]
        public void LevelSolutionTest(int index, Action<GameController> action) { 
            TestExtensions.AssertLevelSolution(index, action);

        }

        [Test, Explicit]
        public void Explicit() {
            //TestExtensions.AssertLevelSolution(5, Solutions.ClickInsteadOfTouch);
        }
    }
    [TestFixture]
    public class HintTests {
        [Test]
        public void HintNotUsed() {
            var (game, startTime, getTotalTime) = TestExtensions.CreateControllerWithTimeInfo(null);
            game.SolveLevel(0, game => {
                ShowHintTimer(game, HintManager.MinHintInterval);
                Solutions.LevelSolutions[0](game);
            });
            game.SolveLevel(1, game => {
                ShowHintTimer(game, HintManager.MinHintInterval);
                Solutions.LevelSolutions[1](game);
            });
            game.SolveLevel(2, game => {
                ShowHintTimer(game, HintManager.MinHintInterval);
                Solutions.LevelSolutions[2](game);
            });
        }
        [Test]
        public void HintUseEveryLevel() {
            var (game, startTime, getTotalTime) = TestExtensions.CreateControllerWithTimeInfo(null);
            game.SolveLevel(0, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval);
                Solutions.LevelSolutions[0](game);
            });
            game.SolveLevel(1, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 2);
                Solutions.LevelSolutions[1](game);
            });
            game.SolveLevel(2, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 4);
                Solutions.LevelSolutions[2](game);
            });
            game.SolveLevel(3, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 8);
                Solutions.LevelSolutions[3](game);
            });
            game.SolveLevel(4, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 16);
                Solutions.LevelSolutions[4](game);
            }); 
            game.SolveLevel(5, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 32);
                Solutions.LevelSolutions[5](game);
            });
            game.SolveLevel(6, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 32);
                Solutions.LevelSolutions[6](game);
            });
        }
        [Test]
        public void HintDecreasePenalty() {
            var (game, startTime, getTotalTime) = TestExtensions.CreateControllerWithTimeInfo(null);
            game.SolveLevel(0, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval);
                Solutions.LevelSolutions[0](game);
            });
            game.SolveLevel(1, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 2);
                Solutions.LevelSolutions[1](game);
            });
            game.SolveLevel(2, game => {
                ShowHintTimer(game, HintManager.MinHintInterval * 4);
                Solutions.LevelSolutions[2](game);
            });
            game.SolveLevel(3, game => {
                ShowHintTimer(game, HintManager.MinHintInterval * 2);
                Solutions.LevelSolutions[3](game);
            });
        }
        [Test]
        public void RecreateGame_Level1() {
            var storage = StorageExtensions.CreateInMemoryStorage();
            var (game1, startTime1, getTotalTime1) = TestExtensions.CreateControllerWithTimeInfo(null, storage: storage);

            game1.SolveLevel(0, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval);
                Solutions.LevelSolutions[0](game);
            });
            game1.AssertLevelShown(1);
            ShowHintTimer(game1, HintManager.MinHintInterval * 2);
               
            var (game2, startTime2, getTotalTime2) = TestExtensions.CreateControllerWithTimeInfo(null, storage: storage, startTimeIn: startTime1 + getTotalTime1() + TimeSpan.FromSeconds(5));

            game2.SolveLevel(1, game => {
                ShowHintTimerAndWaitHint(game2, HintManager.MinHintInterval * 2 - 2 - 5);
                Solutions.LevelSolutions[1](game);
            });

            game2.AssertLevelShown(2);
            ShowHintTimer(game2, HintManager.MinHintInterval * 4);

            var (game3, startTime3, getTotalTime3) = TestExtensions.CreateControllerWithTimeInfo(null, storage: storage, startTimeIn: startTime2 + getTotalTime2());
            game3.AssertLevelShown(2);
            ShowHintTimer(game3, HintManager.MinHintInterval * 4 - 2);

            game3.ShowLevelSelector();
            game3.TapLoadLevelLetter();
            game3.AssertLevelShown(2);
            ShowHintTimer(game3, HintManager.MinHintInterval * 4 - 4);

            game3.ShowLevelSelector();
            game3.TapPrevLevelLetter();
            game3.TapLoadLevelLetter();
            game3.AssertLevelShown(1);
            Assert.NotNull(game3.GetHintBulb(on: true));

            var (game4, startTime4, getTotalTime4) = TestExtensions.CreateControllerWithTimeInfo(null, storage: storage, startTimeIn: startTime3 + getTotalTime3());
            game4.AssertLevelShown(1);
            Assert.NotNull(game4.GetHintBulb(on: true));

            game4.ShowLevelSelector();
            game4.TapNextLevelLetter();
            game4.TapLoadLevelLetter();
            game4.AssertLevelShown(2);
            ShowHintTimer(game4, HintManager.MinHintInterval * 4 - 9);
        }

        [Test]
        public void RecreateGame_Level0() {
            var storage = StorageExtensions.CreateInMemoryStorage();
            var (game1, startTime1, getTotalTime1) = TestExtensions.CreateControllerWithTimeInfo(null, storage: storage);
            game1.AssertLevelShown(0);
            ShowHintTimer(game1, HintManager.MinHintInterval);

            var (game2, startTime2, getTotalTime2) = TestExtensions.CreateControllerWithTimeInfo(null, storage: storage, startTimeIn: startTime1);
            game2.AssertLevelShown(0);
            ShowHintTimer(game2, HintManager.MinHintInterval);
        }

        [Test]
        public void UpdateHintBulb() {
            var (game, startTime, getTotalTime) = TestExtensions.CreateControllerWithTimeInfo(null);
            game.SolveLevel(0, game => {
                ShowHintTimer(game, HintManager.MinHintInterval);
                game.NextFrame(HintManager.MinHintInterval * 1000);
                Assert.NotNull(game.GetHintBulb(on: true));
                Solutions.LevelSolutions[0](game);
            });
        }

        [Test]
        public void NoPenaltyAfterUseHintOnSolvedLevel() {
            var (game, startTime, getTotalTime) = TestExtensions.CreateControllerWithTimeInfo(null);
            game.SolveLevel(0, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval);
                Solutions.LevelSolutions[0](game);
            });

            game.AssertLevelShown(1);
            ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 2);

            game.ShowLevelSelector();
            game.TapPrevLevelLetter();
            game.TapLoadLevelLetter();
            game.AssertLevelShown(0);
            ShowHint(game);

            game.ShowLevelSelector();
            game.TapNextLevelLetter();
            game.TapLoadLevelLetter();
            game.AssertLevelShown(1);

            Solutions.LevelSolutions[1](game);
            game.AssertLevelSwitched(1);

            game.SolveLevel(2, game => {
                ShowHintTimerAndWaitHint(game, HintManager.MinHintInterval * 4);
                Solutions.LevelSolutions[2](game);
            });
        }


        void ShowHintTimer(GameController game, int expectedTime) {
            var hint = game.GetHintBulb(on: false);
            game.TapElement(hint);
            game.NextFrame((float)Constants.HintFadeDuration.TotalMilliseconds);
            game.AssertHintTime(TimeSpan.FromSeconds(expectedTime - 1));
            game.NextFrame(1000);
            game.AssertHintTime(TimeSpan.FromSeconds(expectedTime - 2));
            game.TapElement(game.GetElement<InputHandlerElement>());
        }
        void ShowHintTimerAndWaitHint(GameController game, int expectedTime) {
            var hint = game.GetHintBulb(on: false);
            game.TapElement(hint);
            game.NextFrame((float)Constants.HintFadeDuration.TotalMilliseconds);
            game.AssertHintTime(TimeSpan.FromSeconds(expectedTime - 1));
            game.NextFrame(expectedTime * 1000);
            game.AssertHintShown();
            game.TapElement(game.GetElement<InputHandlerElement>());
        }
        void ShowHint(GameController game) {
            var hint = game.GetHintBulb(on: true);
            game.TapElement(hint);
            game.NextFrame((float)Constants.HintFadeDuration.TotalMilliseconds);
            game.AssertHintShown();
            game.TapElement(game.GetElement<InputHandlerElement>());
        }

    }
    public static class Solutions {
        static void Touch(GameController game) {
            var button = game.GetElement<Button>();
            game.TapButton(button);
        }
        static void DragLetters_Normal(GameController game) {
            var button = game.GetElement<Button>();
            foreach(var (letter, index) in "TOUCH".Select((x, i) => (x, i))) {
                game.DragElement(game.GetLetter(letter), game.GetLetterTargetRect(index, button.Rect));
            }
            game.NextFrame(1);
            game.TapButton(button);
        }
        static void Capital_16xClick(GameController game) {
            var button = game.GetElement<Button>();
            var letters = "Touch".Select(x => game.GetLetter(x)).ToArray();
            for(int i = 0; i < 15; i++) {
                game.TapElement(letters[i % 5]);
            }
            game.TapButton(button);
        }
        static void RotateAroundLetter(GameController game) {
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
        static void LettersBehindButton(GameController game) {
            var button = game.GetElement<Button>();
            Assert.False(button.IsEnabled);
            game.DragElement(button, button.Rect.Offset(new Vector2(0, button.Rect.Height * 2)), snapRadius: 0);
            game.SkipAnimations();

            Assert.False(button.IsEnabled);
            game.DragElement(button, game.GetButtonRect());
            game.SkipAnimations();

            game.TapButton(button);
        }
        static void RandomButton_Simple(GameController game) {
            for(int i = 0; i < 500; i++) {
                game.NextFrame(10);
                var button = game.TryGetElement<Button>(x => x.Tag == Level_RandomButton.Tag_RandomButton);
                if(button != null) {
                    game.TapElement(button);
                    return;
                }
            }
            Assert.Fail();
            //Assert.Null(game.TryGetElement<Button>());
            //game.NextFrame(Constants.FirstButtonInvisibleInterval - 10 - (float)Constants.FadeOutDuration.TotalMilliseconds);
            //Assert.Null(game.TryGetElement<Button>());
            //game.NextFrame(20);
            //var button = game.GetElement<Button>();
            //game.TapButton(button);
        }
        static void ClickInsteadOfTouch(GameController game) {
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

        static Dictionary<string, Action<GameController>> SolutionMethods = new[] {
            RegisterSolution(Touch),
            RegisterSolution(DragLetters_Normal),
            RegisterSolution(Capital_16xClick),
            RegisterSolution(RotateAroundLetter),
            RegisterSolution(LettersBehindButton),
            RegisterSolution(RandomButton_Simple),
            RegisterSolution(ClickInsteadOfTouch),
        }.ToDictionary(x => x.name, x => x.action);

        static (Action<GameController> action, string name) RegisterSolution(Action<GameController> action, [CallerArgumentExpression("action")] string name = "") {
            return (action, name.Replace(nameof(Solutions) + '.', null));
        }
        public static Action<GameController>[] LevelSolutions = GameController
            .Levels
            .Take(SolutionMethods.Count)
            .Select(x=> SolutionMethods[x.name])
            .ToArray();
    }
}
