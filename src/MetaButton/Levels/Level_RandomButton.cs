using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_RandomButton {
        public static LevelContext Load_Hard(GameController game) {
            return LoadCore(
                game,
                allowCthulhu: true
            );
        }
        public static LevelContext Load_Simple(GameController game) {
            return LoadCore(
                game,
                allowCthulhu: false
            );
        }
        static LevelContext LoadCore(GameController game, bool allowCthulhu) {
            AnimationBase appearAnimation = null!;
            AnimationBase disappearAnimation = null!;
            void RemoveAnimations(GameController game) {
                game.animations.RemoveAnimation(appearAnimation);
                game.animations.RemoveAnimation(disappearAnimation);
            }
            var button = game.CreateButton(() => {
                RemoveAnimations(game);
                game.StartNextLevelAnimation();
            }).AddTo(game);
            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
            });

            var cthulhuButton = game.CreateButton(() => {
                RemoveAnimations(game);
                game.StartCthulhuReloadLevelAnimation();
            }).AddTo(game);
            cthulhuButton.Rect = Rect.FromCenter(
                button.Rect.Mid, 
                new Vector2(button.Rect.Width + game.letterDragBoxWidth * 2, button.Rect.Height)
            );
            var cthulhuLetters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index - 1, button.Rect);
            }, "CTHULHU");

            void SetVisibility(bool visible, bool cthulhuVisible) {
                button!.IsVisible = visible;
                foreach(var letter in letters!) {
                    letter.IsVisible = visible;
                }
                cthulhuButton!.IsVisible = cthulhuVisible;
                foreach(var letter in cthulhuLetters!) {
                    letter.IsVisible = cthulhuVisible;
                }
            }

            SetVisibility(false, false);

            var appearInterval = Constants.MinButtonAppearInterval;

            bool firstAppear = true;
            float GetWaitTime() {
                if(firstAppear) {
                    firstAppear = false;
                    return Constants.MinButtonInvisibleInterval * 2;
                }
                return MathF.Random(Constants.MinButtonInvisibleInterval, Constants.MaxButtonInvisibleInterval);
            }

            var initialLocation = button.Rect.Location;
            var cthulhuInitialLocation = cthulhuButton.Rect.Location;

            int appearCount = 0;
            Random random = new Random(0);
            float xSpread = game.buttonWidth / 3;
            float ySpread = game.buttonHeight * 2.5f;
            Vector2 GetNextButtonOffset() => allowCthulhu 
                ? new Vector2(MathF.Random(-xSpread, xSpread), MathF.Random(-ySpread, ySpread)) 
                : default;

            void StartWaitButton() {
                appearAnimation = WaitConditionAnimation.WaitTime(
                    TimeSpan.FromMilliseconds(GetWaitTime()),
                    () => {
                        button.Rect = button.Rect.SetLocation(initialLocation + GetNextButtonOffset());
                        cthulhuButton.Rect = cthulhuButton.Rect.SetLocation(cthulhuInitialLocation + GetNextButtonOffset().SetX(0));
                        for(int i = 0; i < letters.Length; i++) {
                            letters[i].Rect = game.GetLetterTargetRect(i, button.Rect);
                        }
                        for(int i = 0; i < cthulhuLetters.Length; i++) {
                            cthulhuLetters[i].Rect = game.GetLetterTargetRect(i - 1, cthulhuButton.Rect);
                        }

                        appearCount++;
                        var (normal, cthulhu) = allowCthulhu && (appearCount == 4 || (appearCount > 6 && random.Next(4) == 0)) 
                            ? (false, true) 
                            : (true, false);
                        SetVisibility(normal, cthulhu);
                        disappearAnimation = WaitConditionAnimation.WaitTime(
                            TimeSpan.FromMilliseconds(appearInterval),
                            () => {
                                appearInterval = Math.Min(
                                    appearInterval + Constants.ButtonAppearIntervalIncrease, 
                                    Constants.MaxButtonAppearInterval * (allowCthulhu ? 1.5f : 1)
                                );
                                SetVisibility(false, false);
                                StartWaitButton();
                            }
                        ).Start(game);
                    }
                ).Start(game);
            };

            StartWaitButton();

            return new HintSymbol[] { SvgIcon.Timer, '!', SvgIcon.Button, SvgIcon.Tap };
        }
    }
}

