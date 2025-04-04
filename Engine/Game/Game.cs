using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Core.EC;
using Engine.Core.Physics;
using Engine.Core;
using Engine.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using Engine.Core.Rendering;
using Engine.Core.Components;
using Engine.Core.Components.Rendering;
using Engine.Core.Components.Physics;
using Aether.Animation;
using Myra.Graphics2D.UI;
using Myra;

namespace Engine.Game
{
    public class Game : EngineManager
    {
        private Entity CameraEntity;
        private Entity PlayerEntity;
        private Entity AnimatedModel;
        private Animations AnimPlayer;
        private Transform HandTransform;
        private Vector3 HandOffset = Vector3.Up;
        public override void Awake()
        {
            //Debug = true;
        }
        public override void Start()
        {
            Effect ACESToneMapper = Content.Load<Effect>("Rendering/Shaders/ACES");
            PostFxManager.Instance.AddEffect(ACESToneMapper, new List<KeyValuePair<string, object>>() {
                new ("Exposure", 1.5f),
                new ("Gamma", .5f)
            });
            Effect Bloom = Content.Load<Effect>("Rendering/Shaders/Bloom");
            PostFxManager.Instance.AddEffect(Bloom, new List<KeyValuePair<string, object>>() {
                new ("BloomIntensity", 1.0f),    // Controls overall bloom strength
                new ("BloomThreshold", 1.0f),    // Sets brightness threshold for bloom
                new ("BloomBlurSize", 2.0f),     // Controls the spread of the bloom
                new ("BloomExposure", 2.0f)      // Adjusts bloom exposure
            });
            LightManager.Instance.EnvironmentMap = GenerateCubeMap(Content.Load<Texture2D>("Main/Skybox/Skybox"), 0.5f);

            // Call functions to initialize each object
            CreateCamera();
            CreateSkybox();
            CreateFloor();
            CreateDirectionalLight(new Vector3(225, 45, 0), Color.White, .5f);
            CreateSphere(new Vector3(-10, 10, -10), new Vector3(45, 0, 0), 3, Color.Cyan);
            CreateSphere(new Vector3(5, 10, -5), new Vector3(0, 45, 0), 2, Color.Red);
            CreateSphere(new Vector3(-15, 10, 15), new Vector3(0, 0, 90), 4, Color.Green);
            CreateSphere(new Vector3(20, 10, 20), new Vector3(90, 0, 0), 1.5f, Color.Yellow);
            CreateSphere(new Vector3(0, 10, 20), new Vector3(0, 90, 0), 2.5f, Color.Blue);
            HandTransform = CreateHandBox();
            CreateAnimatedModel();
            CreatePlayer();

        }

        public override void MainUpdate(GameTime GameTime)
        {

            //Update Animations
            AnimPlayer.Update(GameTime.ElapsedGameTime, true, Matrix.Identity);


            Matrix RightHandMatrix = AnimPlayer.WorldTransforms[AnimPlayer.GetBoneIndex("mixamorig:RightHand")];
            RightHandMatrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);


            //Scale our positon to be same as AnimatedModel
            position *= AnimatedModel.Transform.Scale;

            //Transform Position relative to AnimatedModel
            HandTransform.Position = AnimatedModel.Transform.Position + Vector3.Transform(position +
                Vector3.Transform(HandOffset, rotation),
                AnimatedModel.Transform.Rotation);

            //Transform Rotation relative to AnimatedModel
            HandTransform.Rotation = AnimatedModel.Transform.Rotation * Quaternion.Negate(rotation);
        }

        public override void FixedUpdate(GameTime GameTime)
        {

        }

        public override void Render(GameTime GameTime)
        {
            Graphics.GraphicsDevice.Clear(Color.Black);
            BasicEffect Effect = new BasicEffect(Graphics.GraphicsDevice);
            var CameraObj = ECManager.Instance.GetComponent<Camera>(CameraEntity);

            if (CameraObj != null)
            {
                Effect.View = CameraObj.GetViewMatrix();
                Effect.Projection = CameraObj.GetProjectionMatrix();
                ECManager.Instance.CallRenderOnComponents(Effect, CameraObj.GetViewMatrix(), CameraObj.GetProjectionMatrix(), GameTime);
            }
            GraphicsDevice.SetRenderTarget(null);
        }

        //Spawning Objects

