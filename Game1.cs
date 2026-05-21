using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace nm
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _backgroundLayer1;
        private Texture2D _backgroundLayer2;
        private Vector2 _layer1Size;
        private Vector2 _layer2Size;
        private static Texture2D _whiteTexture;

        private Camera _cameraLayer1;
        private Camera _cameraLayer2;

        private Player _player;
        private Texture2D _playerTexture;
        private Texture2D _bulletTexture;

        private Texture2D _enemyTexture;
        private Texture2D _orbTexture; 
        private List<Enemy> _enemies = new List<Enemy>();
        private List<ExperienceOrb> _experienceOrbs = new List<ExperienceOrb>();

        private List<Projectile> _projectiles = new List<Projectile>();

        private GameTimer _gameTimer;
        private bool _isGameOver = false;
        private GameOverScreen _gameOverScreen;

        private float _spawnTimer = 0f;
        private float _spawnInterval = 0.5f;

        private Random _random = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _backgroundLayer1 = Content.Load<Texture2D>("bgl3");
            _backgroundLayer2 = Content.Load<Texture2D>("bgl21");

            _layer1Size = new Vector2(_backgroundLayer1.Width, _backgroundLayer1.Height);
            _layer2Size = new Vector2(_backgroundLayer2.Width, _backgroundLayer2.Height);

            _cameraLayer1 = new Camera(speed: 150f, parallaxFactor: 1.0f);
            _cameraLayer2 = new Camera(speed: 80f, parallaxFactor: 0.5f);

            _playerTexture = Content.Load<Texture2D>("player");
            _enemyTexture = Content.Load<Texture2D>("enemy");
            _bulletTexture = Content.Load<Texture2D>("bullet");
            _orbTexture = Content.Load<Texture2D>("orb");

            _whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });

            var playerStartWorld = new Vector2(1000f, 600f);
            _player = new Player(_playerTexture, playerStartWorld, _bulletTexture, _whiteTexture);

            _cameraLayer1.SetPosition(playerStartWorld - new Vector2(
                GraphicsDevice.Viewport.Width / 2f - (_playerTexture.Width * _player.Scale) / 2f,
                GraphicsDevice.Viewport.Height / 2f - (_playerTexture.Height * _player.Scale) / 2f
            ));
            _cameraLayer2.SetPosition(_cameraLayer1.Position * 0.5f);

            SpriteFont timerFont = Content.Load<SpriteFont>("TimerFont");
            Vector2 timerPosition = new Vector2(
                GraphicsDevice.Viewport.Width - 150,
                20
            );
            _gameTimer = new GameTimer(timerFont, timerPosition);
            _gameOverScreen = new GameOverScreen(_gameTimer.Font, _whiteTexture);
        }

        protected override void Update(GameTime gameTime)
        {
            if (_isGameOver)
            {
                _gameOverScreen.Update();
                if (_gameOverScreen.IsRestartRequested)
                    RestartGame();
                return;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();

            _player.Update(gameTime, keyboardState, _projectiles, _cameraLayer1);
            if (!_player.IsAlive)
                _isGameOver = true;

            _cameraLayer1.SetPosition(_player.WorldPosition - new Vector2(
                GraphicsDevice.Viewport.Width / 2f - (_playerTexture.Width * _player.Scale) / 2f,
                GraphicsDevice.Viewport.Height / 2f - (_playerTexture.Height * _player.Scale) / 2f
            ));
            _cameraLayer2.SetPosition(_cameraLayer1.Position * 0.5f);

            _player.UpdateScreenPosition(_cameraLayer1, GraphicsDevice.Viewport);

            foreach (var projectile in _projectiles)
                projectile.Update(gameTime);

            foreach (var projectile in _projectiles)
            {
                if (!projectile.IsActive) continue;

                foreach (var enemy in _enemies)
                {
                    if (!enemy.IsAlive) continue;

                    if (projectile.Bounds.Intersects(enemy.WorldBounds))
                    {
                        enemy.TakeDamage(projectile.Damage);
                        projectile.Deactivate();

                        // дроп опыта при смерти
                        if (!enemy.IsAlive)
                        {
                            var dropPos = enemy.TryDropExperience();
                            if (dropPos.HasValue)
                                _experienceOrbs.Add(new ExperienceOrb(_orbTexture, dropPos.Value));
                        }

                        break;
                    }
                }
            }

            _projectiles.RemoveAll(p => !p.IsActive);

            foreach (var enemy in _enemies)
            {
                enemy.Update(gameTime, _player);

                if (enemy.IsAlive && enemy.WorldBounds.Intersects(_player.WorldBounds))
                {
                    Vector2 pushDir = _player.WorldCenter - enemy.Center;
                    if (pushDir != Vector2.Zero) pushDir.Normalize();
                    _player.WorldPosition += pushDir * 200f * deltaTime;

                    enemy.DealContactDamage(_player, deltaTime);
                }
            }

            _enemies.RemoveAll(e => !e.IsAlive);

            // обновляем орбы
            foreach (var orb in _experienceOrbs)
                orb.Update(gameTime, _player);

            _experienceOrbs.RemoveAll(o => o.IsCollected);

            _gameTimer.Update(gameTime);

            _spawnTimer -= deltaTime;
            if (_spawnTimer <= 0)
            {
                int target = GetTargetEnemyCount();
                int toSpawn = target - _enemies.Count;

                for (int i = 0; i < toSpawn; i++)
                    SpawnEnemy();

                _spawnTimer = _spawnInterval;
            }

            if (!_player.IsAlive)
                _isGameOver = true;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.LinearWrap, blendState: BlendState.AlphaBlend);

            _cameraLayer2.Draw(_spriteBatch, GraphicsDevice, _backgroundLayer2, _layer2Size);
            _cameraLayer1.Draw(_spriteBatch, GraphicsDevice, _backgroundLayer1, _layer1Size);

            foreach (var orb in _experienceOrbs)
                orb.Draw(_spriteBatch, _cameraLayer1);

            foreach (var projectile in _projectiles)
                projectile.Draw(_spriteBatch, _cameraLayer1);

            foreach (var enemy in _enemies)
                enemy.Draw(_spriteBatch, _cameraLayer1);

            _player.Draw(_spriteBatch, _gameTimer.Font, GraphicsDevice.Viewport);

            _gameTimer.Draw(_spriteBatch);

            if (_isGameOver)
                _gameOverScreen.Draw(_spriteBatch, GraphicsDevice.Viewport,
                    _gameTimer.ElapsedTime, _player.Level);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private int GetTargetEnemyCount()
        {
            int minute = (int)(_gameTimer.ElapsedTime / 60f) + 1;
            return minute * 5;
        }

        private void SpawnEnemy()
        {
            Viewport viewport = GraphicsDevice.Viewport;
            float screenWidth = viewport.Width;
            float screenHeight = viewport.Height;
            float margin = 100f;

            int side = _random.Next(4);
            Vector2 spawnPosition;

            switch (side)
            {
                case 0:
                    spawnPosition = new Vector2(
                        _player.WorldPosition.X + _random.Next((int)screenWidth) - screenWidth / 2f,
                        _player.WorldPosition.Y - screenHeight / 2f - margin);
                    break;
                case 1:
                    spawnPosition = new Vector2(
                        _player.WorldPosition.X + screenWidth / 2f + margin,
                        _player.WorldPosition.Y + _random.Next((int)screenHeight) - screenHeight / 2f);
                    break;
                case 2:
                    spawnPosition = new Vector2(
                        _player.WorldPosition.X + _random.Next((int)screenWidth) - screenWidth / 2f,
                        _player.WorldPosition.Y + screenHeight / 2f + margin);
                    break;
                default:
                    spawnPosition = new Vector2(
                        _player.WorldPosition.X - screenWidth / 2f - margin,
                        _player.WorldPosition.Y + _random.Next((int)screenHeight) - screenHeight / 2f);
                    break;
            }

            _enemies.Add(new Enemy(_enemyTexture, spawnPosition, health: 25f));
        }
        private void RestartGame()
        {
            _isGameOver = false;
            _gameOverScreen.Reset();
            _enemies.Clear();
            _projectiles.Clear();
            _experienceOrbs.Clear();

            var playerStartWorld = new Vector2(1000f, 600f);
            _player = new Player(_playerTexture, playerStartWorld, _bulletTexture, _whiteTexture);

            _cameraLayer1.SetPosition(playerStartWorld - new Vector2(
                GraphicsDevice.Viewport.Width / 2f - (_playerTexture.Width * _player.Scale) / 2f,
                GraphicsDevice.Viewport.Height / 2f - (_playerTexture.Height * _player.Scale) / 2f
            ));
            _cameraLayer2.SetPosition(_cameraLayer1.Position * 0.5f);

            _gameTimer = new GameTimer(_gameTimer.Font, new Vector2(
                GraphicsDevice.Viewport.Width - 150, 20
            ));

            _spawnTimer = 0f;
        }
    }
}