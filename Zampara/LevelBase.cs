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
        public readonly ZamparaGame Game;
        public LevelBase(ZamparaGame _game)
        {
            this.Game = _game;
        }

        public abstract void Initialize();

        public abstract void HandleInput();

        public abstract void LoadContent();
        public abstract void UnloadContent();

        public abstract void Draw(GameTime _time);
        public abstract void Update(GameTime _time);
    }

    /// <summary>
    /// One of the three paths the man can walk over.
    /// </summary>
    public struct WalkPath
    {
        public int Y;
        public float Scale;
    }

    public class LevelWalker : LevelBase
    {
        const float WALKING_VELOCITY = 100.1f;
        readonly WalkPath[] Paths; // initialized in constructor

        Texture2D m_zamparaMan;
        Texture2D m_road;
        Texture2D m_barrel;

        Vector2 m_zamparaManPos;
        float m_zamparaManVelocity; // linear velocity
        int m_currentWalkPathIndex = 1;

        public LevelWalker(ZamparaGame _game)
            : base(_game)
        {
            Paths = new WalkPath[] {
                                     new WalkPath{Y=150, Scale=0.5f},
                                     new WalkPath{Y=225, Scale=0.75f},
                                     new WalkPath{Y=300, Scale=1.0f}
                                 };
        }

        public bool IsWaiting
        {
            get
            {
                return Math.Abs(m_zamparaManVelocity) <= 0.0001;
            }
        }

        public WalkPath CurrentWalkPath
        {
            get
            {
                return Paths[m_currentWalkPathIndex];
            }
        }

        /// <summary>
        /// Call with +1 to switch to fronter walkpath, -1 to switch to rearer walkpath.
        /// </summary>
        /// <param name="_diff"></param>
        public void ChangeCurrentWalkPath(int _diff)
        {
            int newIndex = m_currentWalkPathIndex + _diff;
            newIndex = Math.Max(0, Math.Min(Paths.Length - 1, newIndex));
            m_currentWalkPathIndex = newIndex;
        }

        public override void Initialize()
        {
            Game.KeyboardEvents.HandleKBKeyDown += new InputHandlers.Keyboard.KBHandler.DelHandleKBKeyDown(KeyboardEvents_HandleKBKeyDown);
        }

        void KeyboardEvents_HandleKBKeyDown(Keys[] klist, Keys focus, InputHandlers.Keyboard.KBModifiers m)
        {
            if (focus == Keys.Up)
            {
                ChangeCurrentWalkPath(-1);
            }
            else
            {
                if (focus == Keys.Down)
                {
                    ChangeCurrentWalkPath(+1);
                }
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
            m_road = Game.Content.Load<Texture2D>("road");
            m_barrel = Game.Content.Load<Texture2D>("varil");
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
            batch.Draw(m_road, new Rectangle(0, 300, m_road.Width/2, m_road.Height/2), Color.White);
            batch.Draw(m_zamparaMan, new Rectangle((int)m_zamparaManPos.X, (int)CurrentWalkPath.Y, m_zamparaMan.Width, m_zamparaMan.Height), Color.White);
            batch.Draw(m_barrel, new Rectangle(200, 400, (int)(m_barrel.Width * 0.25), (int)(m_barrel.Height * 0.25)), Color.White);
            batch.End();
        }

        public override void Update(GameTime _time)
        {
            m_zamparaManPos.X += m_zamparaManVelocity * _time.ElapsedGameTime.Milliseconds / 1000;
        }
    }
}
