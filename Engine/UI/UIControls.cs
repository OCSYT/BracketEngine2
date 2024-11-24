using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame;
using MonoGame.UI.Forms;
namespace Engine.UI
{
    public class UIControls : ControlManager
    {
        public UIControls(Game game) : base(game)
        {

        }
        public override void InitializeComponent()
        {

        }

        private Label FPSText = null;
        public void DisplayFramerate(float FPS)
        {
            if (FPSText == null)
            {
                FPSText = new Label()
                {
                    Text = "FPS: " + FPS,
                    Size = new Vector2(200, 200),
                    TextColor = Color.Red,
                };
                Controls.Add(FPSText);
            }
            else
            {
                FPSText.Text = "FPS: " + FPS;
            }
        }
    }
}
