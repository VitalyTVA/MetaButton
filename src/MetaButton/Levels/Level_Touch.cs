namespace ThatButtonAgain {
    static class Level_Touch {
        public static LevelContext Load(GameController game) {
            var button = game.CreateButton(() => game.StartNextLevelAnimation()).AddTo(game);

            game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
            });

            return GameControllerExtensions.TapButtonHint;
        }
    }
}

