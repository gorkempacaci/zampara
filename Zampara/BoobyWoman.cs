using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Zampara
{
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

        public override Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X + (Game.CurrentLevel as LevelWalker).RoadOffset, (int)Position.Y, Width, Height);
            }
        }

        public override void Load()
        {
            m_animation = new Animation(Game.Content.Load<Texture2D>("boobywoman"), 0.3f, true, 4);
            m_hitAnimation = new Animation(Game.Content.Load<Texture2D>("boobywoman-hit"), 0.1f, true, 3);
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
            Facing = FaceDirection.Left;
            Velocity = new Vector2(-50, 0);
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
