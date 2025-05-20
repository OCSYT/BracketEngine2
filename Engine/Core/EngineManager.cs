using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Engine.Core.EC;
using Engine.Core.Physics;
using Engine.Core.Audio;
using System.Runtime.InteropServices;
using Engine.Core.Rendering;
using Engine.Game;
using Myra;
using System.Collections.Generic;
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
        public RenderTarget2D RenderTarget { get; set; }
        private RenderTarget2D BackBuffer { get; set; }
        public Effect DefaultShader;
        public Texture2D WhiteTex;
        public Texture2D BlackTex;
        public Texture2D NormalTex;
        public TextureCube CubeTex;
        public UI UIManager = new UI();
        public float RenderScale = 1.0f;
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


        private int PrevWidth;
        private int PrevHeight;
        protected override void LoadContent()
        {
            Start();
            UIManager.Start();
            PrevWidth = GraphicsDevice.Viewport.Width;
            PrevHeight = GraphicsDevice.Viewport.Height;
            if (RenderTarget == null)
            {
                RenderTarget = new RenderTarget2D(GraphicsDevice, (int)(PrevWidth * RenderScale), (int)(PrevHeight * RenderScale), false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
                BackBuffer = new RenderTarget2D(GraphicsDevice, (int)(PrevWidth * RenderScale), (int)(PrevHeight * RenderScale), false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            }
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            DefaultShader = Content.Load<Effect>("Rendering/Shaders/Default");
            base.LoadContent();
        }

        protected override void Draw(GameTime GameTime)
        {
            if ((PrevWidth != GraphicsDevice.Viewport.Width) || (PrevHeight != GraphicsDevice.Viewport.Height))
            {
                PrevWidth = GraphicsDevice.Viewport.Width;
                PrevHeight = GraphicsDevice.Viewport.Height;
                RenderTarget = new RenderTarget2D(GraphicsDevice, (int)(PrevWidth * RenderScale), (int)(PrevHeight * RenderScale), false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
                BackBuffer = new RenderTarget2D(GraphicsDevice, (int)(PrevWidth * RenderScale), (int)(PrevHeight * RenderScale), false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24);
            }

            Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            GraphicsDevice.SetRenderTarget(RenderTarget);
            Render(GameTime);
            GraphicsDevice.SetRenderTarget(null);

            RenderTarget2D CurrentSource = RenderTarget;

            foreach (KeyValuePair<int, Effect> PostFX in PostFxManager.Instance.PostFxList)
            {
                foreach (KeyValuePair<string, object> Parameter in PostFxManager.Instance.PostFXParameterList[PostFX.Key])
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(Parameter.Key) && Parameter.Value != null)
                        {
                            var EffectParameter = PostFX.Value.Parameters[Parameter.Key];
                            if (EffectParameter != null)
                            {
                                Type ParamType = Parameter.Value.GetType();
                                if (ParamType == typeof(float))
                                    EffectParameter.SetValue((float)Parameter.Value);
                                else if (ParamType == typeof(int))
                                    EffectParameter.SetValue((int)Parameter.Value);
                                else if (ParamType == typeof(bool))
                                    EffectParameter.SetValue((bool)Parameter.Value);
                                else if (ParamType == typeof(Quaternion))
                                    EffectParameter.SetValue((Quaternion)Parameter.Value);
                                else if (ParamType == typeof(Vector4))
                                    EffectParameter.SetValue((Vector4)Parameter.Value);
                                else if (ParamType == typeof(Vector3))
                                    EffectParameter.SetValue((Vector3)Parameter.Value);
                                else if (ParamType == typeof(Vector2))
                                    EffectParameter.SetValue((Vector2)Parameter.Value);
                                else if (ParamType == typeof(Matrix))
                                    EffectParameter.SetValue((Matrix)Parameter.Value);
                                else if (ParamType == typeof(Texture2D))
                                    EffectParameter.SetValue((Texture2D)Parameter.Value);
                                else if (ParamType == typeof(TextureCube))
                                    EffectParameter.SetValue((TextureCube)Parameter.Value);
                                else if (ParamType.IsArray)
                                {
                                    var ElementType = ParamType.GetElementType();
                                    if (ElementType == typeof(float))
                                        EffectParameter.SetValue((float[])Parameter.Value);
                                    else if (ElementType == typeof(int))
                                        EffectParameter.SetValue((int[])Parameter.Value);
                                    else if (ElementType == typeof(Vector3))
                                        EffectParameter.SetValue((Vector3[])Parameter.Value);
                                    else if (ElementType == typeof(Vector2))
                                        EffectParameter.SetValue((Vector2[])Parameter.Value);
                                    else if (ElementType == typeof(Vector4))
                                        EffectParameter.SetValue((Vector4[])Parameter.Value);
                                    else if (ElementType == typeof(Matrix))
                                        EffectParameter.SetValue((Matrix[])Parameter.Value);
                                }
                            }
                        }
                    }
                    catch { }
                }

                foreach (EffectTechnique Technique in PostFX.Value.Techniques)
                {
                    RenderTarget2D Destination = (CurrentSource == RenderTarget) ? BackBuffer : RenderTarget;

                    GraphicsDevice.SetRenderTarget(Destination);
                    GraphicsDevice.Clear(Color.Black);

                    PostFX.Value.CurrentTechnique = Technique;

                    SpriteBatch.Begin(effect: PostFX.Value, samplerState: SamplerState.PointClamp);
                    SpriteBatch.Draw(CurrentSource, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
                    SpriteBatch.End();

                    GraphicsDevice.SetRenderTarget(null);
                    CurrentSource = Destination;
                }
            }

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(CurrentSource, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            SpriteBatch.End();

            UIManager.Render(GameTime);

            if (WhiteTex == null && BlackTex == null && NormalTex == null && CubeTex == null)
            {
                Texture2D WhiteTexture = new Texture2D(GraphicsDevice, 1, 1);
                WhiteTexture.SetData(new Color[] { Color.White });
                WhiteTex = WhiteTexture;

                Texture2D BlackTexture = new Texture2D(GraphicsDevice, 1, 1);
                BlackTexture.SetData(new Color[] { Color.Black });
                BlackTex = BlackTexture;

                Texture2D FlatNormalTexture = new Texture2D(GraphicsDevice, 1, 1);
                FlatNormalTexture.SetData(new Color[] { new Color(128, 128, 255) });
                NormalTex = FlatNormalTexture;

                CubeTex = new TextureCube(GraphicsDevice, 1, false, SurfaceFormat.Color);
                Color[] BlackColor = new Color[1] { Color.Black };
                CubeTex.SetData(CubeMapFace.PositiveX, BlackColor);
                CubeTex.SetData(CubeMapFace.NegativeX, BlackColor);
                CubeTex.SetData(CubeMapFace.PositiveY, BlackColor);
                CubeTex.SetData(CubeMapFace.NegativeY, BlackColor);
                CubeTex.SetData(CubeMapFace.PositiveZ, BlackColor);
                CubeTex.SetData(CubeMapFace.NegativeZ, BlackColor);
            }

            Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            SpriteBatch.Begin();
            DrawGUI(GameTime);
            ECManager.Instance.CallDrawGUIOnComponents(GameTime);
            SpriteBatch.End();
            Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            FrameCount++;
            ElapsedTime += (float)GameTime.ElapsedGameTime.TotalSeconds;
            if (ElapsedTime >= 1.0f)
            {
                CurrentFrameRate = FrameCount;
                FrameCount = 0;
                ElapsedTime = 0f;
            }

            base.Draw(GameTime);
        }

        public TextureCube GenerateCubeMap(Texture2D sourceTexture, float brightness = 1.0f)
        {
            // Determine the size of each cubemap face
            int faceSize = sourceTexture.Width / 4; // Assuming 4 faces horizontally

            TextureCube cubeMap = new TextureCube(GraphicsDevice, faceSize, false, SurfaceFormat.Color);
            Color[] sourceData = new Color[sourceTexture.Width * sourceTexture.Height];
            sourceTexture.GetData(sourceData);

            // Define the 6 cube map faces
            CubeMapFace[] cubeFaces = {
        CubeMapFace.PositiveX, CubeMapFace.NegativeX,
        CubeMapFace.PositiveY, CubeMapFace.NegativeY,
        CubeMapFace.PositiveZ, CubeMapFace.NegativeZ
    };

            // Define the source texture regions for each face
            Rectangle[] sourceRects = {
        new Rectangle(faceSize * 2, faceSize, faceSize, faceSize), // +X
        new Rectangle(0, faceSize, faceSize, faceSize),           // -X
        new Rectangle(faceSize, 0, faceSize, faceSize),           // +Y
        new Rectangle(faceSize, faceSize * 2, faceSize, faceSize), // -Y
        new Rectangle(faceSize, faceSize, faceSize, faceSize),     // +Z
        new Rectangle(faceSize * 3, faceSize, faceSize, faceSize) // -Z
    };

            for (int i = 0; i < 6; i++)
            {
                CubeMapFace face = cubeFaces[i];
                Rectangle sourceRect = sourceRects[i];
                Color[] faceData = new Color[faceSize * faceSize];

                // Copy data from the source texture to the cubemap face
                for (int y = 0; y < faceSize; y++)
                {
                    for (int x = 0; x < faceSize; x++)
                    {
                        int sourceX = sourceRect.X + x;
                        int sourceY = sourceRect.Y + y;
                        faceData[y * faceSize + x] = sourceData[sourceY * sourceTexture.Width + sourceX] * brightness;
                    }
                }

                cubeMap.SetData(face, faceData);
            }

            return cubeMap;
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
            ECManager.Instance.CallMainUpdateOnComponents(gameTime);

            if (Accumulator >= FixedTimeStep)
            {

                lock (PhyicsLock)
                {
                    PhysicsManager.Instance.PhysicsUpdate(FixedTimeStep);
                }

                GameTime fixedUpdateTime = new GameTime(TimeSpan.FromSeconds(TotalTime), TimeSpan.FromSeconds(FixedTimeStep));
                FixedUpdate(fixedUpdateTime);
                ECManager.Instance.CallFixedUpdateOnComponents(fixedUpdateTime);

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
