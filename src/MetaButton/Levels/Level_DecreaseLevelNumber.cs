using MetaArt.Core;

namespace ThatButtonAgain {
    static class Level_DecreaseLevelNumber {
        public static LevelContext Load(GameController game) {
            var currentIndex = game.LevelIndex;

            void ChangeIndex(int delta) {
                currentIndex += delta;
                if(currentIndex < 0) currentIndex += GameController.Levels.Length;
                if(currentIndex >= GameController.Levels.Length) currentIndex -= GameController.Levels.Length;
                int digitIndex = 0;
                foreach(var digit in currentIndex.ToString()) {
                    game.levelNumberLeterrs[digitIndex].Value = digit;
                    digitIndex++;
                }
                for(int i = digitIndex; i < game.levelNumberLeterrs.Count; i++) {
                    game.levelNumberLeterrs[digitIndex].Value = ' ';
                }
            }

            ChangeIndex(-1);

            DateTime lastClickTime = DateTime.Now;
            var interval = TimeSpan.FromMilliseconds(300);
            var timer = DelegateAnimation.Timer(
                interval, 
                () => {
                    if(currentIndex != game.LevelIndex -1 && (DateTime.Now - lastClickTime) > interval)
                        ChangeIndex(+1);
                })
                .Start(game);

            var button = game.CreateButton(() => {
                if(currentIndex != game.LevelIndex) {
                    ChangeIndex(-1);
                    lastClickTime = DateTime.Now;
                    game.playSound(SoundKind.Tap);
                    if(currentIndex == game.LevelIndex) {
                        game.animations.RemoveAnimation(timer);
                    }
                } else {
                    game.StartNextLevelAnimation();
                }
            }).AddTo(game);

            game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
            });

            return new HintSymbol[] { SvgIcon.Button, SvgIcon.Tap, SvgIcon.Tap, SvgIcon.Elipsis, SvgIcon.Fast };
        }
    }
}

