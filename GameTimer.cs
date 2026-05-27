using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nm
{
    public class GameTimer
    {
        private SpriteFont _font;
        private float _elapsedTime = 0f;
        public float ElapsedTime => _elapsedTime;
        public SpriteFont Font => _font;

        // позиция на экране
        public Vector2 Position { get; set; }

        // цвет текста и тени
        public Color TextColor { get; set; } = Color.White;
        public Color ShadowColor { get; set; } = Color.Black;

        // cмещение тени
        public Vector2 ShadowOffset { get; set; } = new Vector2(2, 2);

        public bool ShowHours { get; set; } = true;

        public GameTimer(SpriteFont font, Vector2 position)
        {
            _font = font;
            Position = position;
        }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string timeString = FormatTime(_elapsedTime);

            // тень
            spriteBatch.DrawString(
                _font,
                timeString,
                Position + ShadowOffset,
                ShadowColor
            );

            //основной текст
            spriteBatch.DrawString(
                _font,
                timeString,
                Position,
                TextColor
            );
        }

        private string FormatTime(float totalSeconds)
        {
            int totalSecs = (int)totalSeconds;
            int minutes = (totalSecs % 3600) / 60;
            int seconds = totalSecs % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
