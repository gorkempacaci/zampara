using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Zampara
{
    public abstract class ActorBase
    {
        public enum FaceDirection
        {
            Left,
            Right
        }

        protected AnimationPlayer m_animationPlayer;
        public readonly ZamparaGame Game;
        public Vector2 Position;
        public Vector2 Velocity;
        public FaceDirection Facing = FaceDirection.Right;
        public bool IsEnabled = false;

        public abstract int Height { get; }
        public abstract int Width { get; }

        public abstract int DrawX { get; }
        public abstract int DrawY { get; }

        public ActorBase(ZamparaGame _game)
        {
            Game = _game;
        }

        protected LevelWalker WalkerLevel
        {
            get
            {
                return (Game.CurrentLevel as LevelWalker);
            }
        }

        public virtual bool IsMoving
        {
            get
            {
                return Math.Abs(Velocity.X) > 0.001 || Math.Abs(Velocity.Y) > 0.001;
            }
        }

        public virtual Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            }
        }

        public abstract void Load();
        public virtual void Update(GameTime _time)
        {
            if (IsEnabled)
            {
                Position = Position + Velocity * _time.ElapsedGameTime.Milliseconds / 1000f;
            }
        }
        public abstract void Draw(GameTime _time, SpriteBatch _batch, int _offset);

    }
}
