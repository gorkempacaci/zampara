using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Zampara
{
    class LevelCredits : LevelBase
    {
        Texture2D m_image;
        double m_firstSeconds = 0;
        double m_lastSeconds = 0;

        public LevelCredits(ZamparaGame _game)
            : base(_game)
        { }

        public override void Initialize()
        {
            Game.KeyboardEvents.HandleKBKeyDown += new InputHandlers.Keyboard.KBHandler.DelHandleKBKeyDown(KeyboardEvents_HandleKBKeyDown);

        }

        public override void HandleInput()
        {
        }

        void KeyboardEvents_HandleKBKeyDown(Keys[] klist, Keys focus, InputHandlers.Keyboard.KBModifiers m)
        {
            if (m_lastSeconds - m_firstSeconds > 2)
            {
                Game.Restart();
            }
        }

        public override void LoadContent()
        {
            m_image = Game.Content.Load<Texture2D>("win2");
            Game.Content.Load<SoundEffect>("applause").Play();
        }

        public override void UnloadContent()
        {
            Game.KeyboardEvents.HandleKBKeyDown -= KeyboardEvents_HandleKBKeyDown;
        }

        public override void Draw(GameTime _time)
        {
            SpriteBatch batch = new SpriteBatch(Game.GraphicsDevice);

            batch.Begin();

            batch.Draw(m_image, new Rectangle(0, 0, m_image.Width, m_image.Height), Color.White);

            batch.End();
        }

        public override void Update(GameTime _time)
        {
            if (m_firstSeconds == 0)
            {
                m_firstSeconds = _time.TotalGameTime.TotalSeconds;
            }
            m_lastSeconds = _time.TotalGameTime.TotalSeconds;
        }
    }
}
