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

namespace Engine
{
    public class MainEngine : EngineManager
    {
        private Entity CameraEntity;
        private Entity PlayerEntity;
        public List<Transform> PointTransforms = new List<Transform>();
        public override void Awake()
        {
            Debug = true;
        }
        public override void Start()
        {
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
            CreateCamera();
            CreatePlayer();
        }

        public override void MainUpdate(GameTime GameTime)
        {
            UIControls.DisplayFramerate(CurrentFrameRate);
        }

        public override void FixedUpdate(GameTime gameTime)
        {
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
        }

        //Spawning Objects

        // Function to create Camera
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

            Transform _Transform = ECSManager.Instance.GetComponent<Transform>(FloorObj);
            _Transform.Position = new Vector3(0, -2, 0);
            _Transform.Scale = new Vector3(100, 1, 100);
            ECSManager.Instance.AddComponent(FloorObj, new MeshRenderer(FloorModel, [ FloorMaterial ]));
            ECSManager.Instance.AddComponent(FloorObj, new RigidBody
            {
                Mass = 0,
                Shapes = [ new BulletSharp.BoxShape(1, 1, 1) ],
                IsStatic = true,
                CollisionGroup = PhysicsManager.CreateCollisionMask([1]),
                CollisionMask = PhysicsManager.CreateCollisionMask([1, 2]),
            });
        }

        // Function to create Sphere object
        private void CreateSphere(Vector3 Position, Vector3 Rotation, float Scale, Color Color)
        {
            Entity SphereObj = ECSManager.Instance.CreateEntity();
            StaticMesh SphereModel = PrimitiveModel.CreateSphere(1);
            Texture2D CheckerTex = Content.Load<Texture2D>("GameContent/Textures/Default/checkerboard");
            Material SphereMaterial = new Material {DiffuseTexture = CheckerTex,  DiffuseColor = Color };


            Transform _Transform = ECSManager.Instance.GetComponent<Transform>(SphereObj);
            _Transform.Position = Position;
            _Transform.Rotation = Quaternion.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
            _Transform.Scale = new Vector3(Scale, Scale, Scale);

            ECSManager.Instance.AddComponent(SphereObj, new MeshRenderer(SphereModel, [ SphereMaterial ]));
            ECSManager.Instance.AddComponent(SphereObj, new RigidBody
            {
                Friction = 5,
                Mass = 1000 * Scale,
                Shapes = [ new BulletSharp.SphereShape(1) ],
                IsStatic = false,
                CollisionGroup = PhysicsManager.CreateCollisionMask([1]),
                CollisionMask = PhysicsManager.CreateCollisionMask([1, 2]),
            });
        }


        // Function to create Player object
        private void CreatePlayer()
        {
            float PlayerHeight = 5;
            PlayerEntity = ECSManager.Instance.CreateEntity();

            Transform _Transform = ECSManager.Instance.GetComponent<Transform>(PlayerEntity);
            _Transform.Position = Vector3.Up * 10;

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

    }
}
