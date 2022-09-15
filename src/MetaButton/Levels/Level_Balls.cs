using MetaArt.Core;
namespace ThatButtonAgain {
    static class Level_Balls {
        public static LevelContext Load_20Level(GameController game) {
            game.VerifyExpectedLevelIndex(20);
            game.RemoveLastLevelLetter();
            bool ballSnapped = false;
            LoadCore(
                game,
                onElementsCreated: (letters, balls) => { },
                onHit: (b1, b2) => false,
                shouldBallBeOutsideButton: ball => true,
                shouldReloadLevel: () => false,
                canEnableButton: () => ballSnapped,
                ballsUpdated: simulation => {
                    if(ballSnapped)
                        return;
                    foreach(var ball in simulation.GetBalls()) {
                        if((game.LevelNumberElementRect().TopRight - ball.Element().Rect.Location).Length() < game.GetSnapDistance()) { 
                            ballSnapped = true;
                            simulation.RemoveBall(ball);
                            var element = ball.Element();
                            element.Rect = Rect.FromCenter(
                                new Vector2(game.LevelNumberElementRect().Right + element.Rect.Width / 2, game.LevelNumberElementRect().MidY),
                                element.Rect.Size
                            );
                            element.State = BallState.Disabled;
                            game.playSound(SoundKind.SuccessSwitch);
                            break;
                        }
                    }
                }
            );
            return new[] {
                new HintSymbol[] { SvgIcon.Ball, SvgIcon.Arrows },
                new HintSymbol[] { SvgIcon.Ball, SvgIcon.Up, '1' },
                GameControllerExtensions.TapButtonHint,
            };
        }
        public static LevelContext Load_KeepO(GameController game) {
            Ball oBall = null!;
            Vector2 oBallLocation = default;
            LoadCore(
                game,
                onElementsCreated: (letters, balls) => {
                    letters[1].IsVisible = false;
                    oBall = balls.ElementAt(1);
                    oBallLocation = oBall.Element().Rect.Location;
                },
                onHit: (b1, b2) => {
                    if(b1 == oBall || b2 == oBall) {
                        b1.Element().State = BallState.Broken;
                        b2.Element().State = BallState.Broken;
                        game.playSound(SoundKind.BrakeBall);
                        return true;
                    }
                    return false;
                },
                shouldBallBeOutsideButton: ball => ball != oBall,
                shouldReloadLevel: () => !MathF.VectorsEqual(oBallLocation, oBall.Element().Rect.Location),
                canEnableButton: () => true,
                ballsUpdated: simulation => { }
            );

            return new[] {
                new HintSymbol[] { SvgIcon.Ball, SvgIcon.Arrows },
                new HintSymbol[] { 'O', SvgIcon.Alert },
                GameControllerExtensions.TapButtonHint,
            };
        }
        static void LoadCore(
            GameController game,
            Action<Letter[], IEnumerable<Ball>> onElementsCreated,
            Func<Ball, Ball, bool> onHit,
            Func<Ball, bool> shouldBallBeOutsideButton,
            Func<bool> shouldReloadLevel,
            Func<bool> canEnableButton,
            Action<BallsSimulation> ballsUpdated
        ) {
            //var balls = new Ball[12];
            //for(int i = 0; i < balls.Length; i++) {
            //    balls[i] = new Ball(MathF.Random(0, scene.width), MathF.Random(0, scene.height), MathF.Random(30, 70));
            //}

            bool win = false;
            var button = game.CreateButton(() => {
                win = true;
                game.StartNextLevelAnimation();
            }).AddTo(game);
            button.IsEnabled = false;
            button.Rect = button.Rect.Offset(new Vector2(0, -button.Rect.Width / 2));

            var hitBallLocation = new Vector2(button.Rect.MidX, game.scene.height - button.Rect.Width * .7f);
            var spring = new Line { From = hitBallLocation, To = hitBallLocation, Thickness = Constants.ButtonBorderWeight }.AddTo(game);

            var letters = game.CreateLetters((letter, index) => {
                letter.Rect = game.GetLetterTargetRect(index, button.Rect);
                letter.Scale = new Vector2(.65f);
            });

            var diameter = game.letterDragBoxWidth * .7f;
            var simulation = new BallsSimulation(size: null, gravity: 0, onHit: (b1, b2) => {
                if(!onHit(b1, b2)) {
                    game.playSound(SoundKind.Tap);
                }
            });
            Ball CreateBall(Vector2 center) {
                var ball = new Ball(new BallElement { Rect = Rect.FromCenter(center, new Vector2(diameter)) }, diameter) {
                    x = center.X,
                    y = center.Y,
                };
                ball.Element().AddTo(game);
                return ball;
            }

            foreach(var item in letters) {
                simulation.AddBall(CreateBall(item.Rect.Mid));
            }

            onElementsCreated(letters, simulation.GetBalls());

            void SetLocation(Ball ball, Vector2 location) {
                ball.x = location.X;
                ball.y = location.Y;
                ball.Element().Rect = Rect.FromCenter(location, ball.Element().Rect.Size);
            }

            float maxSpringLength = game.buttonWidth * 0.5f;

            Ball? hitBall = null;
            void CreateHitBall() {
                hitBall = CreateBall(hitBallLocation);
                hitBall.Element().HitTestVisible = true;
                var startLocation = hitBall.Element().Rect.Mid;
                hitBall.Element().GetPressState = DragInputState.GetDragHandler(
                    onDrag: delta => {
                        var deltaLength = delta.Length();
                        if(MathF.Greater(deltaLength, 0))
                            delta *= MathF.Min(maxSpringLength, deltaLength) / deltaLength;
                        SetLocation(hitBall, startLocation + delta);
                        spring.To = hitBall.Element().Rect.Mid;
                        return true;
                    },
                    onRelease: delta => {
                        spring.To = spring.From;
                        if(delta.Length() > game.GetSnapDistance()) {
                            delta = -delta * .15f / Steps;
                            hitBall.vx = delta.X;
                            hitBall.vy = delta.Y;
                            hitBall.Element().GetPressState = null;
                            simulation.AddBall(hitBall);
                            hitBall = null;
                        } else {
                            SetLocation(hitBall, startLocation);
                        }
                    }
                );
            }

            CreateHitBall();

            var toRemove = new List<(Ball, BallElement)>();
            new DelegateAnimation(deltaTime => {
                for(int i = 0; i < Steps; i++) {
                    simulation.NextFrame();
                }
                foreach(var ball in simulation.GetBalls()) {
                    SetLocation(ball, new Vector2(ball.x, ball.y));
                    if(!ball.Element().Rect.Intersects(new Rect(0, 0, game.scene.width, game.scene.height)))
                        toRemove.Add((ball, ball.Element()));
                }
                foreach(var (ball, element) in toRemove) {
                    game.scene.RemoveElement(ball.Element());
                    simulation.RemoveBall(ball);
                }
                toRemove.Clear();
                ballsUpdated(simulation);
                var reloadArea = Rect.FromCenter(hitBallLocation, new Vector2(maxSpringLength + diameter) * 2);
                if(hitBall == null && !simulation.GetBalls().Any(x => reloadArea.Intersects(x.Element().Rect)))
                    CreateHitBall();
                button.IsEnabled = canEnableButton() && !simulation.GetBalls().Any(x => shouldBallBeOutsideButton(x) && button.Rect.Intersects(x.Element().Rect));
                if(shouldReloadLevel()) {
                    game.StartReloadLevelAnimation();
                    return false;
                }
                return !win;
            }).Start(game);
        }
        const int Steps = 10;
        static BallElement Element(this Ball ball) => (BallElement)ball.payload;
    }
}

