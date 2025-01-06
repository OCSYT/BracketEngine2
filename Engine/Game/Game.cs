using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Core.ECS;
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
        public List<Transform> PointTransforms = new List<Transform>();
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


            // Call functions to initialize each object
            CreateFloor();
            CreatePointLight(new Vector3(0, 25, -50), Color.Red);
            CreatePointLight(new Vector3(50, 25, 0), Color.Green);
            CreatePointLight(new Vector3(0, 25, 50), Color.Blue);
            CreateSphere(new Vector3(-10, 10, -10), new Vector3(45, 0, 0), 3, Color.Cyan);
            CreateSphere(new Vector3(5, 10, -5), new Vector3(0, 45, 0), 2, Color.Red);
            CreateSphere(new Vector3(-15, 10, 15), new Vector3(0, 0, 90), 4, Color.Green);
            CreateSphere(new Vector3(20, 10, 20), new Vector3(90, 0, 0), 1.5f, Color.Yellow);
            CreateSphere(new Vector3(0, 10, 20), new Vector3(0, 90, 0), 2.5f, Color.Blue);
            HandTransform = CreateHandBox();
            CreateAnimatedModel();
            CreateCamera();
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
            //Point lights moving
            foreach (Transform pointlight in PointTransforms)
            {
                pointlight.Position = Vector3.Transform(pointlight.Position, Quaternion.CreateFromYawPitchRoll(1 * (MathF.PI / 180), 0, 0));
            }
        }

        public override void Render(GameTime GameTime)
        {
            Graphics.GraphicsDevice.Clear(Color.Black);
            BasicEffect Effect = new BasicEffect(Graphics.GraphicsDevice) { AmbientLightColor = Vector3.One / 4 };
            var CameraObj = ECSManager.Instance.GetComponent<Camera>(CameraEntity);

            if (CameraObj != null)
            {
                Effect.View = CameraObj.GetViewMatrix();
                Effect.Projection = CameraObj.GetProjectionMatrix();
                ECSManager.Instance.CallRenderOnComponents(Effect, CameraObj.GetViewMatrix(), CameraObj.GetProjectionMatrix(), GameTime);
            }
            GraphicsDevice.SetRenderTarget(null);
        }

        //Spawning Objects

        private void CreateCamera()
        {
            CameraEntity = ECSManager.Instance.CreateEntity();
            Camera Cam = new Camera();
            ECSManager.Instance.AddComponent(CameraEntity, Cam);
        }

        private void CreatePointLight(Vector3 Position, Color Color, float Range = 50)

        {
            Entity PointLightEntity = ECSManager.Instance.CreateEntity();
            PointTransforms.Add(PointLightEntity.Transform);
            PointLightEntity.Transform.Position = Position;
            LightComponent PointLight = new LightComponent
            {
                LightType = LightType.Point,
                Color = Color,
                Intensity = 25,
                Range = Range
            };
            ECSManager.Instance.AddComponent(PointLightEntity, PointLight);
        }

        // Function to create the Floor object
        private void CreateFloor()
        {
            Entity FloorObj = ECSManager.Instance.CreateEntity();
            StaticMesh FloorModel = PrimitiveModel.CreateBox(1, 1, 1);
            Texture2D CheckerTex = Content.Load<Texture2D>("GameContent/Textures/Default/checkerboard");
            Material FloorMaterial = new Material
            {
                DiffuseTexture = CheckerTex,
            };

            FloorObj.Transform.Position = new Vector3(0, -2, 0);
            FloorObj.Transform.Scale = new Vector3(100, 1, 100);
            ECSManager.Instance.AddComponent(FloorObj, new MeshRenderer(FloorModel, [FloorMaterial]));
            ECSManager.Instance.AddComponent(FloorObj, new RigidBody
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
            Entity SphereObj = ECSManager.Instance.CreateEntity();
            StaticMesh SphereModel = PrimitiveModel.CreateSphere(1);
            Texture2D CheckerTex = Content.Load<Texture2D>("GameContent/Textures/Default/checkerboard");
            Material SphereMaterial = new Material { DiffuseTexture = CheckerTex, DiffuseColor = Color };

            SphereObj.Transform.Position = Position;
            SphereObj.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
            SphereObj.Transform.Scale = new Vector3(Scale, Scale, Scale);

            ECSManager.Instance.AddComponent(SphereObj, new MeshRenderer(SphereModel, [SphereMaterial]));
            ECSManager.Instance.AddComponent(SphereObj, new RigidBody
            {
                Friction = 25 * Scale,
                Mass = 1000 * Scale,
                Shapes = [new BulletSharp.SphereShape(1)],
                IsStatic = false,
                CollisionGroup = PhysicsManager.CreateCollisionMask([1]),
                CollisionMask = PhysicsManager.CreateCollisionMask([1, 2]),
            });
        }

        private Transform CreateHandBox()
        {
            Entity BoxObj = ECSManager.Instance.CreateEntity();
            StaticMesh BoxModel = PrimitiveModel.CreateBox(1, 1, 1);
            Texture2D CheckerTex = Content.Load<Texture2D>("GameContent/Textures/Default/checkerboard");
            Material BoxMaterial = new Material { DiffuseTexture = CheckerTex, DiffuseColor = Color.Purple };
            BoxObj.Transform.Scale = Vector3.One / 2;

            ECSManager.Instance.AddComponent(BoxObj, new MeshRenderer(BoxModel, [BoxMaterial]));
            return BoxObj.Transform;
        }


        private void CreatePlayer()
        {
            float PlayerHeight = 5;
            PlayerEntity = ECSManager.Instance.CreateEntity();

            PlayerEntity.Transform.Position = Vector3.Up * 10;

            RigidBody PlayerBody = new RigidBody
            {
                Mass = 70,
                Shapes = new[] { new BulletSharp.CapsuleShape(1, PlayerHeight) },
                CollisionGroup = PhysicsManager.CreateCollisionMask([2]),
                CollisionMask = PhysicsManager.CreateCollisionMask([1]),
            };

            ECSManager.Instance.AddComponent(PlayerEntity, PlayerBody);
            PlayerController Controller = new PlayerController()
            {
                Body = PlayerBody,
                CameraObj = ECSManager.Instance.GetComponent<Camera>(CameraEntity),
                Height = PlayerHeight
            };
            ECSManager.Instance.AddComponent(PlayerEntity, Controller);
        }

        public void CreateAnimatedModel()
        {
            AnimatedModel = ECSManager.Instance.CreateEntity();
            Model Model = Content.Load<Model>("GameContent/Walking");
            AnimPlayer = Model.GetAnimations();
            Clip WalkClip = AnimPlayer.Clips["mixamo.com"];
            AnimPlayer.SetClip(WalkClip);

            MeshRenderer Renderer = new MeshRenderer(Model, [new Material { DiffuseColor = Color.Cyan }], AnimPlayer);
            ECSManager.Instance.AddComponent(AnimatedModel, Renderer);
            AnimatedModel.Transform.Scale = Vector3.One / 20;
            AnimatedModel.Transform.Position += -Vector3.Forward * 5 - Vector3.Right * 3;
            AnimatedModel.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(45), 0, 0);
        }

    }
}
