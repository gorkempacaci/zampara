using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Resources;
using System.Reflection;

namespace Zampara
{
    public class LevelWalker : LevelBase
    {
        public const float WALKING_VELOCITY = 300.1f;
        public const int MAN_MINX = 50;
        public const int WALK_MINX = 100;
        public const int WALK_MAXX = 400;
        public const int MAN_MAXX = 650;
        public const int WALK_MINY = 325;
        public const int WALK_MAXY = 500;
        public const int HOMESY = 100;
        public const int WALLSY = 156;
        readonly WalkPath[] Paths; // initialized in constructor

        Effect m_blurEffect;

        

        Texture2D m_wall;
        Texture2D m_road;
        Texture2D m_clouds;
        Texture2D m_blocks;
        Texture2D m_grass;

        Texture2D m_home1;
        Texture2D m_home2;

        Texture2D[] m_availableTreeKinds;
        int[] m_treePositions;
        int[] m_treeKindIndices;

        Texture2D[] m_availableBinKinds;
        int[] m_binPositions;
        int[] m_binKindIndices;

        int[] m_wall1Coordinates;
        int[] m_wall2Coordinates;
        int[] m_wall3Coordinates;

        int[] m_home1Coordinates;
        int[] m_home2Coordinates;

        public int RoadOffset = 0;

        BoobyWoman m_boobyWoman;
        ZamparaMan m_zamparaMan;

        public LevelWalker(ZamparaGame _game)
            : base(_game)
        {
            m_boobyWoman = new BoobyWoman(this.Game);
            m_boobyWoman.Position = new Vector2(500, 450);
            m_boobyWoman.Velocity = new Vector2(-50, 0);

            Paths = new WalkPath[]
            {
                 new WalkPath{Y=220, Scale=0.8f},
                 new WalkPath{Y=270, Scale=0.9f},
                 new WalkPath{Y=320, Scale=1.0f}
            };
        }

        public bool IsWaiting
        {
            get
            {
                return Math.Abs(m_zamparaMan.Velocity.X) <= 0.0001;
            }
        }


        public int CurrentPosition
        {
            get
            {
                return RoadOffset + (int)m_zamparaMan.Position.X;
            }
        }


        public override void Initialize()
        {
            m_zamparaMan = new ZamparaMan(this.Game);
            Game.KeyboardEvents.HandleKBKeyDown += new InputHandlers.Keyboard.KBHandler.DelHandleKBKeyDown(KeyboardEvents_HandleKBKeyDown);
            m_zamparaMan.Velocity.Y = (WALK_MAXY - WALK_MINY) / 2;
        }

        void KeyboardEvents_HandleKBKeyDown(Keys[] klist, Keys focus, InputHandlers.Keyboard.KBModifiers m)
        {

        }

        public override void HandleInput()
        {
            m_zamparaMan.HandleInput();   
        }

        public override void LoadContent()
        {
            Random rand = new Random();


            m_road = Game.Content.Load<Texture2D>("road");
            
            m_blurEffect = Game.Content.Load<Effect>("radialblur");
            m_boobyWoman.Load();
            m_boobyWoman.IsEnabled = true;
            m_zamparaMan.Load();
            m_zamparaMan.IsEnabled = true;

            m_clouds = Game.Content.Load<Texture2D>("clouds");
            m_blocks = Game.Content.Load<Texture2D>("blocks");
            m_grass = Game.Content.Load<Texture2D>("grass");

            m_wall = Game.Content.Load<Texture2D>("wall");

            m_home1 = Game.Content.Load<Texture2D>("home1");
            m_home2 = Game.Content.Load<Texture2D>("home2");


            m_availableTreeKinds = new Texture2D[2];
            m_availableTreeKinds[0] = Game.Content.Load<Texture2D>("tree1");
            m_availableTreeKinds[1] = Game.Content.Load<Texture2D>("tree2");

            m_availableBinKinds = new Texture2D[1];
            m_availableBinKinds[0] = Game.Content.Load<Texture2D>("bin");

            m_zamparaMan.Position = new Vector2(0, Game.Window.ClientBounds.Height - m_zamparaMan.Height);

            // LOAD TREES
            ResourceManager res = new ResourceManager("GameObjects.resx", Assembly.GetExecutingAssembly());
            m_treePositions = GameObjects.Trees.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_treeKindIndices = m_treePositions.Select(x => rand.Next(0, m_availableTreeKinds.Length)).ToArray();
            m_binPositions = GameObjects.Bins.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_binKindIndices = m_binPositions.Select(x => rand.Next(0, m_availableBinKinds.Length)).ToArray();

            m_wall1Coordinates = GameObjects.Wall1.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_wall2Coordinates = GameObjects.Wall2.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_wall3Coordinates = GameObjects.Wall3.Split(',').Select(x => int.Parse(x.Trim())).ToArray();

            m_home1Coordinates = GameObjects.Home1.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_home2Coordinates = GameObjects.Home2.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        }

