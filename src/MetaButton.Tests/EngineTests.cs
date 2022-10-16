using MetaArt.Core;
using NUnit.Framework;
using System.Numerics;

namespace MetaButton.Tests {
    [TestFixture]
    public class EngineTests {
        [Test]
        public void Multitouch_Tap() {
            var engine = new Engine(100, 100);
            var element = new InputHandlerElement {
                Rect = new Rect(0, 0, 50, 50),

            };
            bool pressed = false;
            element.GetPressState = TapInputState.GetClickHandler(element, () => Assert.Fail(), x => pressed = x);
            engine.scene.AddElement(element);
            engine.scene.Press(new Vector2(25, 25));
            Assert.True(pressed);
            engine.scene.Press(new Vector2(75, 75));
            Assert.False(pressed);
            engine.scene.Release(new Vector2(25, 25));
        }
        [Test]
        public void Multitouch_Hover() {
            var engine = new Engine(100, 100);
            var element = new InputHandlerElement {
                Rect = new Rect(0, 0, 50, 50),

            };
            bool released = false;
            element.GetPressState = HoverInputState.GetHoverHandler(engine.scene, element, _ => { }, () => released = true);
            engine.scene.AddElement(element);
            engine.scene.Press(new Vector2(25, 25));
            engine.scene.Press(new Vector2(75, 75));
            Assert.True(released);
            engine.scene.Release(new Vector2(25, 25));
        }
    }
}
