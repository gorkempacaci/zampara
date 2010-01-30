using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Resources;
using System.Reflection;

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




}
