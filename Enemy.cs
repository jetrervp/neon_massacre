using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace nm
{
    public class Enemy
    {
        private readonly Texture2D _texture;
        private Vector2 _position;
        private Vector2 _velocity;

        public float Scale { get; set; } = 1.2f;
        public float Speed { get; set; } = 100f;
        public float Health { get; private set; }
        public int Damage { get; set; } = 1;

        private enum State { Idle, Chase, Attack }
        private State _currentState = State.Idle;
        private float _attackRange = 10f;
        private float _chaseRange = 1000f;
        private float _attackCooldown = 1.5f;
        private float _attackTimer = 1f;

        private float _hitFlashTimer = 0f;
        private const float HIT_FLASH_DURATION = 0.2f;
        private Color _currentColor = Color.White;

        public Vector2 Center => new Vector2(
            _position.X + (_texture.Width * Scale) / 2f,
            _position.Y + (_texture.Height * Scale) / 2f
        );

        public Rectangle WorldBounds => new Rectangle(
            (int)_position.X,
            (int)_position.Y,
            (int)(_texture.Width * Scale),
            (int)(_texture.Height * Scale)
        );

        public bool IsAlive => Health > 0;

        public Enemy(Texture2D texture, Vector2 spawnPosition, float health = 100f)
        {
            _texture = texture;
            _position = spawnPosition;
            Health = health;
        }

        public void Update(GameTime gameTime, Player player)
        {
            if (!IsAlive) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_hitFlashTimer > 0)
            {
                _hitFlashTimer -= deltaTime;
                float intensity = _hitFlashTimer / HIT_FLASH_DURATION;
                _currentColor = new Color(
                    255,
                    (byte)(255 * (1 - intensity)),
                    (byte)(255 * (1 - intensity))
                );
            }
            else
            {
                _currentColor = Color.White;
            }

            Vector2 toPlayer = player.WorldPosition - _position;
            float distance = toPlayer.Length();

            if (_attackTimer > 0) _attackTimer -= deltaTime;

            if (distance <= _attackRange && _attackTimer <= 0)
            {
                _currentState = State.Attack;
                Attack(player);
            }
            else if (distance <= _chaseRange && distance > 0.1f)
            {
                _currentState = State.Chase;
                Chase(toPlayer, deltaTime);
            }
            else
            {
                _currentState = State.Idle;
            }
        }

        private void Chase(Vector2 directionToPlayer, float deltaTime)
        {
            if (directionToPlayer.LengthSquared() < 1f)
                return;

            directionToPlayer.Normalize();
            _position += directionToPlayer * Speed * deltaTime;
        }

        private void Attack(Player player)
        {
            player.TakeDamage(Damage);
            _attackTimer = _attackCooldown;
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            _hitFlashTimer = HIT_FLASH_DURATION;
            if (Health <= 0) Health = 0;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (!IsAlive) return;

            Vector2 screenPos = _position - camera.Position;

            spriteBatch.Draw(
                _texture,
                screenPos,
                null,
                _currentColor,
                0f,
                Vector2.Zero,
                Scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}