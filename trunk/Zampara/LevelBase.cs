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

    public class LevelWalker : LevelBase
    {
        const float WALKING_VELOCITY = 200.1f;
        const int MAN_MINX = 50;
        const int WALK_MINX = 100;
        const int WALK_MAXX = 400;
        const int MAN_MAXX = 650;
        const int HOMESY = 100;
        readonly WalkPath[] Paths; // initialized in constructor

        Texture2D m_zamparaMan;
        Texture2D m_road;
        Texture2D m_clouds;
        Texture2D m_blocks;
        Texture2D m_grass;
        Texture2D m_wall1;
        Texture2D m_wall2;
        Texture2D m_wall3;
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

        Vector2 m_zamparaManPos;
        float m_zamparaManVelocity; // linear velocity
        int m_roadOffset = 0;
        int m_currentWalkPathIndex = 1;

        public LevelWalker(ZamparaGame _game)
            : base(_game)
        {
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

        public int CurrentPosition
        {
            get
            {
                return m_roadOffset + (int)m_zamparaManPos.X;
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
            Random rand = new Random();
            m_road = Game.Content.Load<Texture2D>("road");
            m_zamparaMan = Game.Content.Load<Texture2D>("zampara_man");
            m_clouds = Game.Content.Load<Texture2D>("clouds");
            m_blocks = Game.Content.Load<Texture2D>("blocks");
            m_grass = Game.Content.Load<Texture2D>("grass");

            m_wall1 = Game.Content.Load<Texture2D>("wall1");
            m_wall2 = Game.Content.Load<Texture2D>("wall2");
            m_wall3 = Game.Content.Load<Texture2D>("wall3");

            m_home1 = Game.Content.Load<Texture2D>("home1");
            m_home2 = Game.Content.Load<Texture2D>("home2");


            m_availableTreeKinds = new Texture2D[2];
            m_availableTreeKinds[0] = Game.Content.Load<Texture2D>("tree1");
            m_availableTreeKinds[1] = Game.Content.Load<Texture2D>("tree2");

            m_availableBinKinds = new Texture2D[1];
            m_availableBinKinds[0] = Game.Content.Load<Texture2D>("bin");
            
            m_zamparaManPos = new Vector2(0, Game.Window.ClientBounds.Height - m_zamparaMan.Height);
            
            // LOAD TREES
            ResourceManager res = new ResourceManager("GameObjects.resx", Assembly.GetExecutingAssembly());
            m_treePositions = GameObjects.Trees.Split(',').Select(x=>int.Parse(x.Trim())).ToArray();
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

            int howManyRoadsAreBehind = m_roadOffset / (m_road.Width / 2);

            DrawTile(batch, m_clouds, m_roadOffset / 4, 0, 1f);
            DrawTile(batch, m_blocks, m_roadOffset / 2, 50, 0.5f);

            int homeLayerOffset = (int)(m_roadOffset / 1.5);
            float homeLayerScale = 0.25f;
            for (int wi = 0; wi < m_wall1Coordinates.Length; wi++)
            {
                batch.Draw(m_wall1, new Rectangle(m_wall1Coordinates[wi] - homeLayerOffset, HOMESY, (int)(m_wall1.Width * homeLayerScale), (int)(m_wall1.Height * homeLayerScale)), Color.White);
            }
            for (int wi = 0; wi < m_wall1Coordinates.Length; wi++)
            {
                batch.Draw(m_wall2, new Rectangle(m_wall2Coordinates[wi] - homeLayerOffset, HOMESY, (int)(m_wall2.Width * homeLayerScale), (int)(m_wall2.Height * homeLayerScale)), Color.White);
            }
            for (int wi = 0; wi < m_wall1Coordinates.Length; wi++)
            {
                batch.Draw(m_wall3, new Rectangle(m_wall3Coordinates[wi] - homeLayerOffset, HOMESY, (int)(m_wall3.Width * homeLayerScale), (int)(m_wall3.Height * homeLayerScale)), Color.White);
            }
            for (int hi = 0; hi < m_home1Coordinates.Length; hi++)
            {
                batch.Draw(m_home1, new Rectangle(m_home1Coordinates[hi] - homeLayerOffset, HOMESY, (int)(m_home1.Width * homeLayerScale), (int)(m_home1.Height * homeLayerScale)), Color.White);
            }
            for (int hi = 0; hi < m_home2Coordinates.Length; hi++)
            {
                batch.Draw(m_home2, new Rectangle(m_home2Coordinates[hi] - homeLayerOffset, HOMESY, (int)(m_home2.Width * homeLayerScale), (int)(m_home2.Height * homeLayerScale)), Color.White);
            }

            DrawTile(batch, m_road, m_roadOffset, 300, 0.5f);

            // TREES
            for (int ti = 0; ti < m_treePositions.Length; ti++)
            {
                Texture2D tree = m_availableTreeKinds[m_treeKindIndices[ti]];
                int treePosition = m_treePositions[ti];
                batch.Draw(tree, new Rectangle(treePosition - m_roadOffset, 140, tree.Width / 2, tree.Height / 2), Color.White);
            }

            // ZAMPARA
            batch.Draw(m_zamparaMan, new Rectangle((int)m_zamparaManPos.X, (int)CurrentWalkPath.Y, (int)(m_zamparaMan.Width * CurrentWalkPath.Scale), (int)(m_zamparaMan.Height * CurrentWalkPath.Scale)), Color.White);

            // BINS
            for (int bi = 0; bi < m_binPositions.Length; bi++)
            {
                Texture2D bin = m_availableBinKinds[m_binKindIndices[bi]];
                int binPosition = m_binPositions[bi];
                batch.Draw(bin, new Rectangle(binPosition - m_roadOffset, 430, bin.Width / 4, bin.Height / 4), Color.White);
            }

            DrawTile(batch, m_grass, (int)(m_roadOffset*1.2), 343, 0.4f);

            batch.End();


            Game.Window.Title = CurrentPosition.ToString();
        }

        public override void Update(GameTime _time)
        {
            int deltaX = (int)(m_zamparaManVelocity * _time.ElapsedGameTime.Milliseconds / 1000);
            int newX = (int)(m_zamparaManPos.X + deltaX);
            if (newX < WALK_MINX)
            {
                int leftForRoadOffset = WALK_MINX - newX;
                int roadOffsetPossible = m_roadOffset; // it can only go down to zero.
                if (roadOffsetPossible >= leftForRoadOffset)
                {
                    m_roadOffset -= leftForRoadOffset;
                }
                else
                {
                    m_roadOffset -= roadOffsetPossible;
                    int leftForWalkLastPiece = leftForRoadOffset - roadOffsetPossible;
                    int lastPiecePossible = (int)m_zamparaManPos.X - MAN_MINX;
                    if (lastPiecePossible >= leftForWalkLastPiece)
                    {
                        m_zamparaManPos.X -= leftForWalkLastPiece;
                    }
                    else
                    {
                        m_zamparaManPos.X -= lastPiecePossible;
                    }
                }

                newX = WALK_MINX;
            }
            else
            {
                if (newX > WALK_MAXX)
                {
                    int necessaryEffect = newX - WALK_MAXX;
                    newX = WALK_MAXX;
                    m_roadOffset += necessaryEffect;
                }
                
            }
            m_zamparaManPos.X = newX;
            
        }

    }
}
