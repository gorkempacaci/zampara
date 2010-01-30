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
        protected AnimationPlayer m_animationPlayer;
        public readonly ZamparaGame Game;
        public Vector2 Position;
        public Vector2 Velocity;
        public bool IsEnabled = false;

        public ActorBase(ZamparaGame _game)
        {
            Game = _game;
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
            m_walkingAnimation = new Animation(Game.Content.Load<Texture2D>("lady-walk"), 0.4f, true, 4);
        }

        public override void Draw(GameTime _time, SpriteBatch _batch, int _offset)
        {
            m_animationPlayer.PlayAnimation(m_walkingAnimation);
            m_animationPlayer.Draw(_time, _batch, new Vector2(Position.X + _offset, Position.Y), SpriteEffects.FlipHorizontally);
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
            m_animationPlayer.Draw(_time, _batch, new Vector2(Position.X + _offset, Position.Y), SpriteEffects.FlipHorizontally);
        }

        public BoobyWoman(ZamparaGame _game)
            :base(_game)
        {
            State = BoobyWomanState.Walking;
            Velocity = new Vector2(-100, 0);
        }
    }
}
