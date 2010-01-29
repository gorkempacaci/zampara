using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Zampara
{
    public abstract class LevelBase
    {
        public readonly Game Game;
        
        public abstract void LoadContent();
        public abstract void UnloadContent();

        public abstract void Draw(GameTime _time);
        public abstract void Update(GameTime _time);
    }

    public class LevelWalker : LevelBase
    {
        public override void LoadContent()
        {
            
        }

        public override void UnloadContent()
        {
            
        }

        public override void Draw(GameTime _time)
        {
            
        }

        public override void Update(GameTime _time)
        {
            
        }
    }
}
