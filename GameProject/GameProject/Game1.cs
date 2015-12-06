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
using System.IO;

namespace GameProject
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        enum GameState
        {
            MainMenu,
            Result,
            Playing,
            Paused
        }

        // first game state is main menu
        GameState CurrentGameState = GameState.MainMenu;

        // adding buttons in main menu
        cButton playButton;
        cButton restartButton;
        cButton quitButton;

        // game objects. Using inheritance would make this
        // easier, but inheritance isn't a GDD 1200 topic
        Burger burger;
        static List<Projectile> projectiles = new List<Projectile>();
        List<Explosion> explosions = new List<Explosion>();
        List<Cabbage> cabbages = new List<Cabbage>();

        // projectile and explosion sprites. Saved so they don't have to
        // be loaded every time projectiles or explosions are created
        static Texture2D frenchFriesSprite;
        static Texture2D explosionSpriteStrip;
        static Texture2D cabbageProjectileSprite;
        static Texture2D ketchupProjectileSprite;

        // scoring support
        int score = 0;
        string scoreString = GameConstants.SCORE_PREFIX + 0;
        HighscoreData data;
        public string HighscoresFilename = "highscore.dat";
        string playerName;
        string scoreboard;
        string cmdString = "Enter your name and press Enter";
        string messageString = "";

        // health support
        string healthString = GameConstants.HEALTH_PREFIX + 
            GameConstants.BURGER_INITIAL_HEALTH;
        bool burgerDead = false;

        // text display support
        SpriteFont font;

        // sound effects
        SoundEffect burgerDamage;
        SoundEffect burgerDeath;
        SoundEffect burgerShot;
        SoundEffect explosion;
        SoundEffect cabbageBounce;
        SoundEffect cabbageShot;

        // pause support
        private static bool paused = false;
        private bool pauseKeyDown = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution
            graphics.PreferredBackBufferWidth = GameConstants.WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = GameConstants.WINDOW_HEIGHT;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            RandomNumberGenerator.Initialize();

            // Get the path of the save game
            string fullpath = "highscores.dat";

            // Check to see if the save exists
            if (!File.Exists(fullpath))
            {
                //If the file doesn't exist, make a fake one...
                // Create the data to save
                data = new HighscoreData(GameConstants.MAX_NUMBER_OF_HIGHSCORES);
                data.PlayerNames[0] = "tester";
                data.Scores[0] = 100;

                HighscoreData.SaveHighScores(data, fullpath);

            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load audio content
            burgerDamage = Content.Load<SoundEffect>(@"Sounds/BurgerDamage");
            burgerDeath = Content.Load<SoundEffect>(@"Sounds/BurgerDeath");
            burgerShot = Content.Load<SoundEffect>(@"Sounds/BurgerShot");
            explosion = Content.Load<SoundEffect>(@"Sounds/Explosion");
            cabbageBounce = Content.Load<SoundEffect>(@"Sounds/CabbageBounce");
            cabbageShot = Content.Load<SoundEffect>(@"Sounds/CabbageShot");

            // load sprite font
            font = Content.Load<SpriteFont>("Arial20"); 

            // load projectile and explosion sprites
            frenchFriesSprite = Content.Load<Texture2D>("frenchfries");
            cabbageProjectileSprite = Content.Load<Texture2D>("cabbageProjectile");
            explosionSpriteStrip = Content.Load<Texture2D>("explosion");
            ketchupProjectileSprite = Content.Load<Texture2D>("Ketchup");

            // add initial game objects
            burger = new Burger(Content, "burger",
                graphics.PreferredBackBufferWidth / 2,
                graphics.PreferredBackBufferHeight - graphics.PreferredBackBufferHeight / 8,
                burgerShot);

            for (int i = 0; i < GameConstants.MAX_ENEMIES; i++)
            {
                SpawnEnemy();
            }

            // set initial health and score strings
            healthString = GameConstants.HEALTH_PREFIX + burger.Health;
            scoreString = GameConstants.SCORE_PREFIX + score;

            // create play button and set position
            playButton = new cButton(Content.Load<Texture2D>("playbutton"), GraphicsDevice);
            playButton.Position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 8);

            // create quit button ans set position
            quitButton = new cButton(Content.Load<Texture2D>("quitbutton"), GraphicsDevice);
            quitButton.Position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 8 + GraphicsDevice.Viewport.Height / 4);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // get current keyboard and mouse state and update burger
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    if (playButton.isClicked)
                    {
                        CurrentGameState = GameState.Playing;
                    }
                    playButton.Update(mouse);

                    if (quitButton.isClicked)
                    {
                        this.Exit();
                    }
                    quitButton.Update(mouse);
                    break;

                case GameState.Playing:
                    burger.Update(gameTime, keyboard, mouse);

                    // check user hit pause button
                    checkPauseKey(keyboard);

                    if (paused)
                    {
                        CurrentGameState = GameState.Paused;
                    }

                    // update other game objects
                    foreach (Cabbage cabbage in cabbages)
                    {
                        cabbage.Update(gameTime);
                    }
                    foreach (Projectile projectile in projectiles)
                    {
                        projectile.Update(gameTime);
                    }
                    foreach (Explosion explosion in explosions)
                    {
                        explosion.Update(gameTime);
                    }

                    // check and resolve collisions between cabbages
                    for (int i = 0; i < cabbages.Count; i++)
                    {
                        for (int j = i + 1; j < cabbages.Count; j++)
                        {
                            if (cabbages[i].Active &&
                                cabbages[j].Active)
                            {
                                CollisionResolutionInfo cri = CollisionUtils.CheckCollision(
                                    gameTime.ElapsedGameTime.Milliseconds,
                                    GameConstants.WINDOW_WIDTH,
                                    GameConstants.WINDOW_HEIGHT,
                                    cabbages[i].Velocity,
                                    cabbages[i].DrawRectangle,
                                    cabbages[j].Velocity,
                                    cabbages[j].DrawRectangle);
                                if (cri != null)
                                {
                                    // play bounce sound
                                    cabbageBounce.Play();

                                    // resolve collision
                                    if (cri.FirstOutOfBounds)
                                    {
                                        cabbages[i].Active = false;
                                    }
                                    else
                                    {
                                        cabbages[i].Velocity = cri.FirstVelocity;
                                        cabbages[i].DrawRectangle = cri.FirstDrawRectangle;
                                    }
                                    if (cri.SecondOutOfBounds)
                                    {
                                        cabbages[j].Active = false;
                                    }
                                    else
                                    {
                                        cabbages[j].Velocity = cri.SecondVelocity;
                                        cabbages[j].DrawRectangle = cri.SecondDrawRectangle;
                                    }
                                }
                            }
                        }
                    }

                    // check and resolve collisions between burger and cabbages
                    foreach (Cabbage cabbage in cabbages)
                    {
                        if (cabbage.Active &&
                            burger.CollisionRectangle.Intersects(cabbage.CollisionRectangle))
                        {
                            burger.Health -= GameConstants.CABBAGE_DAMAGE;
                            cabbage.Active = false;
                            explosions.Add(new Explosion(explosionSpriteStrip,
                                cabbage.Location.X,
                                cabbage.Location.Y,
                                explosion));

                            // play sound when burger take damage
                            burgerDamage.Play();

                            // check burger health
                            CheckBurgerKill();

                            // update health string
                            healthString = GameConstants.HEALTH_PREFIX + burger.Health;
                        }
                    }

                    // check and resolve collisions between burger and projectiles
                    foreach (Projectile projectile in projectiles)
                    {
                        if (projectile.Type == ProjectileType.Cabbage &&
                            projectile.Active &&
                            burger.CollisionRectangle.Intersects(projectile.CollisionRectangle))
                        {
                            projectile.Active = false;
                            burger.Health -= GameConstants.RED_CABBAGE_PROJECTILE_DAMAGE;

                            // play sound when burger take damage
                            burgerDamage.Play();

                            // check burger health
                            CheckBurgerKill();

                            // update health string
                            healthString = GameConstants.HEALTH_PREFIX + burger.Health;
                        }
                    }

                    // check and resolve collisions between teddy bears and projectiles
                    foreach (Cabbage cabbage in cabbages)
                    {
                        foreach (Projectile projectile in projectiles)
                        {
                            if ((projectile.Type == ProjectileType.FrenchFries ||
                                 projectile.Type == ProjectileType.Ketchup) &&
                                cabbage.Active &&
                                projectile.Active &&
                                cabbage.CollisionRectangle.Intersects(projectile.CollisionRectangle))
                            {
                                cabbage.Active = false;
                                projectile.Active = false;
                                explosions.Add(new Explosion(explosionSpriteStrip,
                                    cabbage.Location.X, cabbage.Location.Y, explosion));

                                // add to score
                                score += GameConstants.CABBAGE_POINTS;

                                // set score string
                                scoreString = GameConstants.SCORE_PREFIX + score;
                            }
                        }
                    }

                    // clean out inactive teddy bears and add new ones as necessary
                    for (int i = cabbages.Count - 1; i >= 0; i--)
                    {
                        if (!cabbages[i].Active)
                        {
                            cabbages.RemoveAt(i);
                        }
                    }
                    while (cabbages.Count < GameConstants.MAX_ENEMIES)
                    {
                        SpawnEnemy();
                    }

                    // clean out inactive projectiles
                    for (int i = projectiles.Count - 1; i >= 0; i--)
                    {
                        if (!projectiles[i].Active)
                        {
                            projectiles.RemoveAt(i);
                        }
                    }

                    // clean out finished explosions
                    for (int i = explosions.Count - 1; i >= 0; i--)
                    {
                        if (explosions[i].Finished)
                        {
                            explosions.RemoveAt(i);
                        }
                    }
                    break;

                case GameState.Paused:
                    // check user hit pause button
                    checkPauseKey(keyboard);
                    if (!paused)
                    {
                        CurrentGameState = GameState.Playing;
                    }
                    break;

                case GameState.Result:
                    SaveHighScore();
                    if (quitButton.isClicked)
                    {
                        this.Exit();
                    }
                    quitButton.Update(mouse);

                    CurrentGameState = GameState.MainMenu;
                    break;
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.BurlyWood);

            spriteBatch.Begin();

            switch (CurrentGameState)
            {
                case GameState.MainMenu:
                    // set mouse visible during main menu and results
                    IsMouseVisible = true;

                    // draw a play button in main menu
                    playButton.Draw(spriteBatch);
                    quitButton.Draw(spriteBatch);

                    // draw score and health
                    spriteBatch.DrawString(font, healthString, GameConstants.HEALTH_LOCATION, Color.White);
                    spriteBatch.DrawString(font, scoreString, GameConstants.SCORE_LOCATION, Color.White);
                    break;

                case GameState.Playing:
                    // set mouse visibility to false during playing the game
                    IsMouseVisible = false;

                    // draw game objects
                    burger.Draw(spriteBatch);
                    foreach (Cabbage cabbage in cabbages)
                    {
                        cabbage.Draw(spriteBatch);
                    }
                    foreach (Projectile projectile in projectiles)
                    {
                        projectile.Draw(spriteBatch);
                    }
                    foreach (Explosion explosion in explosions)
                    {
                        explosion.Draw(spriteBatch);
                    }

                    // draw score and health
                    spriteBatch.DrawString(font, healthString, GameConstants.HEALTH_LOCATION, Color.White);
                    spriteBatch.DrawString(font, scoreString, GameConstants.SCORE_LOCATION, Color.White);
                    break;

                case GameState.Result:
                    // set mouse visible during main menu and results
                    IsMouseVisible = true;

                    quitButton.Draw(spriteBatch);
                    spriteBatch.DrawString(font, scoreString, GameConstants.SCORE_LOCATION, Color.White);
                    break;

            }
            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Public methods

        /// <summary>
        /// Gets the projectile sprite for the given projectile type
        /// </summary>
        /// <param name="type">the projectile type</param>
        /// <returns>the projectile sprite for the type</returns>
        public static Texture2D GetProjectileSprite(ProjectileType type)
        {
            // replace with code to return correct projectile sprite based on projectile type
            if (type == ProjectileType.FrenchFries)
            {
                return frenchFriesSprite;
            }
            else if (type == ProjectileType.Cabbage)
            {
                return cabbageProjectileSprite;
            }
            else if (type == ProjectileType.Ketchup)
            {
                return ketchupProjectileSprite;
            }
            else
            {
                return frenchFriesSprite;
            }
        }

        /// <summary>
        /// Adds the given projectile to the game
        /// </summary>
        /// <param name="projectile">the projectile to add</param>
        public static void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }

        /// <summary>
        /// Iterate through data if highscore is called and make the string to be saved
        /// </summary>
        /// <returns></returns>
        public string makeHighScoreString()
        {
            // create the data to save
            HighscoreData data2 = HighscoreData.LoadHighScores(HighscoresFilename);

            // create scoreBoardString
            string scoreBoardString = "Highscores:\n\n";

            for (int i = 0; i < GameConstants.MAX_NUMBER_OF_HIGHSCORES; i++) 
            {
                scoreBoardString = scoreBoardString + data2.PlayerNames[i] + "-" + data2.Scores[i] + "\n";
            }
            return scoreBoardString;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Spawns a new enemy at a random location
        /// </summary>
        private void SpawnEnemy()
        {
            // generate random location
            int x = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE,
                graphics.PreferredBackBufferWidth - 2 * GameConstants.SPAWN_BORDER_SIZE);
            int y = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE,
                graphics.PreferredBackBufferHeight - 2 * GameConstants.SPAWN_BORDER_SIZE);

            // generate random velocity
            float speed = GameConstants.MIN_CABBAGE_SPEED +
                RandomNumberGenerator.NextFloat(GameConstants.CABBAGE_SPEED_RANGE);
            float angle = RandomNumberGenerator.NextFloat(2 * (float)Math.PI);
            Vector2 velocity = new Vector2(
                (float)(speed * Math.Cos(angle)), (float)(speed * Math.Sin(angle)));

            // create new bear
            Cabbage newCabbage = new Cabbage(Content, "cabbage", x, y, velocity,
                cabbageBounce, cabbageShot);

            // make sure we don't spawn into a collision
            List<Rectangle> collisionRectangles = GetCollisionRectangles();
            while (!CollisionUtils.IsCollisionFree(newCabbage.CollisionRectangle,
                collisionRectangles))
            {
                newCabbage.X = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE,
                    graphics.PreferredBackBufferWidth - 2 * GameConstants.SPAWN_BORDER_SIZE);
                newCabbage.Y = GetRandomLocation(GameConstants.SPAWN_BORDER_SIZE,
                    graphics.PreferredBackBufferHeight - 2 * GameConstants.SPAWN_BORDER_SIZE);
            }

            // add new bear to list
            cabbages.Add(newCabbage);
        }

        /// <summary>
        /// Gets a random location using the given min and range
        /// 
        /// Example: For a random location between 100 and 700,
        /// pass in 100 for min and 600 for range
        /// </summary>
        /// <param name="min">the minimum</param>
        /// <param name="range">the range</param>
        /// <returns>the random location</returns>
        private int GetRandomLocation(int min, int range)
        {
            return min + RandomNumberGenerator.Next(range);
        }

        /// <summary>
        /// Gets a list of collision rectangles for all the objects in the game world
        /// </summary>
        /// <returns>the list of collision rectangles</returns>
        private List<Rectangle> GetCollisionRectangles()
        {
            List<Rectangle> collisionRectangles = new List<Rectangle>();
            collisionRectangles.Add(burger.CollisionRectangle);
            foreach (Cabbage bear in cabbages)
            {
                collisionRectangles.Add(bear.CollisionRectangle);
            }
            foreach (Projectile projectile in projectiles)
            {
                collisionRectangles.Add(projectile.CollisionRectangle);
            }
            foreach (Explosion explosion in explosions)
            {
                collisionRectangles.Add(explosion.CollisionRectangle);
            }
            return collisionRectangles;
        }

        /// <summary>
        /// Checks to see if the burger has just been killed
        /// </summary>
        private void CheckBurgerKill()
        {
            // if burger is dead play appropriate sound
            if (burger.Health == 0 &&
                !burgerDead)
            {
                burgerDead = true;
                burgerDeath.Play();

                CurrentGameState = GameState.Result;
            }

        }

        /// <summary>
        /// Function to begin pause.
        /// </summary>
        private void BeginPause()
        {
            paused = true;
        }

        /// <summary>
        /// Function to end the pause.
        /// </summary>
        private void EndPause()
        {
            paused = false;
        }

        /// <summary>
        /// Check does user had hit the pause button
        /// </summary>
        /// <param name="keyboard"></param>
        private void checkPauseKey(KeyboardState keyboard)
        {
            bool pauseKeyDownThisFrame = (keyboard.IsKeyDown(Keys.P));
            // If key was not down before, but is down now, we toggle the
            // pause setting
            if (!pauseKeyDown && pauseKeyDownThisFrame)
            {
                if (!paused)
                    BeginPause();
                else
                    EndPause();
            }
            pauseKeyDown = pauseKeyDownThisFrame;
        }

        /// <summary>
        /// Save player highscore when game ends.
        /// </summary>
        private void SaveHighScore()
        {
            // create the data to saved
            HighscoreData data = HighscoreData.LoadHighScores(HighscoresFilename);
            int scoreIndex = -1;
            for (int i = GameConstants.MAX_NUMBER_OF_HIGHSCORES - 1; i > -1; i--)
            {
                if (score >= data.Scores[i])
                {
                    scoreIndex = i;
                }
            }

            if (scoreIndex > -1)
            {
                // new high score found ... do swaps
                for (int i = GameConstants.MAX_NUMBER_OF_HIGHSCORES - 1; i > scoreIndex; i--)
                {
                    data.PlayerNames[i] = data.PlayerNames[i - 1];
                    data.Scores[i] = data.Scores[i - 1];
                }

                // retrieve User Name Here
                data.PlayerNames[scoreIndex] = playerName;
                // retrieve score here
                data.Scores[scoreIndex] = score; 

                HighscoreData.SaveHighScores(data, HighscoresFilename);
            }
        }

        #endregion
    }
}
