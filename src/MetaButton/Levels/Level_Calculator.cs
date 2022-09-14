using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_Calculator {
        static readonly Dictionary<char, int> charToDigit = new() {
            { 'O', 0 },
            { 'I', 1 },
            { 'L', 2 },
            { 'K', 3 },
            { 'H', 4 },
            { 'C', 5 },
            { 'U', 6 },
            { 'T', 7 },
        };
        static readonly Dictionary<int, char> digitToChar = charToDigit.ToDictionary(x => x.Value, x => x.Key);
        public static LevelContext Load(GameController game) {
            var buttonRect = game.GetButtonRect().Offset(new Vector2(0, -game.letterDragBoxHeight));

            var click = StringToNumber("CLICK");
            var touch = StringToNumber("TOUCH");
            Debug.Assert(click + StringToNumber("IUCOI") == touch);

            var clickLetters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, buttonRect, row: -2);
            }, "CLICK");
            var answerLetters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, buttonRect, row: -1);
            }, "    O");
            var signLetter = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(-1, buttonRect, row: -1.5f);
            }, "+");
            var touchLetters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, buttonRect);
            }, "     ");

            int answer = 0;
            Button button = null!;
            void DisabledClick() {
                button!.IsVisible = false;
                answer = 0;
                FillLetters(answerLetters!, 0);
                FillLetters(touchLetters!, 0);
                touchLetters.Last().Value = ' ';
            }
            button = game.CreateButton(() => game.StartNextLevelAnimation(), () => DisabledClick());
            button.IsVisible = false;
            button.Rect = buttonRect;
            game.scene.AddElementBehind(button);




            var digitLetters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index % 3 + 1, button.Rect, row: index / 3 + 1.5f);
                letter.ActiveRatio = 0;
                letter.HitTestVisible = true;
                if(letter.Value != '=')
                    letter.Value = digitToChar[(byte)letter.Value - (byte)'0'];
                letter.GetPressState = TapInputState.GetClickHandler(
                    letter,
                    () => {
                        if(button.IsVisible) {
                            game.playSound(SoundKind.ErrorClick);
                            return;
                        }
                        if(letter.Value != '=') {
                            if(answerLetters[0].Value != ' ') {
                                game.playSound(SoundKind.ErrorClick);
                                return;
                            }
                            game.playSound(SoundKind.Tap);
                            answer = answer * 8 + charToDigit[letter.Value];
                            FillLetters(answerLetters, answer);
                        } else {
                            game.playSound(SoundKind.Tap);
                            FillLetters(touchLetters, click + answer);
                            button.IsVisible = true;
                            button.IsEnabled = click + answer == touch;
                        }
                    },
                    setState: isPressed => {
                        letter.ActiveRatio = isPressed ? 1 : 0;
                    }
                );
            }, "56723401=");

            return new[] {
                new HintSymbol[] { 'I', 'U', 'C', 'O', 'I' },
                new HintSymbol[] { '=' },
                GameControllerExtensions.TapButtonHint,
            };
        }
        static int StringToNumber(string s) => s
            .Select((c, i) => (c, i: s.Length - 1 - i))
            .Sum(x => charToDigit![x.c] * (1 << x.i * 3));

        static List<char> NumberToLetters(int n) {
            var result = new List<char>(5);
            do {
                result.Add(digitToChar![n % 8]);
                n /= 8;
            } while(n > 0);
            return result;
        }

        static void FillLetters(Letter[] letters, int number) {
            var digits = NumberToLetters(number);
            for(int i = 0; i < 5; i++) {
                letters[4 - i].Value = i < digits.Count ? digits[i] : ' ';
            }
        }
    }
}

