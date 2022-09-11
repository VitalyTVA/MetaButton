namespace ThatButtonAgain {
    public class LetterArea {
        public const char X = 'X';
        public const char Robot = '¤';
        const char E = ' ';


        public static char[][] CreateSwapPlusShapeArea() =>
            new char[][] {
                new[] { X, X, E, X, X  },
                new[] { X, X, E, X, X  },
                new[] { 'H', 'C', 'U', 'O', 'T' },
                new[] { X, X, E, X, X  }
            };
        public static char[][] CreateSwapWShapeArea() =>
            new char[][] {
                new[] { E, X, E, X, E  },
                new[] { 'H', 'C', 'U', 'O', 'T' },
                new[] { X, E, X, E, X  }
            };
        public static char[][] CreateSwapHShapeArea() =>
            new char[][] {
                new[] { X, E, X, E, X  },
                new[] { 'H', 'C', 'U', 'O', 'T' },
                new[] { X, E, X, E, X  }
            };
        public static char[][] CreateArrowDirectedLetters() =>
            new char[][] {
                new[] { E, E, 'T', E, E  },
                new[] { E, E, 'O', E, E  },
                new[] { E, E, 'U', E, E  },
                new[] { E, E, 'C', E, E  },
                new[] { E, E, 'H', E, E  },
            };
        public static char[][] CreateMoveInLineArea() =>
            new char[][] {
                new[] { E, E, E, X, E  },
                new[] { E, 'H', E, 'C', E  },
                new[] { X, E, X, E, E  },
                new[] { 'U', E, E, E, E  },
                new[] { E, E, E, E, X  },
                new[] { E, E, E, E, E  },
                new[] { 'O', E, E, E, 'T'  },
            };
        public static char[][] CreateMoveAllArea() =>
            new char[][] {
                new[] { E, E, E, E, E  },
                new[] { E, E, X, E, E  },
                new[] { 'H', 'C', 'U', 'O', 'T' },
                new[] { E, E, X, E, E  },
                new[] { E, E, E, E, E  },
            };
        public static char[][] CreateSokobanArea() =>
            new char[][] {
                new[] { E, E, E, Robot, E, E, E  },
                new[] { E, E, E, 'T', E, E, E  },
                new[] { E, E, X, 'O', X, E, E  },
                new[] { E, E, E, 'U', E, E, E  },
                new[] { E, E, X, 'C', X, E, E  },
                new[] { E, E, E, 'H', E, E, E  },
                new[] { E, E, E, E, E, E, E  },
            };

        readonly char[][] area;
        public LetterArea(char[][] area) {
            this.area = area;
        }
        internal int Width => area[0].Length;
        internal int Height => area.Length;

        public int MoveLine(char letter, Direction direction) {
            int count = 0;
            while(Move(letter, direction)) count++;
            return count;
        }

        public (int row, int col, char letter)[] MoveAll(Direction direction) {
            var letters = GetLetters().Where(x => x.letter != X);
            if(direction is Direction.Down or Direction.Right) {
                letters = letters.Reverse();
            }
            return letters
                .ToArray()
                .Where(x => Move(x.letter, direction))
                .ToArray();
        }

        public char? GetLetter(int row, int col) { 
            if(row < 0 || col < 0) return null;
            if(row >= Height || col >= Width) return null;
            var letter = area[row][col];
            return letter is E or X ? null : letter;
        }

        public bool Move(char letter, Direction direction) {
            var (row, col) = LocateLetter(letter);
            bool TrySwapLetters(bool canApplyDelta, int deltaRow, int deltaCol) {
                if(!canApplyDelta)
                    return false;
                if(area[row + deltaRow][col + deltaCol] == E) {
                    area[row + deltaRow][col + deltaCol] = letter;
                    area[row][col] = E;
                    return true;
                }
                return false;
            }
            switch(direction) {
                case Direction.Left:
                    return TrySwapLetters(col > 0, 0, -1);
                case Direction.Right:
                    return TrySwapLetters(col < Width - 1, 0, 1);
                case Direction.Up:
                    return TrySwapLetters(row > 0, -1, 0);
                case Direction.Down:
                    return TrySwapLetters(row < Height - 1, 1, 0);
                default:
                    throw new InvalidOperationException();
            }
        }

        public (int row, int col) LocateLetter(char letter) {
            var (row, col, _) = GetLetters().First(x => x.letter == letter);
            return (row, col);
        }

        public IEnumerable<(int row, int col, char letter)> GetLetters() {
            for(int row = 0; row < Height; row++) {
                for(int col = 0; col < Width; col++) {
                    var letter = area[row][col];
                    if(letter != E)
                        yield return (row, col, letter);
                }
            }
        }
    }
}

