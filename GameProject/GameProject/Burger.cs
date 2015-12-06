using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameProject
{
    /// <summary>
    /// A class for a burger
    /// </summary>
    public class Burger
    {
        #region Fields

        // graphic and drawing info
        Texture2D sprite;
        Rectangle drawRectangle;

        // burger stats
        int health = 100;

        // shooting support
        bool canShoot = true;
        int elapsedCooldownTime = 0;

        // sound effect
        SoundEffect shootSound;

        // projectile type support
        ProjectileType currentProjectileType = ProjectileType.FrenchFries;

        // planting support
        int ketchupNumber = GameConstants.TOTAL_KETCHUP_NUMBER;
        int elapsedSpawnTime = 0;
        int elapsedCooldownKetcupTime = 0;
        bool canPlant = true;

        #endregion

        #region Constructors

        /// <summary>
        ///  Constructs a burger
        /// </summary>
        /// <param name="contentManager">the content manager for loading content</param>
        /// <param name="spriteName">the sprite name</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        /// <param name="shootSound">the sound the burger plays when shooting</param>
        public Burger(ContentManager contentManager, string spriteName, int x, int y,
            SoundEffect shootSound)
        {
            LoadContent(contentManager, spriteName, x, y);
            this.shootSound = shootSound;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collision rectangle for the burger
        /// </summary>
        public Rectangle CollisionRectangle
        {
            get { return drawRectangle; }
        }

        /// <summary>
        /// Gets and sets the burger health
        /// </summary>
        public int Health
        {
            get { return health; }
            set 
            {
                if (value < 0)
                {
                    health = 0;
                }
                else if (value > 100)
                {
                    health = 100;
                }
                else
                {
                    health = value;
                }
            }
        }

        #endregion

        #region Private properties

        /// <summary>
        /// Gets and sets the x location of the center of the burger
        /// </summary>
        private int X
        {
            get { return drawRectangle.Center.X; }
            set
            {
                drawRectangle.X = value - drawRectangle.Width / 2;

                // clamp to keep in range
                if (drawRectangle.X < 0)
                {
                    drawRectangle.X = 0;
                }
                else if (drawRectangle.X > GameConstants.WINDOW_WIDTH - drawRectangle.Width)
                {
                    drawRectangle.X = GameConstants.WINDOW_WIDTH - drawRectangle.Width;
                }
            }
        }

        /// <summary>
        /// Gets and sets the y location of the center of the burger
        /// </summary>
        private int Y
        {
            get { return drawRectangle.Center.Y; }
            set
            {
                drawRectangle.Y = value - drawRectangle.Height / 2;

                // clamp to keep in range
                if (drawRectangle.Y < 0)
                {
                    drawRectangle.Y = 0;
                }
                else if (drawRectangle.Y > GameConstants.WINDOW_HEIGHT - drawRectangle.Height)
                {
                    drawRectangle.Y = GameConstants.WINDOW_HEIGHT - drawRectangle.Height;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Updates the burger's location based on keyboard. Also fires 
        /// french fries as appropriate
        /// </summary>
        /// <param name="gameTime">game time</param>
        /// <param name="keyboard">the current state of the keyboard</param>
        public void Update(GameTime gameTime, KeyboardState keyboard, MouseState mouse)
        {
            // burger should only respond to input if it still has health
            if (health > 0)
            {
                // move burger using keyboard
                //drawRectangle.X = keyboard.X - drawRectangle.Width / 2;
                //drawRectangle.Y = keyboard.Y - drawRectangle.Height / 2;

                // move the fish based on the keyboard state
                if (keyboard.IsKeyDown(Keys.Right) ||
                    keyboard.IsKeyDown(Keys.D))
                {
                    X += GameConstants.BURGER_MOVEMENT_AMOUNT;
                }
                if (keyboard.IsKeyDown(Keys.Left) ||
                    keyboard.IsKeyDown(Keys.A))
                {
                    X -= GameConstants.BURGER_MOVEMENT_AMOUNT;
                }
                if (keyboard.IsKeyDown(Keys.Up) ||
                    keyboard.IsKeyDown(Keys.W))
                {
                    Y -= GameConstants.BURGER_MOVEMENT_AMOUNT;
                }
                if (keyboard.IsKeyDown(Keys.Down) ||
                    keyboard.IsKeyDown(Keys.S))
                {
                    Y += GameConstants.BURGER_MOVEMENT_AMOUNT;
                }

                // clamp burger in window
                if (drawRectangle.Left < 0)
                {
                    drawRectangle.X = 0;
                }
                else if (drawRectangle.Right > GameConstants.WINDOW_WIDTH)
                {
                    drawRectangle.X = GameConstants.WINDOW_WIDTH - drawRectangle.Width;
                }
                if (drawRectangle.Top < 0)
                {
                    drawRectangle.Y = 0;
                }
                else if (drawRectangle.Bottom > GameConstants.WINDOW_HEIGHT)
                {
                    drawRectangle.Y = GameConstants.WINDOW_HEIGHT - drawRectangle.Height;
                }

                // update shooting allowed
                // timer concept (for animations) introduced in Chapter 7
                if (!canShoot)
                {
                    elapsedCooldownTime += gameTime.ElapsedGameTime.Milliseconds;
                    if (elapsedCooldownTime >= GameConstants.BURGER_COOLDOWN_MILLISECONDS ||
                        mouse.LeftButton == ButtonState.Released)
                    {
                        canShoot = true;
                        elapsedCooldownTime = 0;
                    }
                }

                // updating plant allowed
                if (!canPlant)
                {
                    elapsedCooldownKetcupTime += gameTime.ElapsedGameTime.Milliseconds;
                    if (elapsedCooldownKetcupTime >= GameConstants.KETCHUP_COOLDOWN_MILLISECONDS ||
                        mouse.RightButton == ButtonState.Released)
                    {
                        canPlant = true;
                        elapsedCooldownKetcupTime = 0;
                    }
                }
                

                // switch projectile types
                if (keyboard.IsKeyDown(Keys.D1))
                {
                    currentProjectileType = ProjectileType.FrenchFries;
                }
                else if (keyboard.IsKeyDown(Keys.D2))
                {
                    // to implement code
                }
                

                // shoot if appropriate
                if (health > 0 &&
                    mouse.LeftButton == ButtonState.Pressed &&
                    canShoot)
                {
                    // play shoot sound
                    shootSound.Play();

                    canShoot = false;

                    Projectile projectile = new Projectile(currentProjectileType,
                        Game1.GetProjectileSprite(currentProjectileType),
                        drawRectangle.Center.X,
                        drawRectangle.Center.Y - GameConstants.FRENCH_FRIES_PROJECTILE_OFFSET,
                        -GameConstants.FRENCH_FRIES_PROJECTILE_SPEED);
                    Game1.AddProjectile(projectile);
                }

                // do not have any more ketchups, wait for new ones
                if (ketchupNumber == 0)
                {
                    elapsedSpawnTime += gameTime.ElapsedGameTime.Milliseconds;
                    if (elapsedSpawnTime >= GameConstants.KETCHUP_TIME)
                    {
                        elapsedSpawnTime = 0;
                        ketchupNumber = GameConstants.TOTAL_KETCHUP_NUMBER;
                    }
                }

                // plant a ketchup
                if (health > 0 && 
                    mouse.RightButton == ButtonState.Pressed &&
                    ketchupNumber > 0 &&
                    canPlant)
                {
                    // you have dropped the ketchup
                    ketchupNumber--;

                    // cannot plant a new ketchup right away 
                    canPlant = false;

                    Projectile projectile = new Projectile(ProjectileType.Ketchup,
                        Game1.GetProjectileSprite(ProjectileType.Ketchup),
                        drawRectangle.Center.X,
                        drawRectangle.Center.Y - GameConstants.KETCHUP_PROJECTILE_OFFSET,
                        0);
                    Game1.AddProjectile(projectile);

                }
            }
        }

        /// <summary>
        /// Draws the burger
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to use</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, drawRectangle, Color.White);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads the content for the burger
        /// </summary>
        /// <param name="contentManager">the content manager to use</param>
        /// <param name="spriteName">the name of the sprite for the burger</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        private void LoadContent(ContentManager contentManager, string spriteName,
            int x, int y)
        {
            // load content and set remainder of draw rectangle
            sprite = contentManager.Load<Texture2D>(spriteName);
            drawRectangle = new Rectangle(x - sprite.Width / 2,
                y - sprite.Height / 2, sprite.Width,
                sprite.Height);
        }

        #endregion
    }
}
