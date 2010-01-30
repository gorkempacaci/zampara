using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zampara
{
    public class BoobyWoman
    {
        public enum BoobyWomanState
        { 
            Walking,
            Hitting
        }

        public BoobyWomanState State;
        public int Velocity;
        public readonly Animation Animation;

        public BoobyWoman(ZamparaGame _game)
        {
            State = BoobyWomanState.Walking;
            Velocity = -100;
        }
    }
}
