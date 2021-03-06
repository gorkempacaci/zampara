﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Zampara
{
    public class ZamparaMan : ActorBase
    {
        const float GETTING_HIT_PENALTY_PER_SECOND = 20f;
        Animation m_walkingAnimation;
        Animation m_walkingAnimationShoes;
        Animation m_walkingAnimationShirt;
        Animation m_currentAnimation;

        Texture2D m_imageHit;

        private ZamparaManState m_state;
        public ZamparaManState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        public float Health = 100f;
        public int HidingInsideBinIndex = -1;

        public enum ZamparaManState
        {
            Waiting,
            Walking,
            HidingBehindTree,
            HidingInsideBin,
            GettingHit
        }

        public ZamparaMan(ZamparaGame _game)
            : base(_game)
        {
            State = ZamparaManState.Waiting;
        }

        public override void Load()
        {
            m_walkingAnimation = new Animation(Game.Content.Load<Texture2D>("behlul"), 0.15f, true, 10);
            m_walkingAnimationShoes = new Animation(Game.Content.Load<Texture2D>("behlul-ayakkabi"), 0.15f, true, 10);
            m_walkingAnimationShirt = new Animation(Game.Content.Load<Texture2D>("behlul-gomlek"), 0.15f, true, 10);
            m_currentAnimation = m_walkingAnimation;
            m_imageHit = Game.Content.Load<Texture2D>("behlul-hurt");
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

            if (State == ZamparaManState.GettingHit)
            {
                this.Health -= GETTING_HIT_PENALTY_PER_SECOND * (_time.ElapsedGameTime.Milliseconds / 1000f);
            }

        }

        public override int DrawX
        {
            get { return (int)Position.X; }
        }

        public override int DrawY
        {
            get { return (int)Position.Y; }
        }

        public void HideInsideBin(int binIndex)
        {
            HidingInsideBinIndex = binIndex;
            State = ZamparaManState.HidingInsideBin;
            Velocity = new Vector2();
        }

        public void GetOutOfBin()
        {
            State = ZamparaManState.Waiting;
            HidingInsideBinIndex = -1;
        }

        public void GrantShoes()
        {
            if (m_currentAnimation == m_walkingAnimation)
            {
                m_currentAnimation = m_walkingAnimationShoes;
            }
        }

        public void GrantShirt()
        {
            if (m_currentAnimation == m_walkingAnimationShoes)
            {
                m_currentAnimation = m_walkingAnimationShirt;
            }
        }

        public override void Draw(GameTime _time, SpriteBatch _batch, int _offset)
        {
            float scaleFactor = 0.8f + 0.2f * (Position.Y - LevelWalker.WALK_MINY) / (LevelWalker.WALK_MAXY - LevelWalker.WALK_MINY);
            switch (State)
            {
                case ZamparaManState.GettingHit:
                    _batch.Draw(m_imageHit, new Rectangle((int)(Position.X - _offset - 90), (int)Position.Y - Height + 20, (int)(Width * scaleFactor), (int)(Height * scaleFactor)), Color.White);
                    break;
                case ZamparaManState.HidingInsideBin:
                    // no draw.
                    break;
                default:
                    m_animationPlayer.PlayAnimation(m_currentAnimation);
                    GameTime animationTime;
                    animationTime = _time;
                    m_animationPlayer.Draw(animationTime, _batch, new Vector2(DrawX, DrawY), (Facing == FaceDirection.Right ? SpriteEffects.None : SpriteEffects.FlipHorizontally), scaleFactor, !IsMoving);
                    break;
            }
        }

        public void HandleInput()
        {
            KeyboardState key = Keyboard.GetState();
            if (State == ZamparaManState.HidingInsideBin)
            {
                if (key.IsKeyDown(Keys.Up))
                {
                    this.GetOutOfBin();
                }
            }
            else
            {
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

}
