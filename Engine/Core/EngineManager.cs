using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using BulletSharp;
using System.Threading.Tasks;
using Engine.Core.ECS;
using Engine.Core.Physics;
using Engine.Core.Audio;
using System.Runtime.InteropServices;
using Engine.Core.Rendering;
using Engine.Game;
using Myra;
namespace Engine.Core
{
    public class EngineManager : Microsoft.Xna.Framework.Game
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

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

        public GraphicsDeviceManager Graphics;

        public float FixedTimeStep = 0.016f;
        private float Accumulator = 0;

        public SpriteBatch SpriteBatch { get; private set; }

        private int FrameCount = 0;
        private float ElapsedTime = 0f;
        public float CurrentFrameRate = 0f;
        public bool Debug = false;
        public Effect DefaultShader;
        public Texture2D WhiteTex;
        public Texture2D BlackTex;
        public UI UIManager = new UI();
        protected EngineManager()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Only one instance of EngineManager is allowed.");
            }
            _instance = this;

            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / short.MaxValue);
            Graphics = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = false
            };
            MyraEnvironment.Game = this;
            InactiveSleepTime = new TimeSpan(0);
            Window.AllowUserResizing = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Awake();
            if (Debug)
            {
                AllocConsole();
            }
            base.Initialize();
        }


        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            DefaultShader = Content.Load<Effect>("Rendering/Shaders/Default");
            Start();
            UIManager.Start();
            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Render(gameTime);
            UIManager.Render(gameTime);
            if (WhiteTex == null && BlackTex == null)
            {
                Texture2D WhiteTexture = new Texture2D(GraphicsDevice, 1, 1);
                Color[] WhiteData = new Color[1] { Color.White };
                WhiteTexture.SetData(WhiteData);
                WhiteTex = WhiteTexture;
                Texture2D BlackTexture = new Texture2D(GraphicsDevice, 1, 1);
                Color[] BlackData = new Color[1] { Color.Black };
                BlackTexture.SetData(BlackData);
                BlackTex = BlackTexture;
            }

            Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            SpriteBatch.Begin();
            DrawGUI(gameTime);
            ECSManager.Instance.CallDrawGUIOnComponents(gameTime);
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
            base.Draw(gameTime);
        }

        private DateTime LastTime = DateTime.Now;
        private float TotalTime = 0f;

        private readonly object PhyicsLock = new object();
        protected override void Update(GameTime gameTime)
        {

            var currentTime = DateTime.Now;
            var elapsedTime = (currentTime - LastTime).TotalSeconds;
            LastTime = currentTime;
            Accumulator += (float)elapsedTime;
            MainUpdate(gameTime);
            ECSManager.Instance.CallMainUpdateOnComponents(gameTime);

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

            SoundManager.Instance.Update();
            base.Update(gameTime);
        }
        protected override void UnloadContent()
        {
            OnDestroy();
            base.UnloadContent();
        }

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

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void MainUpdate(GameTime gameTime) { }
        public virtual void FixedUpdate(GameTime gameTime) { }

        public virtual void Render(GameTime gameTime) { }
        public virtual void DrawGUI(GameTime gameTime) { }
        public virtual void OnDestroy() { }
    }
}
