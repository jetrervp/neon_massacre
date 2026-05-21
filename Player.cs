using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace nm
{
    public class Player
    {
        private readonly Texture2D _texture;
        private Texture2D _bulletTexture;
        private readonly Texture2D _whiteTexture;

        public Vector2 WorldPosition { get; set; }
        public Vector2 ScreenPosition { get; private set; }

        public float Scale { get; set; } = 0.5f;
        public float Speed { get; set; } = 150f;
        public float Health { get; private set; }
        public float MaxHealth { get; set; } = 100f;
        public int BulletDamage { get; private set; } = 10;
        public bool IsAlive => Health > 0;

        // система уровней
        public int Level { get; private set; } = 1;
        public int CurrentExperience { get; private set; } = 0;
        public int ExperienceToNextLevel => Level * 100; // 100, 200, 300...

        private float _shootCooldown = 1.0f;
        private float _shootTimer = 0f;

        public Rectangle WorldBounds => new Rectangle(
            (int)WorldPosition.X,
            (int)WorldPosition.Y,
            (int)(_texture.Width * Scale),
            (int)(_texture.Height * Scale)
        );

        public Vector2 WorldCenter => new Vector2(
            WorldPosition.X + (_texture.Width * Scale) / 2f,
            WorldPosition.Y + (_texture.Height * Scale) / 2f
        );

        public Player(Texture2D texture, Vector2 worldStartPosition, Texture2D bulletTexture, Texture2D whiteTexture)
        {
            _texture = texture;
            _bulletTexture = bulletTexture;
            _whiteTexture = whiteTexture;
            WorldPosition = worldStartPosition;
            Health = MaxHealth;
        }

        public void GainExperience(int amount)
        {
            CurrentExperience += amount;

            while (CurrentExperience >= ExperienceToNextLevel)
            {
                CurrentExperience -= ExperienceToNextLevel;
                LevelUp();
            }
        }

        private void LevelUp()
        {
            Level++;

            // автоматические улучшения при каждом уровне
            MaxHealth += 10f;
            Health = MaxHealth;
            Speed += 5f;
            BulletDamage += 5;
            _shootCooldown = MathHelper.Max(0.2f, _shootCooldown - 0.05f);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, List<Projectile> projectiles, Camera camera)
        {
            if (!IsAlive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 movement = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                movement.X += 1;
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                movement.X -= 1;
            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                movement.Y += 1;
            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                movement.Y -= 1;

            if (movement != Vector2.Zero)
                movement.Normalize();

            WorldPosition += movement * Speed * deltaTime;

            _shootTimer -= deltaTime;
            if (_shootTimer <= 0 && _bulletTexture != null)
            {
                ShootInAllDirections(projectiles, camera);
                _shootTimer = _shootCooldown;
            }
        }

        private void ShootInAllDirections(List<Projectile> projectiles, Camera camera)
        {
            Vector2 spawnPos = GetVisualCenterWorldPosition(camera);

            var directions = new[]
            {
                new Vector2(0, -1),
                new Vector2(0, 1),
                new Vector2(-1, 0),
                new Vector2(1, 0),
                new Vector2(-1, -1),
                new Vector2(1, -1),
                new Vector2(-1, 1),
                new Vector2(1, 1)
            };

            foreach (var dir in directions)
            {
                var normalizedDir = dir;
                if (normalizedDir != Vector2.Zero)
                    normalizedDir.Normalize();

                var p = new Projectile(_bulletTexture, spawnPos, normalizedDir);
                p.Damage = BulletDamage;
                projectiles.Add(p);
            }
        }

        public void TakeDamage(float amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
        }

        public void UpdateScreenPosition(Camera mainCamera, Viewport viewport)
        {
            ScreenPosition = new Vector2(
                viewport.Width / 2f - (_texture.Width * Scale) / 2f,
                viewport.Height / 2f - (_texture.Height * Scale) / 2f
            );
        }

        public Vector2 GetVisualCenterWorldPosition(Camera camera)
        {
            Vector2 screenCenter = ScreenPosition + new Vector2(
                (_texture.Width * Scale) / 2f,
                (_texture.Height * Scale) / 2f
            );
            return screenCenter + camera.Position;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Viewport viewport)
        {
            if (!IsAlive) return;

            spriteBatch.Draw(
                _texture,
                ScreenPosition,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                Scale,
                SpriteEffects.None,
                0f
            );

            DrawHealthBar(spriteBatch);
            DrawExperienceBar(spriteBatch, font, viewport);
        }

        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            var barWidth = 40;
            var barHeight = 5;
            var barPos = ScreenPosition + new Vector2(
                (_texture.Width * Scale) / 2 - barWidth / 2,
                -barHeight - 18
            );

            spriteBatch.Draw(_whiteTexture,
                new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, barHeight),
                Color.Black);

            var healthWidth = (int)(barWidth * (Health / MaxHealth));
            spriteBatch.Draw(_whiteTexture,
                new Rectangle((int)barPos.X, (int)barPos.Y, healthWidth, barHeight),
                Color.Red);
        }

        private void DrawExperienceBar(SpriteBatch spriteBatch, SpriteFont font, Viewport viewport)
        {
            int barHeight = 8;
            int screenWidth = viewport.Width;

            // фон полоски
            spriteBatch.Draw(_whiteTexture,
                new Rectangle(0, 0, screenWidth, barHeight),
                Color.DarkBlue);

            // заполнение опытом
            int expWidth = (int)(screenWidth * ((float)CurrentExperience / ExperienceToNextLevel));
            spriteBatch.Draw(_whiteTexture,
                new Rectangle(0, 0, expWidth, barHeight),
                Color.DeepSkyBlue);

            // уровень справа сверху
            if (font != null)
            {
                string lvlText = $"LV {Level}";
                Vector2 textSize = font.MeasureString(lvlText);
                spriteBatch.DrawString(font, lvlText,
                    new Vector2(screenWidth - textSize.X - 10, barHeight + 4),
                    Color.White);
            }
        }
    }
}