using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Resources;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

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

        AnimationPlayer m_animator;

        Effect m_blurEffect;
        Animation m_hand;
        Song m_backgroundMusic;

        SpriteFont m_font;

        Texture2D m_wall;
        Texture2D m_road;
        Texture2D m_clouds;
        Texture2D m_blocks;
        Texture2D m_grass;
        Texture2D m_hearts;

        Texture2D m_target1;
        Texture2D m_target2;
        Texture2D m_target3;
        Texture2D m_target4;
        Texture2D m_targetDisplay;

        Texture2D m_shoes;
        Vector2 m_shoePos;

        Texture2D m_shirt;
        Vector2 m_shirtPos;

        Texture2D m_pants;
        Vector2 m_pantsPos;

        Texture2D m_home1;
        Texture2D m_home2;

        Texture2D[] m_availableTreeKinds;
        int[] m_treePositions;
        int[] m_treeKindIndices;

        Texture2D[] m_availableBinKinds;
        Texture2D[] m_availableBinKindsHidingIn;
        int[] m_binPositions;
        int[] m_binKindIndices;

        int[] m_home1Coordinates;
        int[] m_home2Coordinates;

        public int RoadOffset = 0;

        BoobyWoman[] m_boobyWoman;
        ZamparaMan m_zamparaMan;

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
            if (focus == Keys.Escape)
            {
                Game.Exit();
            }
        }

        public override void HandleInput()
        {
            KeyboardState keyState = Keyboard.GetState();
            m_zamparaMan.HandleInput(); // for controlling the guy
            // if he's going down
            if (keyState.IsKeyDown(Keys.Down) && m_zamparaMan.State != ZamparaMan.ZamparaManState.GettingHit)
            {
                // if he's at the bottom
                if (m_zamparaMan.Position.Y >= WALK_MAXY - 5)
                {
                    for (int bi = 0; bi < m_binPositions.Length; bi++)
                    {
                        if (Math.Abs(m_binPositions[bi] - (m_zamparaMan.Position.X + RoadOffset)) < 100)
                        {
                            m_zamparaMan.HideInsideBin(bi);
                        }
                    }
                }
            }
        }

        public override void LoadContent()
        {
            m_backgroundMusic = Game.Content.Load<Song>("wftsw");
            MediaPlayer.Play(m_backgroundMusic);
            MediaPlayer.Volume = 0.1f;

            m_font = Game.Content.Load<SpriteFont>("Hud");

            Random rand = new Random();

            m_hand = new Animation(Game.Content.Load<Texture2D>("hand"), 0.5f, true, 3);

            m_road = Game.Content.Load<Texture2D>("road");
            
            m_blurEffect = Game.Content.Load<Effect>("radialblur");

            m_zamparaMan.Load();
            m_zamparaMan.IsEnabled = true;


            m_target1 = Game.Content.Load<Texture2D>("target1");
            m_target2 = Game.Content.Load<Texture2D>("target2");
            m_target3 = Game.Content.Load<Texture2D>("target3");
            m_target4 = Game.Content.Load<Texture2D>("target4");
            m_targetDisplay = m_target1;

            m_clouds = Game.Content.Load<Texture2D>("clouds");
            m_blocks = Game.Content.Load<Texture2D>("blocks");
            m_grass = Game.Content.Load<Texture2D>("grass");
            m_hearts = Game.Content.Load<Texture2D>("hearts");
            m_shoes = Game.Content.Load<Texture2D>("shoes");
            m_shirt = Game.Content.Load<Texture2D>("shirt");
            m_pants = Game.Content.Load<Texture2D>("pants");

            m_wall = Game.Content.Load<Texture2D>("wall");

            m_home1 = Game.Content.Load<Texture2D>("home1");
            m_home2 = Game.Content.Load<Texture2D>("home2");


            m_availableTreeKinds = new Texture2D[2];
            m_availableTreeKinds[0] = Game.Content.Load<Texture2D>("tree1");
            m_availableTreeKinds[1] = Game.Content.Load<Texture2D>("tree2");

            m_availableBinKinds = new Texture2D[1];
            m_availableBinKinds[0] = Game.Content.Load<Texture2D>("bin");

            m_availableBinKindsHidingIn = new Texture2D[1];
            m_availableBinKindsHidingIn[0] = Game.Content.Load<Texture2D>("bin-hidein");

            m_zamparaMan.Position = new Vector2(0, Game.Window.ClientBounds.Height - m_zamparaMan.Height);

            // LOAD TREES
            ResourceManager res = new ResourceManager("GameObjects.resx", Assembly.GetExecutingAssembly());
            m_treePositions = GameObjects.Trees.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_treeKindIndices = m_treePositions.Select(x => rand.Next(0, m_availableTreeKinds.Length)).ToArray();

            m_binPositions = GameObjects.Bins.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_binKindIndices = m_binPositions.Select(x => rand.Next(0, m_availableBinKinds.Length)).ToArray();

            m_shoePos = new Vector2(int.Parse(GameObjects.Shoes), 300);
            m_shirtPos = new Vector2(int.Parse(GameObjects.Shirt), 300);
            m_pantsPos = new Vector2(int.Parse(GameObjects.Pants), 300);

            m_home1Coordinates = GameObjects.Home1.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_home2Coordinates = GameObjects.Home2.Split(',').Select(x => int.Parse(x.Trim())).ToArray();

            int[] boobyCoordinates = GameObjects.BoobyWoman.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            m_boobyWoman = new BoobyWoman[boobyCoordinates.Length];
            for (int i = 0; i < boobyCoordinates.Length; i++)
            {
                m_boobyWoman[i] = new BoobyWoman(Game);
                m_boobyWoman[i].Load();
                m_boobyWoman[i].Position.X = boobyCoordinates[i];
                m_boobyWoman[i].Position.Y = (float)(WALK_MINY + (WALK_MAXY - WALK_MINY) * rand.NextDouble());
                m_boobyWoman[i].IsEnabled = false;
            }
            m_boobyWoman = m_boobyWoman.OrderBy(x => x.Position.Y).ToArray();
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

            DrawTile(batch, m_clouds, RoadOffset / 4, 0, .5f);
            

            DrawTile(batch, m_blocks, RoadOffset / 2, 100, .5f);

            DrawTile(batch, m_wall, (int)(RoadOffset / 1.1), 218, 1f);

            int homeLayerOffset = (int)(RoadOffset / 1.1);
            float homeLayerScale = 0.25f;

            //ROAD
            DrawTile(batch, m_road, RoadOffset, 300, 0.5f);

            // TREES
            for (int ti = 0; ti < m_treePositions.Length; ti++)
            {
                Texture2D tree = m_availableTreeKinds[m_treeKindIndices[ti]];
                int treePosition = m_treePositions[ti];
                batch.Draw(tree, new Rectangle(treePosition - RoadOffset, 140, tree.Width / 2, tree.Height / 2), Color.White);
            }

            //HOMES
            for (int hi = 0; hi < m_home1Coordinates.Length; hi++)
            {
                batch.Draw(m_home1, new Rectangle(m_home1Coordinates[hi] - homeLayerOffset, HOMESY, (int)(m_home1.Width * homeLayerScale), (int)(m_home1.Height * homeLayerScale)), Color.White);
            }
            for (int hi = 0; hi < m_home2Coordinates.Length; hi++)
            {
                batch.Draw(m_home2, new Rectangle(m_home2Coordinates[hi] - homeLayerOffset, HOMESY, (int)(m_home2.Width * homeLayerScale), (int)(m_home2.Height * homeLayerScale)), Color.White);
            }

            batch.Draw(m_shoes, new Rectangle((int)(m_shoePos.X - RoadOffset), (int)m_shoePos.Y, m_shoes.Width, m_shoes.Height), Color.White);
            batch.Draw(m_shirt, new Rectangle((int)(m_shirtPos.X - RoadOffset), (int)m_shirtPos.Y, m_shoes.Width, m_shoes.Height), Color.White);
            batch.Draw(m_pants, new Rectangle((int)(m_pantsPos.X - RoadOffset), (int)m_pantsPos.Y, m_pants.Width, m_pants.Height), Color.White);


            // ZAMPARA
            m_zamparaMan.Draw(_time, batch, 0);

            //BOOBY WOMEN
            foreach (BoobyWoman b in m_boobyWoman)
            {
                b.Draw(_time, batch, -RoadOffset);
            }

            // BINS
            for (int bi = 0; bi < m_binPositions.Length; bi++)
            {
                Texture2D bin;
                if (m_zamparaMan.State == ZamparaMan.ZamparaManState.HidingInsideBin && m_zamparaMan.HidingInsideBinIndex == bi)
                {
                    bin = m_availableBinKindsHidingIn[m_binKindIndices[bi]];
                }
                else
                {
                    bin = m_availableBinKinds[m_binKindIndices[bi]];
                }
                int binX = m_binPositions[bi] - RoadOffset;
                int binY = 430;
                batch.Draw(bin, new Rectangle(binX, binY, bin.Width / 4, bin.Height / 4), Color.White);
                if (m_zamparaMan.State != ZamparaMan.ZamparaManState.HidingInsideBin)
                {
                    if (Math.Abs(binX - m_zamparaMan.Position.X) < Game.Window.ClientBounds.Width / 4)
                    {
                        m_animator.PlayAnimation(m_hand);
                        m_animator.Draw(_time, batch, new Vector2(binX, binY), SpriteEffects.None, 0.5f, false);
                    }
                }
            }

            DrawTile(batch, m_grass, (int)(RoadOffset * 1.2), 343, 0.4f);

            batch.Draw(m_hearts, new Vector2(25, 25), new Rectangle(0, 0, (int)(m_hearts.Width * m_zamparaMan.Health * 0.01f), m_hearts.Height), Color.White);
            //batch.DrawString(m_font, "Health:" + m_zamparaMan.Health.ToString("#"), new Vector2(5, 5), Color.Black);

            batch.Draw(m_targetDisplay, new Vector2(450, 10), Color.White);

            batch.End();
        }

        public override void Update(GameTime _time)
        {
            foreach (BoobyWoman b in m_boobyWoman)
            {
                b.Update(_time);
            }
            

            foreach (BoobyWoman b in m_boobyWoman)
            {
                if (Math.Abs(b.Position.X - m_zamparaMan.Position.X - RoadOffset) < Game.Window.ClientBounds.Width * 0.7)
                {
                    b.IsEnabled = true;
                }
                else
                {
                    b.IsEnabled = b.IsEnabled;
                }
            }

            m_zamparaMan.Update(_time);

            if (m_zamparaMan.Health <= 0)
            {
                Game.SwitchToGameOver();
                return;
            }

            CheckForCollisionsAndAct();
        }

        public void CheckForCollisionsAndAct()
        {
            if (m_zamparaMan.State != ZamparaMan.ZamparaManState.HidingInsideBin)
            {
                bool hits = false;
                List<BoobyWoman> hittingWomen = new List<BoobyWoman>();
                foreach (BoobyWoman b in m_boobyWoman)
                {
                    Rectangle rectBoobywoman = new Rectangle((int)(b.DrawX), (int)b.DrawY, b.Width, b.Height);
                    Rectangle rectZampara = new Rectangle((int)(m_zamparaMan.DrawX), (int)m_zamparaMan.DrawY, m_zamparaMan.Width, m_zamparaMan.Height);

                    rectBoobywoman.Inflate(-20, -20);
                    rectZampara.Inflate(-30, -30);

                    if (rectBoobywoman.Intersects(rectZampara))
                    {
                        hittingWomen.Add(b);
                        hits = true;
                    }
                }

                if (m_zamparaMan.Position.X + RoadOffset >= m_shoePos.X)
                {
                    m_zamparaMan.GrantShoes();
                    m_shoePos.X = -500;
                    m_targetDisplay = m_target2;
                }

                if (m_zamparaMan.Position.X + RoadOffset >= m_shirtPos.X)
                {
                    m_zamparaMan.GrantShirt();
                    m_shirtPos.X = -500;
                    m_targetDisplay = m_target3;
                }

                if (m_zamparaMan.Position.X + RoadOffset >= m_pantsPos.X)
                {
                    m_pantsPos.X = -500;
                    m_targetDisplay = m_target4;
                    Game.SwitchToWinGame();
                }


                if (hits)
                {
                    foreach (var b in hittingWomen)
                    {
                        b.State = BoobyWoman.BoobyWomanState.Hitting;
                    }
                    m_zamparaMan.State = ZamparaMan.ZamparaManState.GettingHit;
                }
                else
                {
                    foreach (var b in m_boobyWoman)
                    {
                        b.State = BoobyWoman.BoobyWomanState.Walking;
                    }
                    m_zamparaMan.State = ZamparaMan.ZamparaManState.Walking;
                }
            }
            else
            {
                foreach (var b in m_boobyWoman)
                {
                    b.State = BoobyWoman.BoobyWomanState.Walking;
                }
            }
        }
    }
}
