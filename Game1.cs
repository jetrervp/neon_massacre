using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace nm
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // текстуры фонов
        private Texture2D _backgroundLayer1;  // передний
        private Texture2D _backgroundLayer2;  // задний 
        private Vector2 _layer1Size;
        private Vector2 _layer2Size;

        // камеры для каждого слоя
        private Camera _cameraLayer1;
        private Camera _cameraLayer2;

        // игрок
        private Player _player;

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

            // фоны
            _backgroundLayer1 = Content.Load<Texture2D>("bgl3");  // верхний слой
            _backgroundLayer2 = Content.Load<Texture2D>("bgl21");   // нижний слой

            _layer1Size = new Vector2(_backgroundLayer1.Width, _backgroundLayer1.Height);
            _layer2Size = new Vector2(_backgroundLayer2.Width, _backgroundLayer2.Height);

            // скорость камер
            _cameraLayer1 = new Camera(speed: 150f, parallaxFactor: 1.0f);
            _cameraLayer2 = new Camera(speed: 80f, parallaxFactor: 0.5f);

            // текстура игрока
            var playerTexture = Content.Load<Texture2D>("player");

            // позиция игрока
            var startPosition = new Vector2(
                GraphicsDevice.Viewport.Width / 2f - playerTexture.Width / 2f,
                GraphicsDevice.Viewport.Height / 2f - playerTexture.Height / 2f
            );

            // создание игрока
            _player = new Player(playerTexture, startPosition);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();
            _player.LockToCenter(GraphicsDevice.Viewport);
            Vector2 cameraMovement = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                cameraMovement.X -= 1;
            if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                cameraMovement.X += 1;
            if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                cameraMovement.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                cameraMovement.Y += 1;

            _cameraLayer1.Move(cameraMovement, deltaTime);
            _cameraLayer2.Move(cameraMovement, deltaTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.LinearWrap,
                              blendState: BlendState.AlphaBlend);

            // для расчёта ширины экрана
            _cameraLayer2.Draw(_spriteBatch, GraphicsDevice, _backgroundLayer2, _layer2Size);
            _cameraLayer1.Draw(_spriteBatch, GraphicsDevice, _backgroundLayer1, _layer1Size);
            _player.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}