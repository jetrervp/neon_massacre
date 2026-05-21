using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace nm
{
    public class GameOverScreen
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _whiteTexture;
        private KeyboardState _prevKeyboardState;

        public bool IsRestartRequested { get; private set; } = false;

        public GameOverScreen(SpriteFont font, Texture2D whiteTexture)
        {
            _font = font;
            _whiteTexture = whiteTexture;
        }

        public void Update()
        {
            KeyboardState current = Keyboard.GetState();
            if (current.IsKeyDown(Keys.R) && _prevKeyboardState.IsKeyUp(Keys.R))
                IsRestartRequested = true;
            _prevKeyboardState = current;
        }

        public void Draw(SpriteBatch spriteBatch, Viewport viewport, float elapsedTime, int level)
        {
            int centerX = viewport.Width / 2;
            int centerY = viewport.Height / 2;

            // затемнение
            spriteBatch.Draw(_whiteTexture,
                new Rectangle(0, 0, viewport.Width, viewport.Height),
                Color.Black * 0.7f);

            // GAME OVER
            string gameOver = "GAME OVER";
            Vector2 gameOverSize = _font.MeasureString(gameOver);
            spriteBatch.DrawString(_font, gameOver,
                new Vector2(centerX - gameOverSize.X / 2f, centerY - 60),
                Color.Red);

            // время
            int totalSecs = (int)elapsedTime;
            string timeText = $"Time: {totalSecs / 60:D2}:{totalSecs % 60:D2}";
            Vector2 timeSize = _font.MeasureString(timeText);
            spriteBatch.DrawString(_font, timeText,
                new Vector2(centerX - timeSize.X / 2f, centerY - 20),
                Color.White);

            // уровень
            string levelText = $"Level: {level}";
            Vector2 levelSize = _font.MeasureString(levelText);
            spriteBatch.DrawString(_font, levelText,
                new Vector2(centerX - levelSize.X / 2f, centerY + 20),
                Color.White);

            // рестарт
            string restartText = "Press R to restart";
            Vector2 restartSize = _font.MeasureString(restartText);
            spriteBatch.DrawString(_font, restartText,
                new Vector2(centerX - restartSize.X / 2f, centerY + 60),
                Color.White);
        }
        public void Reset()
        {
            IsRestartRequested = false;
            _prevKeyboardState = default;
        }
    }
}