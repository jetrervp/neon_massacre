using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace nm
{
    public class ExperienceOrb
    {
        private readonly Texture2D _texture;
        private Vector2 _position;
        public int Value { get; private set; }
        public bool IsCollected { get; private set; } = false;

        public float Scale { get; set; } = 0.4f;

        // притяжение начинается когда игрок близко
        private float _attractRange = 100f;
        private float _attractSpeed = 200f;
        private bool _isAttracting = false;

        public Rectangle WorldBounds => new Rectangle(
            (int)_position.X,
            (int)_position.Y,
            (int)(_texture.Width * Scale),
            (int)(_texture.Height * Scale)
        );

        public ExperienceOrb(Texture2D texture, Vector2 position, int value = 10)
        {
            _texture = texture;
            _position = position;
            Value = value;
        }

        public void Update(GameTime gameTime, Player player)
        {
            if (IsCollected) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 toPlayer = player.WorldCenter - _position;
            float distance = toPlayer.Length();

            // начинаем притягиваться если игрок рядом
            if (distance <= _attractRange)
                _isAttracting = true;

            if (_isAttracting)
            {
                if (distance > 1f)
                {
                    toPlayer.Normalize();
                    float speed = _attractSpeed * (1f + (_attractRange - distance) / _attractRange);
                    _position += toPlayer * speed * deltaTime;
                }
            }

            // подбор при касании
            if (WorldBounds.Intersects(player.WorldBounds))
            {
                player.GainExperience(Value);
                IsCollected = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (IsCollected) return;

            Vector2 screenPos = _position - camera.Position;

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
    }
}