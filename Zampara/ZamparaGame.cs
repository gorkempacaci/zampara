using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using InputHandlers.Mouse;
using InputHandlers.Keyboard;
using System.IO;


namespace Zampara
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ZamparaGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager m_graphics;
        LevelBase m_currentLevel;

        public LevelBase CurrentLevel
        {
            get
            {
                return m_currentLevel;
            }
        }

        public KBHandler KeyboardEvents;

        public ZamparaGame()
        {
            m_currentLevel = new LevelCinema(this); //new LevelCinema(this);
            m_graphics = new GraphicsDeviceManager(this);
            m_graphics.PreferredBackBufferWidth = 800;
            m_graphics.PreferredBackBufferHeight = 500;
            if (File.Exists(Directory.GetCurrentDirectory() + "fullscreen.txt"))
            {
                m_graphics.IsFullScreen = true;
            }
            
            KeyboardEvents = KBHandler.Instance;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            MouseHandler.Instance.Reset();
            KBHandler.Instance.Reset();
            m_currentLevel.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            m_currentLevel.LoadContent();
            

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            m_currentLevel.UnloadContent();
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardEvents.Poll(Keyboard.GetState(), gameTime);

            m_currentLevel.HandleInput();
            m_currentLevel.Update(gameTime);
            

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            m_currentLevel.Draw(gameTime);
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        public void SwitchToWalkerLevel()
        {
            m_currentLevel.UnloadContent();
            m_currentLevel = new LevelWalker(this);
            m_currentLevel.Initialize();
            m_currentLevel.LoadContent();
        }

        public void SwitchToWinGame()
        {
            m_currentLevel.UnloadContent();
            m_currentLevel = new LevelWin(this);
            m_currentLevel.Initialize();
            m_currentLevel.LoadContent();
        }

        public void SwitchToCredits()
        {
            m_currentLevel.UnloadContent();
            m_currentLevel = new LevelCredits(this);
            m_currentLevel.Initialize();
            m_currentLevel.LoadContent();
        }

        public void SwitchToGameOver()
        {
            m_currentLevel.UnloadContent();
            m_currentLevel = new LevelGameOver(this);
            m_currentLevel.Initialize();
            m_currentLevel.LoadContent();
        }

        public void Restart()
        {
            m_currentLevel.UnloadContent();
            m_currentLevel = new LevelCinema(this);
            m_currentLevel.Initialize();
            m_currentLevel.LoadContent();
        }
    }
}