        public override void UnloadContent()
        {

        }

        public void DrawTile(SpriteBatch _batch, Texture2D _bitmap, int _offset, int _y, float _scale)
        {
            float bitmapWidth = _bitmap.Width * _scale;
            float bitmapHeight = _bitmap.Height * _scale;

            if (_offset >= bitmapWidth)
            {
                _offset = _offset % (int)bitmapWidth;
            }

            int howManyItemsNeeded = (int)Math.Ceiling((float)Game.Window.ClientBounds.Width / bitmapWidth) + 1;

            for (int i = 0; i < howManyItemsNeeded; i++)
            {
                int xPos = (int)(-_offset + i * bitmapWidth);
                _batch.Draw(_bitmap, new Rectangle(xPos, _y, (int)bitmapWidth, (int)bitmapHeight), Color.White);
            }
        }

        public override void Draw(GameTime _time)
        {
            SpriteBatch batch = new SpriteBatch(Game.GraphicsDevice);

            batch.Begin();

            int howManyRoadsAreBehind = RoadOffset / (m_road.Width / 2);

            DrawTile(batch, m_clouds, RoadOffset / 4, 0, 1f);

            DrawTile(batch, m_blocks, RoadOffset / 2, 100, .5f);

            DrawTile(batch, m_wall, (int)(RoadOffset / 1.1), 218, 1f);

            int homeLayerOffset = (int)(RoadOffset / 1.1);
            float homeLayerScale = 0.25f;

            for (int hi = 0; hi < m_home1Coordinates.Length; hi++)
            {
                batch.Draw(m_home1, new Rectangle(m_home1Coordinates[hi] - homeLayerOffset, HOMESY, (int)(m_home1.Width * homeLayerScale), (int)(m_home1.Height * homeLayerScale)), Color.White);
            }
            for (int hi = 0; hi < m_home2Coordinates.Length; hi++)
            {
                batch.Draw(m_home2, new Rectangle(m_home2Coordinates[hi] - homeLayerOffset, HOMESY, (int)(m_home2.Width * homeLayerScale), (int)(m_home2.Height * homeLayerScale)), Color.White);
            }

            DrawTile(batch, m_road, RoadOffset, 300, 0.5f);

            // TREES
            for (int ti = 0; ti < m_treePositions.Length; ti++)
            {
                Texture2D tree = m_availableTreeKinds[m_treeKindIndices[ti]];
                int treePosition = m_treePositions[ti];
                batch.Draw(tree, new Rectangle(treePosition - RoadOffset, 140, tree.Width / 2, tree.Height / 2), Color.White);
            }

            m_boobyWoman.Draw(_time, batch, -RoadOffset);

            // ZAMPARA
            m_zamparaMan.Draw(_time, batch, 0);

            // BINS
            for (int bi = 0; bi < m_binPositions.Length; bi++)
            {
                Texture2D bin = m_availableBinKinds[m_binKindIndices[bi]];
                int binPosition = m_binPositions[bi];
                batch.Draw(bin, new Rectangle(binPosition - RoadOffset, 430, bin.Width / 4, bin.Height / 4), Color.White);
            }

            DrawTile(batch, m_grass, (int)(RoadOffset * 1.2), 343, 0.4f);

            batch.End();
        }

        public override void Update(GameTime _time)
        {
            m_boobyWoman.Update(_time);
            m_zamparaMan.Update(_time);



            CheckForCollisionsAndAct();
        }

        public void CheckForCollisionsAndAct()
        { 
            
        }

    }
}
