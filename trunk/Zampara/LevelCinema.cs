using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Zampara
{
    public class LevelCinema : LevelBase
    {
        public enum ScenarioState
        { 
            InBed,
            Shouting,
            WindowFocus
        }

        Texture2D m_cine1;
        Texture2D m_cine2;
        Effect m_blurEffect;
        ScenarioState m_state;

        double beginTime;
        double lastTime;

        public LevelCinema(ZamparaGame _game)
            : base(_game)
        {
            m_state = ScenarioState.InBed;
        }

        public override void Initialize()
        {

        }

        public override void HandleInput()
        {

        }

        public override void LoadContent()
        {
            m_cine1 = Game.Content.Load<Texture2D>("cine1");
            m_cine2 = Game.Content.Load<Texture2D>("cine2");

        }

        public override void UnloadContent()
        {

        }

        public override void Draw(GameTime _time)
        {
            SpriteBatch batch = new SpriteBatch(Game.GraphicsDevice);

            batch.Begin();

            switch (m_state)
            { 
                case ScenarioState.InBed:
                    batch.Draw(m_cine1, new Rectangle(0, 0, m_cine1.Width, m_cine1.Height), Color.White);
                    break;
                case ScenarioState.Shouting:
                    batch.Draw(m_cine2, new Rectangle(0, 0, m_cine2.Width, m_cine2.Height), Color.White);
                    break;
                case ScenarioState.WindowFocus:
                    batch.Draw(m_cine2, new Rectangle(0, 0, m_cine2.Width, m_cine2.Height), Color.White);
                    break;
            }

            batch.End();
        }

        public override void Update(GameTime _time)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Game.Exit();
            }

            if (beginTime == 0)
            {
                beginTime = _time.TotalGameTime.TotalSeconds;
            }
            lastTime = _time.TotalGameTime.TotalSeconds;

            if (lastTime - beginTime > 3)
            {
                if (lastTime - beginTime > 6)
                {
                    m_state = ScenarioState.WindowFocus;
                }
                else
                {
                    m_state = ScenarioState.Shouting;
                }
            }
            if (m_state == ScenarioState.WindowFocus)
            {
                if (Keyboard.GetState().GetPressedKeys().Length > 0 || Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    Game.SwitchToWalkerLevel();
                }
            }
        }
    }
}