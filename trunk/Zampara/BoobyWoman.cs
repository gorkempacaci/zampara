using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        public ActorBase(ZamparaGame _game)
        {
            Game = _game;
        }

        public virtual bool IsMoving
        {
            get
            {
                return Math.Abs(Velocity.X) > 0 || Math.Abs(Velocity.Y) > 0;
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

    public class ZamparaMan : ActorBase
    {
        Animation m_walkingAnimation;

        public enum ZamparaManState
        { 
            Walking,
            HidingBehindTree,
            HindingBehindBin
        }

        public ZamparaMan(ZamparaGame _game)
            : base(_game)
        { 
        
        }

        public override void Load()
        {
            m_walkingAnimation = new Animation(Game.Content.Load<Texture2D>("lady-walk"), 0.2f, true, 4);
        }

        public override void Update(GameTime _time)
        {
            LevelWalker walkerLevel = Game.CurrentLevel as LevelWalker;

            int deltaY = (int)(Velocity.Y * _time.ElapsedGameTime.Milliseconds / 1000);
            int newY = (int)(Position.Y + deltaY);
            newY = Math.Min(LevelWalker.WALK_MAXY, Math.Max(LevelWalker.WALK_MINY, newY));


            int deltaX = (int)(Velocity.X * _time.ElapsedGameTime.Milliseconds / 1000);
            int newX = (int)(Position.X + deltaX);
            if (newX < LevelWalker.WALK_MINX)
            {
                int leftForRoadOffset = LevelWalker.WALK_MINX - newX;
                int roadOffsetPossible = walkerLevel.RoadOffset; // it can only go down to zero.
                if (roadOffsetPossible >= leftForRoadOffset)
                {
                    walkerLevel.RoadOffset -= leftForRoadOffset;
                }
                else
                {
                    walkerLevel.RoadOffset -= roadOffsetPossible;
                    int leftForWalkLastPiece = leftForRoadOffset - roadOffsetPossible;
                    int lastPiecePossible = (int)Position.X - LevelWalker.MAN_MINX;
                    if (lastPiecePossible >= leftForWalkLastPiece)
                    {
                        Position.X -= leftForWalkLastPiece;
                    }
                    else
                    {
                        Position.X -= lastPiecePossible;
                    }
                }

                newX = LevelWalker.WALK_MINX;
            }
            else
            {
                if (newX > LevelWalker.WALK_MAXX)
                {
                    int necessaryEffect = newX - LevelWalker.WALK_MAXX;
                    newX = LevelWalker.WALK_MAXX;
                    walkerLevel.RoadOffset += necessaryEffect;
                }

            }
            Position.X = newX;
            Position.Y = newY;
        }

        public override void Draw(GameTime _time, SpriteBatch _batch, int _offset)
        {
            m_animationPlayer.PlayAnimation(m_walkingAnimation);
            float scaleFactor = 0.8f + 0.2f * (Position.Y - LevelWalker.WALK_MINY) / (LevelWalker.WALK_MAXY - LevelWalker.WALK_MINY);
            GameTime animationTime;
            if (IsMoving)
            {
                animationTime = _time;
            }
            else
            {
                animationTime = new GameTime();
            }
            m_animationPlayer.Draw(animationTime, _batch, new Vector2(Position.X + _offset, Position.Y), (Facing == FaceDirection.Left ? SpriteEffects.None : SpriteEffects.FlipHorizontally), scaleFactor);
        }

        public void HandleInput()
        {
            KeyboardState key = Keyboard.GetState();

            if (key.IsKeyDown(Keys.Right))
            {
                Velocity.X = LevelWalker.WALKING_VELOCITY;
                Facing = FaceDirection.Right;
            }
            else if (key.IsKeyDown(Keys.Left))
            {
                Velocity.X = -LevelWalker.WALKING_VELOCITY;
                Facing = FaceDirection.Left;
            }
            else
            {
                Velocity.X = 0f;
            }

            if (key.IsKeyDown(Keys.Down))
            {
                Velocity.Y = (int)(LevelWalker.WALKING_VELOCITY * 0.7);
            }
            else if (key.IsKeyDown(Keys.Up))
            {
                Velocity.Y = (int)(-LevelWalker.WALKING_VELOCITY * 0.7);
            }
            else
            {
                Velocity.Y = 0f;
            }            
        }

        public override int Height
        {
            get { return m_walkingAnimation.FrameHeight; }
        }

        public override int Width
        {
            get { return m_walkingAnimation.FrameWidth; }
        }
    }

    public class BoobyWoman : ActorBase
    {
        public enum BoobyWomanState
        { 
            Walking,
            Hitting
        }

        Animation m_animation;
        Animation m_hitAnimation;

        public BoobyWomanState State;

        public override void Load()
        {
            m_animation = new Animation(Game.Content.Load<Texture2D>("boobywoman"), 0.3f, true, 4);
            m_hitAnimation = new Animation(Game.Content.Load<Texture2D>("boobywoman-hit"), 0.3f, true, 3);
            Velocity = new Vector2(-100, 0);
        }

        public override void Draw(GameTime _time, SpriteBatch _batch, int _offset)
        {
            switch (State)
            {
                case BoobyWomanState.Walking:
                    m_animationPlayer.PlayAnimation(m_animation);
                    break;
                case BoobyWomanState.Hitting:
                    m_animationPlayer.PlayAnimation(m_hitAnimation);
                    break;
            }
            m_animationPlayer.Draw(_time, _batch, new Vector2(Position.X + _offset, Position.Y), Facing == FaceDirection.Left ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
        }

        public BoobyWoman(ZamparaGame _game)
            :base(_game)
        {
            State = BoobyWomanState.Walking;
            Velocity = new Vector2(-100, 0);
            Facing = FaceDirection.Left;
        }

        public override int Height
        {
            get { return m_animation.FrameHeight; }
        }

        public override int Width
        {
            get { return m_animation.FrameWidth; }
        }
    }
}
