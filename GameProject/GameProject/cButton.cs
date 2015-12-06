using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameProject
{
    /// <summary>
    /// Class for representing buttons in menu
    /// </summary>
    class cButton
    {
        #region Fields

        // Button support
        Texture2D texture;
        Vector2 position;
        Rectangle rectangle;

        // Defining base color
        Color color = Color.White;

        // size of button
        public Vector2 size;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructing cButton for menu buttons
        /// </summary>
        /// <param name="texture, loading image for button"></param>
        /// <param name="graphics"></param>
        public cButton(Texture2D texture, GraphicsDevice graphics)
        {
            this.texture = texture;
            size = new Vector2(graphics.Viewport.Width / 6, graphics.Viewport.Height / 12);

        }

        #endregion

        #region Property

        public Vector2 Position
        {
            set { position = value; }
        }

        #endregion

        #region Public methods

        // support for mouse
        bool down;
        public bool isClicked;

        /// <summary>
        /// Update method for cButton
        /// </summary>
        /// <param name="mouse"></param>
        public void Update(MouseState mouse)
        {
            rectangle = new Rectangle((int)position.X,
                                      (int)position.Y,
                                      (int)size.X,
                                      (int)size.Y);

            Rectangle mouseRectangle = new Rectangle(mouse.X, mouse.Y, 1, 1);

            // clicking and glowing support
            if (mouseRectangle.Intersects(rectangle))
            {
                if (color.A == 255)
                {
                    down = false;
                }
                else if (color.A == 0)
                {
                    down = true;
                }

                if (down)
                {
                    color.A += 3;
                }
                else
                {
                    color.A -= 3;
                }

                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    isClicked = true;
                }
            }
            else if (color.A < 255)
            {
                color.A += 3;
                isClicked = false;
            }

        }

        /// <summary>
        /// Draw button
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, color);
        }

        #endregion
    }
}
