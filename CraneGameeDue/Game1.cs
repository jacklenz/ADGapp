#region UsingStatements

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace CraneGameeDue
{
    public class Game1 : Game
    {
        #region GeneralsDeclaration

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player;
        
        Camera camera;
        MouseState cms;
        KeyboardState pks, cks;

        #endregion

        #region TerrainDeclaration

        List<CModel> terrain;           //models list to compose each level's environment
        List<Vector3> wallPoints;       //list of points to check level2's labyrinth collisions

        #endregion

        #region StatesDefinitions

        enum GameState
        {
            MainMenu,
            Options,
            Playing,
            Lose,
            Win
        }
        GameState cgs;
        enum LevelState
        {
            Level1,
            Level2,
            Level3,
        }
        LevelState cls;

        #endregion

        #region ButtonsDeclarations

        CButton btnPlay, btnOpt, btnQuit;
        CButton btn1, btn2, btn3;
        CButton btnRetry, btnNxt;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Cst.screenWidth;
            graphics.PreferredBackBufferHeight = Cst.screenHeight;
            graphics.ApplyChanges();
            IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            cgs = GameState.MainMenu;
            cls = LevelState.Level1;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            #region GeneralsLoading

            //Load general game contents
            spriteBatch = new SpriteBatch(GraphicsDevice);
            player = new Player(Content.Load<Model>("MyGru"), Vector3.Zero, Vector3.Zero, new Vector3(1f), GraphicsDevice, .1f);
            camera = new ChaseCamera(new Vector3(0, 2, 10), Vector3.Zero, new Vector3(0, Cst.pi, 0), GraphicsDevice);

            #endregion

            #region TerrainLoading

            //Terrain components loading for each level. In every level the first component is the ball and the last is the goal.
            terrain = new List<CModel>();
            //LVL1 : simple logic
            if (cls == LevelState.Level1)
            {
                terrain.Add(new CModel(Content.Load<Model>("ball"), new Vector3(-7f, 0, 0), Vector3.Zero, new Vector3(1f), GraphicsDevice));
                terrain.Add(new CModel(Content.Load<Model>("Level1"), Vector3.Zero, Vector3.Zero, new Vector3(1f), GraphicsDevice));
                terrain.Add(new CModel(Content.Load<Model>("endflag"), new Vector3(20.4f, 0, 0), Vector3.Zero, new Vector3(1f), GraphicsDevice));
            }
            //LVL2 : labyrinth
            else if (cls == LevelState.Level2)
            {
                terrain.Add(new CModel(Content.Load<Model>("ball"), new Vector3(-7f, 0, 0), Vector3.Zero, new Vector3(1f), GraphicsDevice));
                terrain.Add(new CModel(Content.Load<Model>("Level2"), Vector3.Zero, Vector3.Zero, new Vector3(1f), GraphicsDevice));
                terrain.Add(new CModel(Content.Load<Model>("endflag"), new Vector3(0, 0, -7.5f), Vector3.Zero, new Vector3(1f), GraphicsDevice));

                #region WallPointsCreation

                wallPoints = new List<Vector3>();

                //Contorno
                for (float x = -24.3f; x < 17.9f; x += .1f)
                {
                    wallPoints.Add(new Vector3(x, 1f, -39f));
                    wallPoints.Add(new Vector3(x, 1f, 3f));
                }
                for (float z = -39f; z < 3.1f; z += .1f)
                {
                    wallPoints.Add(new Vector3(-24.3f, 1f, z));
                    wallPoints.Add(new Vector3(17.8f, 1f, z));
                }

                //Muri verticali
                for (float z = -3.8f; z < 3.1f; z += .1f)
                {
                    wallPoints.Add(new Vector3(-10.2f, 1f, z));
                    wallPoints.Add(new Vector3(10.7f, 1f, z));
                }
                for (float z = -10.8f; z < -4.1f; z += .1f)
                {
                    wallPoints.Add(new Vector3(-17.2f, 1f, z));
                }
                for (float z = -25f; z < -3.9f; z += .1f)
                {
                    wallPoints.Add(new Vector3(3.7f, 1f, z));
                }
                for (float z = -25f; z < -10.8f; z += .1f)
                {
                    wallPoints.Add(new Vector3(-3.3f, 1f, z));
                }
                for (float z = -32f; z < -10.9f; z += .1f)
                {
                    wallPoints.Add(new Vector3(10.8f, 1f, z));
                }
                for (float z = -32f; z < -24.9f; z += .1f)
                {
                    wallPoints.Add(new Vector3(-17.2f, 1f, z));
                }

                //Muri orizzontali
                for (float x = -10.2f; x < 3.8f; x += .1f)
                {
                    wallPoints.Add(new Vector3(x, 1f, -3.8f));
                }
                for (float x = -17.2f; x < 10.8f; x += .1f)
                {
                    wallPoints.Add(new Vector3(x, 1f, -10.9f));
                }
                for (float x = -24.3f; x < -17.3f; x += .1f)
                {
                    wallPoints.Add(new Vector3(x, 1f, -18f));
                }
                for (float x = -10.2f; x < -3.3f; x += .1f)
                {
                    wallPoints.Add(new Vector3(x, 1f, -25f));
                }
                for (float x = -17.2f; x < 10.9f; x += .1f)
                {
                    wallPoints.Add(new Vector3(x, 1f, -31.8f));
                }

                #endregion

            }
            //LVL3 : desert landscape with a gap to be passed to reach the goal
            else if (cls == LevelState.Level3)
            {
                terrain.Add(new CModel(Content.Load<Model>("ball"), new Vector3(-11f, 0, 2f), Vector3.Zero, new Vector3(1f), GraphicsDevice));
                //Here we create 2 instances of the level3's terrain, with a specified distance to generate a gap between them
                terrain.Add(new CModel(Content.Load<Model>("Level3"), new Vector3(-5f, 0, 0), Vector3.Zero, new Vector3(1f), GraphicsDevice));
                terrain.Add(new CModel(Content.Load<Model>("Level3"), new Vector3(17f, 0, 0), Vector3.Zero, new Vector3(1f), GraphicsDevice));
                terrain.Add(new CModel(Content.Load<Model>("bridge"), new Vector3(-10f, 0, -1.5f), Vector3.Zero, new Vector3(1f), GraphicsDevice));
                terrain.Add(new CModel(Content.Load<Model>("endflag"), new Vector3(22.4f, 0, 0), Vector3.Zero, new Vector3(1f), GraphicsDevice));
            }

            #endregion

            #region ButtonsInitialization

            btnPlay = new CButton(Content.Load<Texture2D>("ButtonPlay"), graphics.GraphicsDevice);
            btnPlay.setPosition(new Vector2(450,350));
            btnOpt = new CButton(Content.Load<Texture2D>("ButtonOpt"), graphics.GraphicsDevice);
            btnOpt.setPosition(new Vector2(450, 400));
            btnQuit = new CButton(Content.Load<Texture2D>("ButtonQuit"), graphics.GraphicsDevice);
            btnQuit.setPosition(new Vector2(450, 450));
            btn1 = new CButton(Content.Load<Texture2D>("Button1"), graphics.GraphicsDevice);
            btn1.setPosition(new Vector2(350, 190));
            btn2 = new CButton(Content.Load<Texture2D>("Button2"), graphics.GraphicsDevice);
            btn2.setPosition(new Vector2(480, 190));
            btn3 = new CButton(Content.Load<Texture2D>("Button3"), graphics.GraphicsDevice);
            btn3.setPosition(new Vector2(610, 190));
            btnNxt = new CButton(Content.Load<Texture2D>("ButtonNxt"), graphics.GraphicsDevice);
            btnNxt.setPosition(new Vector2(600, 250));
            btnRetry = new CButton(Content.Load<Texture2D>("ButtonRetry"), graphics.GraphicsDevice);
            btnRetry.setPosition(new Vector2(600, 300));

            #endregion
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            //Get the mouse state and the keyboard previous and current state
            cms = Mouse.GetState();
            pks = cks;
            cks = Keyboard.GetState();

            switch (cgs)
            {
                //Menus Logic
                #region ButtonsLogic

                case GameState.MainMenu:

                    //Play button
                    if (btnPlay.isClicked)
                    {
                        UnloadContent();
                        LoadContent();
                        cgs = GameState.Playing;
                    }
                    btnPlay.Update(cms);

                    //Options button
                    if (btnOpt.isClicked) cgs = GameState.Options;
                    btnOpt.Update(cms);

                    //Quit button
                    if (btnQuit.isClicked) this.Exit();
                    btnQuit.Update(cms);

                    break;

                case GameState.Options:

                    //Level1 button
                    if (btn1.isClicked)
                    {
                        cls = LevelState.Level1;
                        cgs = GameState.MainMenu;
                    }
                    btn1.Update(cms);

                    //Level2 button
                    if (btn2.isClicked)
                    {
                        cls = LevelState.Level2;
                        cgs = GameState.MainMenu;
                    }
                    btn2.Update(cms);

                    //Level3 button
                    if (btn3.isClicked)
                    {
                        cls = LevelState.Level3;
                        cgs = GameState.MainMenu;
                    }
                    btn3.Update(cms);

                    break;

                case GameState.Lose:

                    //Retry button
                    if (btnRetry.isClicked)
                    {
                        btnRetry.isClicked = false;
                        UnloadContent();
                        LoadContent();
                        cgs = GameState.Playing;
                    }
                    btnRetry.Update(cms);

                    //Quit button
                    if (btnQuit.isClicked) this.Exit();
                    btnQuit.Update(cms);
                    btnQuit.setPosition(new Vector2(600, 350));

                    break;

                case GameState.Win:

                    //Retry button
                    if (btnRetry.isClicked)
                    {
                        btnRetry.isClicked = false;
                        UnloadContent();
                        LoadContent();
                        cgs = GameState.Playing;
                    }
                    btnRetry.Update(cms);

                    //Quit button
                    if (btnQuit.isClicked) this.Exit();
                    btnQuit.Update(cms);
                    btnQuit.setPosition(new Vector2(600, 350));

                    //Nxt Lvl button
                    if (cls == LevelState.Level1)
                    {
                        if (btnNxt.isClicked)
                        {
                            cls = LevelState.Level2;
                            UnloadContent();
                            LoadContent();
                            cgs = GameState.Playing;
                        }
                        btnNxt.Update(cms);
                    }
                    else if (cls == LevelState.Level2)
                    {
                        if (btnNxt.isClicked)
                        {
                            cls = LevelState.Level3;
                            UnloadContent();
                            LoadContent();
                            cgs = GameState.Playing;
                        }
                        btnNxt.Update(cms);
                    }

                    break;

                #endregion

                //In-Game Logic
                case GameState.Playing:

                    #region LevelsSpecs

                    #region Level1

                    //Lose if the player's bounding box doesn't intersect's the ground's one
                    if (cls == LevelState.Level1)
                        if (!player.boundingBox.Intersects(terrain[1].boundingBox))
                            player.hasLost = true;

                    #endregion

                    #region Level2

                    //Same losing condition as before, but with a new case: we lose also if the player's bounding box intersect a wall point
                    if (cls == LevelState.Level2)
                    {
                        if (!player.boundingBox.Intersects(terrain[1].boundingBox))
                            player.hasLost = true;

                        if (wallPoints.Any(p => player.boundingBox.Contains(p) == ContainmentType.Contains))
                        {
                            player.isCollidingWithWall = true;
                        }

                        if(!wallPoints.Any(p => terrain.First().boundingBox.Contains(p) == ContainmentType.Contains || player.boundingBox.Contains(p) == ContainmentType.Contains) && terrain.First().boundingBox.Intersects(terrain.Last().boundingBox))
                            player.hasWon = true;
                    }

                    #endregion

                    #region Level3
                    if (cls == LevelState.Level3)
                    {
                        //We define a condition to know whether the player is (ENTIRELY) on the bridge or not (the bridge's bounding box must contain the min and max constraints of the player's boundingbox's base)
                        Vector3 maxBasePoint = new Vector3(player.boundingBox.Max.X,player.boundingBox.Min.Y,player.boundingBox.Max.Z);
                        bool playerIsOnBridge = (terrain[3].boundingBox.Contains(maxBasePoint) == ContainmentType.Contains) && (terrain[3].boundingBox.Contains(player.boundingBox.Min) == ContainmentType.Contains);  //player.boundingBox.Min is the minimum base point!!!
                        
                        //We lose if out of the ground, and also if not entirely on the bridge
                        if (!(player.boundingBox.Intersects(terrain[1].boundingBox) || player.boundingBox.Intersects(terrain[2].boundingBox) || playerIsOnBridge))
                            player.hasLost = true;

                        terrain[3].Update(gameTime); //bridge update

                        //Player bridge-catching logic (if the player bounding box is not intersecting the ball's one, but the bridge's, and the player is in "catch" mode, center the bridge on the hook of the player)
                        if (player.trysCatching)
                        {
                            if (!player.boundingBox.Intersects(terrain[0].boundingBox))
                            {
                                if (player.boundingBox.Intersects(terrain[3].boundingBox))
                                {
                                    float trasX = (float)((Math.Cos(player.Rotation.Y)));
                                    float trasZ = (float)((Math.Sin(player.Rotation.Y)));
                                    Vector2 diagXZ = new Vector2(player.modelTransforms[17].Translation.X, player.modelTransforms[17].Translation.Z);
                                    Vector3 tras = new Vector3(diagXZ.Length() * trasZ, player.modelTransforms[17].Translation.Y - terrain[3].modelTransforms[0].Translation.Y, diagXZ.Length() * trasX);
                                    terrain[3].Position = tras + player.Position;
                                }
                            }
                        }

                        //If the bridge is not caught by the player and it is not touching its bounding box,
                        if (!player.boundingBox.Intersects(terrain[3].boundingBox))
                            terrain[3].Position = new Vector3(terrain[3].Position.X,-.1f,terrain[3].Position.Z);
                    }

                    #endregion

                    #endregion

                    #region GeneralRules

                    //If player loses, go to the lose screen
                    if (player.hasLost)
                    {
                        btnNxt.isClicked = false;
                        btnRetry.isClicked = false;
                        cgs = GameState.Lose;
                    }
                    //If player wins, go to the win screen
                    else if (player.hasWon)
                    {
                        btnNxt.isClicked = false;
                        btnRetry.isClicked = false;
                        cgs = GameState.Win;
                    }
                    //If the ball hits the goal, player wins
                    if (cls != LevelState.Level2 && terrain.First().boundingBox.Intersects(terrain.Last().boundingBox))
                        player.hasWon = true;

                    terrain[0].Update(gameTime); //ball update
                    player.Update(gameTime);
                    updateCamera(gameTime);

                    //Ball catching logic (when player is in "catch" mode and its bounding box touches the ball's one, center the ball to the player's hook)
                    if (player.trysCatching)
                    {
                        if (player.boundingBox.Intersects(terrain[0].boundingBox))
                        {
                            float trasX = (float)((Math.Cos(player.Rotation.Y)));
                            float trasZ = (float)((Math.Sin(player.Rotation.Y)));
                            Vector2 diagXZ = new Vector2(player.modelTransforms[17].Translation.X, player.modelTransforms[17].Translation.Z);
                            Vector3 tras = new Vector3(diagXZ.Length() * trasZ, player.modelTransforms[17].Translation.Y - terrain[0].modelTransforms[0].Translation.Y, diagXZ.Length() * trasX);
                            terrain[0].Position = tras + player.Position;
                        }
                    }                    

                    #endregion

                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);            
            switch (cgs)
            {
                #region Drawing2D

                //Call the 2D drawing methods for the menus

                case GameState.MainMenu:
                    spriteBatch.Begin();
                    spriteBatch.Draw(Content.Load<Texture2D>("MainMenu"), new Rectangle(0,0,Cst.screenWidth,Cst.screenHeight), Color.White);
                    btnPlay.Draw(spriteBatch);
                    btnOpt.Draw(spriteBatch);
                    btnQuit.Draw(spriteBatch);
                    spriteBatch.End();
                    break;

                case GameState.Options:
                    spriteBatch.Begin();
                    spriteBatch.Draw(Content.Load<Texture2D>("OptMenu"), new Rectangle(0, 0, Cst.screenWidth, Cst.screenHeight), Color.White);
                    btn1.Draw(spriteBatch);
                    btn2.Draw(spriteBatch);
                    btn3.Draw(spriteBatch);
                    spriteBatch.End();
                    break;

                case GameState.Lose:
                    spriteBatch.Begin();
                    spriteBatch.Draw(Content.Load<Texture2D>("LoseScreen"), new Rectangle(0, 0, Cst.screenWidth, Cst.screenHeight), Color.White);
                    btnRetry.Draw(spriteBatch);
                    btnQuit.Draw(spriteBatch);
                    spriteBatch.End();
                    break;

                case GameState.Win:
                    spriteBatch.Begin();
                    spriteBatch.Draw(Content.Load<Texture2D>("WinScreen"), new Rectangle(0, 0, Cst.screenWidth, Cst.screenHeight), Color.White);
                    if (cls == LevelState.Level1 || cls == LevelState.Level2)
                        btnNxt.Draw(spriteBatch);                    
                    btnRetry.Draw(spriteBatch);
                    btnQuit.Draw(spriteBatch);
                    spriteBatch.End();
                    break;

                #endregion

                #region Drawing3D

                case GameState.Playing:

                    //Call the 3D drawing graphic options
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    
                    //Draw the player
                    player.Draw(camera.View, camera.Projection);

                    //Draw the terrain
                    foreach(CModel terrainElement in terrain)
                        terrainElement.Draw(camera.View, camera.Projection);
                    
                    break;

                    #endregion
            }

            base.Draw(gameTime);
        }

        void updateCamera(GameTime gameTime)
        {
            // Get the new keyboard and mouse state
            // Move the camera to the new model's position and orientation
            ((ChaseCamera)camera).Move(player.Position, player.Rotation);
            // Update the camera
            camera.Update();
        }
    }
}
