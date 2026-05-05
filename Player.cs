using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace nm
{
    public class Player
    {
        private readonly Texture2D _texture;
        private Vector2 _position;
        public float Scale { get; set; } = 0.7f; 

        public Vector2 Position => _position;

        public Player(Texture2D texture, Vector2 startPosition)
        {
            _texture = texture;
            _position = startPosition;
        }

        public void LockToCenter(Viewport viewport)
        {
            float playerWidth = _texture.Width * Scale;
            float playerHeight = _texture.Height * Scale;

            _position.X = viewport.Width / 2f - playerWidth / 2f;
            _position.Y = viewport.Height / 2f - playerHeight / 2f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _texture,
                _position,
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