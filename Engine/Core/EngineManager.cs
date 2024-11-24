using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using BulletSharp;
using System.Threading.Tasks;
using Engine.UI;
using Engine.Core.ECS;
using Engine.Core.Physics;
using Engine.Core.Audio;

namespace Engine.Core
{
    public abstract class EngineManager : Game
    {
        private static EngineManager _instance;
        public static EngineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("EngineManager instance has not been initialized.");
                }
                return _instance;
            }
            protected set
            {
                if (_instance != null)
                {
                    throw new InvalidOperationException("EngineManager instance is already initialized.");
                }
                _instance = value;
            }
        }

        public GraphicsDeviceManager Graphics => Instance._graphics;
        private readonly GraphicsDeviceManager _graphics;

        public float FixedTimeStep = 0.016f;
        private float Accumulator = 0;

        public SpriteBatch SpriteBatch { get; private set; }

        private int FrameCount = 0;
        private float ElapsedTime = 0f;
        public float CurrentFrameRate = 0f;
        public UIControls UIControls;
        protected EngineManager()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Only one instance of EngineManager is allowed.");
            }
            _instance = this;

            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / short.MaxValue);
            _graphics = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = false
            };

            InactiveSleepTime = new TimeSpan(0);
            Window.AllowUserResizing = true;
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Start();
        }

        protected virtual void Start() { }

        protected override void Draw(GameTime gameTime)
        {
            Render(gameTime);
            Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            SpriteBatch.Begin();
            DrawGUI(gameTime);
            if (UIControls != null)
            {
                UIControls.Draw(gameTime);
            }
            SpriteBatch.End();
            Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            FrameCount++;
            ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (ElapsedTime >= 1.0f)
            {
                CurrentFrameRate = FrameCount;
                FrameCount = 0;
                ElapsedTime = 0f;
            }
        }

        protected virtual void Render(GameTime gameTime) { }
        protected virtual void DrawGUI(GameTime gameTime)
        {

        }


        protected override void Initialize()
        {
            UIControls = new UIControls(this);
            Components.Add(UIControls);
            base.Initialize();
        }

        private DateTime LastTime = DateTime.Now;
        private float TotalTime = 0f;

        private readonly object PhyicsLock = new object();
        protected override void Update(GameTime gameTime)
        {
            SoundManager.Instance.Update();
            var currentTime = DateTime.Now;
            var elapsedTime = (currentTime - LastTime).TotalSeconds;
            LastTime = currentTime;
            Accumulator += (float)elapsedTime;

            if (Accumulator >= FixedTimeStep)
            {
                lock (PhyicsLock)
                {
                    PhysicsManager.Instance.PhysicsUpdate(FixedTimeStep);
                }

                GameTime fixedUpdateTime = new GameTime(TimeSpan.FromSeconds(TotalTime), TimeSpan.FromSeconds(FixedTimeStep));
                FixedUpdate(fixedUpdateTime);
                ECSManager.Instance.CallFixedUpdateOnComponents(fixedUpdateTime);

                Accumulator -= FixedTimeStep;
                TotalTime += FixedTimeStep;
            }
            MainUpdate(gameTime);
            ECSManager.Instance.CallMainUpdateOnComponents(gameTime);
        }
        protected virtual void MainUpdate(GameTime gameTime) { }

        public void LockMouse()
        {
            if (IsActive)
            {
                SetMouseVisibility(false);
                CenterMouse();
            }
            else
            {
                SetMouseVisibility(true);
            }
        }

        public void UnlockMouse()
        {
            SetMouseVisibility(true);
        }

        public void SetMouseVisibility(bool isVisible)
        {
            IsMouseVisible = isVisible;
        }

        private void CenterMouse()
        {
            var viewport = Graphics.GraphicsDevice.Viewport;
            Mouse.SetPosition(viewport.Width / 2, viewport.Height / 2);
        }

        protected virtual void FixedUpdate(GameTime gameTime) { }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }
    }
}
