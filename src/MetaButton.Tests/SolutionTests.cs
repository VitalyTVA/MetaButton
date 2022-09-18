using MetaArt.Core;
using NUnit.Framework;
using System.Numerics;
using ThatButtonAgain;

namespace MetaButton.Tests {
    [TestFixture]
    public class SolutionTests {
        class TestSound : Sound {
            public override void Play() {
            }
        }
        class TestSvg : SvgDrawing { 
        }
        [Test]
        public void Level0Test() {
            var controller = new GameController(
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
                levelIndex: 0
            );
            Assert.AreEqual(0, controller.LevelIndex);
            var button = controller.scene.VisibleElements.OfType<Button>().Single();

            Assert.IsInstanceOf<FadeOutElement>(controller.scene.HitTest(button.Rect.Mid));
            controller.NextFrame((float)Constants.FadeOutDuration.TotalMilliseconds - 1);
            Assert.IsInstanceOf<FadeOutElement>(controller.scene.HitTest(button.Rect.Mid));
            controller.NextFrame(2);
            Assert.IsNotInstanceOf<FadeOutElement>(controller.scene.HitTest(button.Rect.Mid));


            Assert.False(button.IsPressed);
            controller.scene.Press(button.Rect.Mid);
            Assert.True(button.IsPressed);
            controller.scene.Release(button.Rect.Mid);

            Assert.IsInstanceOf<FadeOutElement>(controller.scene.HitTest(button.Rect.Mid));
            controller.NextFrame((float)Constants.FadeOutDuration.TotalMilliseconds - 1);
            Assert.IsInstanceOf<FadeOutElement>(controller.scene.HitTest(button.Rect.Mid));
            controller.NextFrame(2);
            Assert.AreEqual(1, controller.LevelIndex);

            Assert.IsInstanceOf<FadeOutElement>(controller.scene.HitTest(button.Rect.Mid));
            controller.NextFrame((float)Constants.FadeOutDuration.TotalMilliseconds - 1);
            Assert.IsInstanceOf<FadeOutElement>(controller.scene.HitTest(button.Rect.Mid));
            controller.NextFrame(2);
            Assert.IsNotInstanceOf<FadeOutElement>(controller.scene.HitTest(button.Rect.Mid));
        }
    }
}
