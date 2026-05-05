using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nm
{
    public class Camera
    {
        private Vector2 _position;
        private float _speed;
        private float _parallaxFactor;

        public Vector2 Position => _position;
        public float Speed { get => _speed; set => _speed = value; }

        public Camera(float speed, float parallaxFactor = 1.0f)
        {
            _position = Vector2.Zero;
            _speed = speed;
            _parallaxFactor = parallaxFactor;
        }

        public void Move(Vector2 direction, float deltaTime)
        {
            _position += direction * _speed * _parallaxFactor * deltaTime;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Texture2D texture, Vector2 textureSize)
        {
            float screenWidth = graphicsDevice.Viewport.Width;
            float screenHeight = graphicsDevice.Viewport.Height;

            float startX = _position.X % textureSize.X;
            if (startX > 0) startX -= textureSize.X;

            float startY = _position.Y % textureSize.Y;
            if (startY > 0) startY -= textureSize.Y;

            for (float y = startY; y < screenHeight; y += textureSize.Y)
            {
                for (float x = startX; x < screenWidth; x += textureSize.X)
                {
                    spriteBatch.Draw(texture, new Vector2(x, y), Color.White);
                }
            }
        }
    }
}
