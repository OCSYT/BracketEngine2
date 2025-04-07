using System;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;

namespace Engine.Game
{
    public class UI
    {
        public Desktop UIDesktop;
        private Label FPSLabel;
        public void Start()
        {
            Panel MainPanel = new Panel();
            FPSLabel = new Label
            {
                Text = "FPS: 0"
            };
            MainPanel.Widgets.Add(FPSLabel);

            UIDesktop = new Desktop();
            UIDesktop.Root = MainPanel;

        }
        public void Render(GameTime GameTime)
        {
            FPSLabel.Text = "FPS: " + MathF.Round(1 / (float)GameTime.ElapsedGameTime.TotalSeconds);
            UIDesktop.Render();
        }
    }
}
