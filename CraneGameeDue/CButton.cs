using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CraneGameeDue
{
    //This class defines the button structure used for any button in the menu

    public class CButton
    {
        Texture2D texture;
        Vector2 position;
        Rectangle rectangle;
        Color color = new Color(255,255,255,255);
        public Vector2 size;
        bool down;
        public bool isClicked;

        public CButton(Texture2D newTexture, GraphicsDevice graphics)
        {
            texture = newTexture;
            //to screen size 800x600 adjustment
            size = new Vector2(graphics.Viewport.Width/8,graphics.Viewport.Height/30);
            isClicked = false;
        }

        //The update method takes as input the current mouse position
        public void Update(MouseState mouse)
        {
            rectangle = new Rectangle((int)position.X,(int)position.Y,(int)size.X,(int)size.Y);
            Rectangle mouseRectangle = new Rectangle(mouse.X,mouse.Y,1,1);

            //if the mouse is on the button, generate the fading effect, and if its left button clicked, set the isClicked parameter to true
            if (mouseRectangle.Intersects(rectangle))
            {
                if (color.A == 255) down = false;
                if (color.A == 0) down = true;
                if (down) color.A += 3; else color.A -= 3;
                if (mouse.LeftButton == ButtonState.Pressed) isClicked = true;
            }
            else if (color.A < 255)
            {
                color.A += 3;
                isClicked = false;
            }
        }

        public void setPosition(Vector2 newPosition)
        {
            position = newPosition;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, color);
        }
    }
}