        void CreateDirectionalLight(Vector3 Rotation, Color Color, float Intensity)
        {
            Entity DirectionalLightEntity = ECManager.Instance.CreateEntity();
            DirectionalLightEntity.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(Rotation.X), MathHelper.ToRadians(Rotation.Y), MathHelper.ToRadians(Rotation.Z));
            LightComponent DirectionalLight = new LightComponent
            {
                LightType = LightType.Directional,
                Color = Color,
                Intensity = Intensity
            };
            ECManager.Instance.AddComponent(DirectionalLightEntity, DirectionalLight);
        }

        private void CreateCamera()
        {
            CameraEntity = ECManager.Instance.CreateEntity();
            Camera Cam = new Camera();
            ECManager.Instance.AddComponent(CameraEntity, Cam);
        }

        // Function to create the Floor object
        private void CreateFloor()
        {
            Entity FloorObj = ECManager.Instance.CreateEntity();
            StaticMesh FloorModel = PrimitiveModel.CreateBox(1, 1, 1);
            Texture2D CheckerTex = Content.Load<Texture2D>("Main/Textures/Default/checkerboard");
            Material FloorMaterial = new Material
            {
                BaseColorTexture = CheckerTex,
            };

            FloorObj.Transform.Position = new Vector3(0, -2, 0);
            FloorObj.Transform.Scale = new Vector3(100, 1, 100);
            ECManager.Instance.AddComponent(FloorObj, new MeshRenderer(FloorModel, [FloorMaterial]));
            ECManager.Instance.AddComponent(FloorObj, new RigidBody
            {
                Mass = 0,
                Shapes = [new BulletSharp.BoxShape(1, 1, 1)],
                IsStatic = true,
                CollisionGroup = PhysicsManager.CreateCollisionMask([1]),
                CollisionMask = PhysicsManager.CreateCollisionMask([1, 2]),
            });
        }


        private void CreateSphere(Vector3 Position, Vector3 Rotation, float Scale, Color Color)
        {
            Entity SphereObj = ECManager.Instance.CreateEntity();
            StaticMesh SphereModel = PrimitiveModel.CreateSphere(1);
            Texture2D CheckerTex = Content.Load<Texture2D>("Main/Textures/Default/checkerboard");
            Material SphereMaterial = new Material
            {
                BaseColorTexture = CheckerTex,
                BaseColor = Color,
                RoughnessIntensity = 1f,
                MetallicIntensity = 1f
            };

            SphereObj.Transform.Position = Position;
            SphereObj.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
            SphereObj.Transform.Scale = new Vector3(Scale, Scale, Scale);

            ECManager.Instance.AddComponent(SphereObj, new MeshRenderer(SphereModel, [SphereMaterial]));
            ECManager.Instance.AddComponent(SphereObj, new RigidBody
            {
                Friction = 25 * Scale,
                Mass = 1000 * Scale,
                Shapes = [new BulletSharp.SphereShape(1)],
                IsStatic = false,
                CollisionGroup = PhysicsManager.CreateCollisionMask([1]),
                CollisionMask = PhysicsManager.CreateCollisionMask([1, 2]),
            });
        }

        private void CreateSkybox()
        {
            float Scale = ECManager.Instance.GetComponent<Camera>(CameraEntity).FarClip / 2f;
            Entity SkyboxObj = ECManager.Instance.CreateEntity();
            StaticMesh SkyboxModel = PrimitiveModel.CreateBox(Scale, Scale, Scale);
            Material SphereMaterial = new Material
            {
                Shader = Content.Load<Effect>("Rendering/Shaders/Skybox"),
                CullMode = CullMode.None
            };

            ECManager.Instance.AddComponent(SkyboxObj, new MeshRenderer(SkyboxModel, [SphereMaterial]));
        }

        private Transform CreateHandBox()
        {
            Entity BoxObj = ECManager.Instance.CreateEntity();
            StaticMesh BoxModel = PrimitiveModel.CreateBox(1, 1, 1);
            Texture2D CheckerTex = Content.Load<Texture2D>("Main/Textures/Default/checkerboard");
            Material BoxMaterial = new Material { BaseColorTexture = CheckerTex, BaseColor = Color.Purple };
            BoxObj.Transform.Scale = Vector3.One / 2;

            ECManager.Instance.AddComponent(BoxObj, new MeshRenderer(BoxModel, [BoxMaterial]));
            return BoxObj.Transform;
        }


        private void CreatePlayer()
        {
            float PlayerHeight = 5;
            PlayerEntity = ECManager.Instance.CreateEntity();

            PlayerEntity.Transform.Position = Vector3.Up * 10;

            RigidBody PlayerBody = new RigidBody
            {
                Mass = 70,
                Shapes = new[] { new BulletSharp.CapsuleShape(1, PlayerHeight) },
                CollisionGroup = PhysicsManager.CreateCollisionMask([2]),
                CollisionMask = PhysicsManager.CreateCollisionMask([1]),
            };

            ECManager.Instance.AddComponent(PlayerEntity, PlayerBody);
            PlayerController Controller = new PlayerController()
            {
                Body = PlayerBody,
                CameraObj = ECManager.Instance.GetComponent<Camera>(CameraEntity),
                Height = PlayerHeight,
                Sensitivity = 5
            };
            ECManager.Instance.AddComponent(PlayerEntity, Controller);
        }

        public void CreateAnimatedModel()
        {
            AnimatedModel = ECManager.Instance.CreateEntity();
            Model Model = Content.Load<Model>("Main/Walking");
            AnimPlayer = Model.GetAnimations();
            Clip WalkClip = AnimPlayer.Clips["mixamo.com"];
            AnimPlayer.SetClip(WalkClip);

            MeshRenderer Renderer = new MeshRenderer(Model, [
                new Material {
                    BaseColor = Color.White,
                    RoughnessIntensity = 1f,
                    MetallicIntensity = 1f
                }],
                AnimPlayer);

            ECManager.Instance.AddComponent(AnimatedModel, Renderer);
            AnimatedModel.Transform.Scale = Vector3.One / 20;
            AnimatedModel.Transform.Position += -Vector3.Forward * 5 - Vector3.Right * 3;
            AnimatedModel.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(45), 0, 0);
        }

    }
}
