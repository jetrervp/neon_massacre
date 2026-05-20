using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace nm
{
    public class Projectile
    {
        private readonly Texture2D _texture;
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }

        public float Speed { get; set; } = 200f;
        public int Damage { get; set; } = 10;
        public float Scale { get; set; } = 0.7f;

        public bool IsActive { get; private set; } = true;

        public Rectangle Bounds => new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            (int)(_texture.Width * Scale),
            (int)(_texture.Height * Scale)
        );

        public Projectile(Texture2D texture, Vector2 spawnPosition, Vector2 direction)
        {
            _texture = texture;
            Position = spawnPosition;
            Velocity = direction * Speed;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Velocity * deltaTime;

            // удаление пули, если она ушла далеко за пределы мира
            if (Position.X < -2000 || Position.X > 6000 ||
                Position.Y < -2000 || Position.Y > 6000)
            {
                IsActive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (!IsActive) return;

            // конвертация мировой позиции пули в экранную для отрисовки
            var screenPos = Position - camera.Position;

            spriteBatch.Draw(
                _texture,
                screenPos,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                Scale,
                SpriteEffects.None,
                0f
            );
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}