using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Zampara
{
    public abstract class LevelBase
    {
        public readonly Game Game;
        public LevelBase(Game _game)
        {
            this.Game = _game;
        }

        public abstract void HandleInput();

        public abstract void LoadContent();
        public abstract void UnloadContent();

        public abstract void Draw(GameTime _time);
        public abstract void Update(GameTime _time);
    }

    public class LevelWalker : LevelBase
    {
        const float WALKING_VELOCITY = 100.1f;

        Texture2D m_zamparaMan;
        Vector2 m_zamparaManPos;
        float m_zamparaManVelocity; // linear velocity

        public LevelWalker(Game _game)
            : base(_game)
        {
        }

        public bool IsWaiting
        {
            get
            {
                return Math.Abs(m_zamparaManVelocity) <= 0.0001;
            }
        }

        public override void HandleInput()
        {
            KeyboardState key = Keyboard.GetState();

            

            if (key.IsKeyDown(Keys.Right))
            {
                m_zamparaManVelocity = WALKING_VELOCITY;
            }
            else if (key.IsKeyDown(Keys.Left))
            {
                m_zamparaManVelocity = -WALKING_VELOCITY;
            }
            else
            {
                m_zamparaManVelocity = 0f;
            }
        }

        public override void LoadContent()
        {
            m_zamparaMan = Game.Content.Load<Texture2D>("zampara_man");
            m_zamparaManPos = new Vector2(0, Game.Window.ClientBounds.Height - m_zamparaMan.Height);
        }

        public override void UnloadContent()
        {
            
        }

        public override void Draw(GameTime _time)
        {
            SpriteBatch batch = new SpriteBatch(Game.GraphicsDevice);

            batch.Begin();
            batch.Draw(m_zamparaMan, new Rectangle((int)m_zamparaManPos.X, (int)m_zamparaManPos.Y, m_zamparaMan.Width, m_zamparaMan.Height), Color.White);
            batch.End();
        }

        public override void Update(GameTime _time)
        {
            m_zamparaManPos.X += m_zamparaManVelocity * _time.ElapsedGameTime.Milliseconds / 1000;
        }
    }
}
