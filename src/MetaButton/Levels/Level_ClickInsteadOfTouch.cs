﻿using MetaArt.Core;
namespace ThatButtonAgain {
    public static class Level_ClickInsteadOfTouch {
        public const string Click = "CLICK";
        public static int[] Solution = new[] { 0, 4, 2, 1 };

        public static LevelContext Load(GameController game) {
            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);
            button.HitTestVisible = false;

            int replaceIndex = 0;

            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
                letter.HitTestVisible = true;
                AnimationBase animation = null!;
                var onPress = () => {
                    if(replaceIndex == Solution.Length || Solution[replaceIndex] != index)
                        return;
                    animation = new LerpAnimation<float>() {
                        From = 1,
                        To = 2,
                        Duration = Constants.InflateButtonDuration,
                        Lerp = MathF.Lerp,
                        End = () => {
                            letter.Value = "TOUCH"[index];
                            letter.Scale = Letter.NoScale;
                            letter.HitTestVisible = false;
                            replaceIndex++;
                            game.playSound(SoundKind.SuccessSwitch);
                        },
                        SetValue = value => letter.Scale = new Vector2(value, value)
                    }.Start(game);
                };
                var onRelease = () => {
                    game.animations.RemoveAnimation(animation);
                    letter.Scale = Letter.NoScale;
                };
                letter.GetPressState = TapInputState.GetPressReleaseHandler(letter, onPress, onRelease);

            }, Click);
            new WaitConditionAnimation(
                condition: deltaTime => replaceIndex == 4) {
                End = () => {
                    button.HitTestVisible = true;
                    foreach(var item in letters) {
                        item.HitTestVisible = false;
                    }
                }
            }.Start(game);

            return new[] {
                new HintSymbol[] { Click[Solution[0]], SvgIcon.Tap, SvgIcon.Timer },
                new HintSymbol[] { Click[Solution[1]], SvgIcon.Tap, SvgIcon.Timer },
                new HintSymbol[] { Click[Solution[2]], SvgIcon.Tap, SvgIcon.Timer },
                new HintSymbol[] { Click[Solution[3]], SvgIcon.Tap, SvgIcon.Timer },
                GameControllerExtensions.TapButtonHint,
            };
        }
    }
}

