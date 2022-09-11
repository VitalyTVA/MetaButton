using MetaArt.Core;

namespace ThatButtonAgain {
    public class Ball {
        public readonly object payload;
        public readonly float diameter;

        public Ball(object payload, float diameter) {
            this.payload = payload;
            this.diameter = diameter;
        }

        public float x, y;
        public float vx = 0;
        public float vy = 0;
    }
    public class BallsSimulation {
        public BallsSimulation(Vector2? size, float gravity, Action<Ball, Ball> onHit) {
            this.size = size;
            this.gravity = gravity;
            this.onHit = onHit;
        }

        public void AddBall(Ball ball) {
            balls.Add(ball);
        }

        public void RemoveBall(Ball ball) {
            balls.Remove(ball);
        }

        const float spring = 0.05f;
        const float friction = -0.9f;
        readonly Vector2? size;
        private readonly float gravity;
        private readonly Action<Ball, Ball> onHit;
        List<Ball> balls = new();

        //public IEnumerable<Ball> GetBalls() => balls;

        public void NextFrame() {
            for(int i = 0; i < balls.Count; i++) {
                var ball = balls[i];
                for(int j = i + 1; j < balls.Count; j++) {
                    var other = balls[j];
                    CollideBalls(ball, other);
                }
                MoveBall(ball);
            }
        }

        HashSet<(Ball, Ball)> collisions = new();

        void CollideBalls(Ball ball, Ball other) {
            float dx = other.x - ball.x;
            float dy = other.y - ball.y;
            float distance = MathFEx.Sqrt(dx * dx + dy * dy);
            float minDist = other.diameter / 2 + ball.diameter / 2;
            if(distance < minDist) {
                if(!collisions.Contains((ball, other)) && !collisions.Contains((other, ball))) {
                    collisions.Add((ball, other));
                    collisions.Add((other, ball));
                    onHit(ball, other);
                }
                float angle = MathFEx.Atan2(dy, dx);
                float targetX = ball.x + MathFEx.Cos(angle) * minDist;
                float targetY = ball.y + MathFEx.Sin(angle) * minDist;
                float ax = (targetX - other.x) * spring;
                float ay = (targetY - other.y) * spring;
                ball.vx -= ax;
                ball.vy -= ay;
                other.vx += ax;
                other.vy += ay;
            } else {
                collisions.Remove((ball, other));
                collisions.Remove((other, ball));
            }
        }

        internal IEnumerable<Ball> GetBalls() {
            return balls;
        }

        void MoveBall(Ball ball) {
            ball.vy += gravity;
            ball.x += ball.vx;
            ball.y += ball.vy;
            if(size != null) {
                var width = size.Value.X;
                var height = size.Value.Y;
                if(ball.x + ball.diameter / 2 > width) {
                    ball.x = width - ball.diameter / 2;
                    ball.vx *= friction;
                } else if(ball.x - ball.diameter / 2 < 0) {
                    ball.x = ball.diameter / 2;
                    ball.vx *= friction;
                }
                if(ball.y + ball.diameter / 2 > height) {
                    ball.y = height - ball.diameter / 2;
                    ball.vy *= friction;
                } else if(ball.y - ball.diameter / 2 < 0) {
                    ball.y = ball.diameter / 2;
                    ball.vy *= friction;
                }
            }
        }
    }
}

