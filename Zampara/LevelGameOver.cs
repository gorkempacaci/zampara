using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Zampara
{
    public class LevelGameOver : LevelBase
    {
        Texture2D m_image;

        public LevelGameOver(ZamparaGame _game)
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
            Game.Restart();
        }

        public override void LoadContent()
        {
            m_image = Game.Content.Load<Texture2D>("gameover");
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
            
        }
    }
}
