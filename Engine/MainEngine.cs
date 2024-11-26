using Engine.Core.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Core.ECS;
using Engine.Core.Physics;
using Engine.Core.Components.Physics;
using Engine.Core;
using Engine.Components;
using System;
using Engine.Core.Rendering;
using Engine.Core.Components.Rendering;

namespace Engine
{
    public class MainEngine : EngineManager
    {
        private int CameraEntity;
        private int PlayerEntity;
        private int DirectionalLightEntity;

        public override void Start()
        {
            Window.Title = "Game";

            // Call functions to initialize each object
            CreateCamera();
            CreateDirectionalLight();
            CreateFloor();
            CreateSphere(new Vector3(-10, 10, -10), new Vector3(45, 0, 0), 3, Color.Cyan);
            CreateSphere(new Vector3(5, 10, -5), new Vector3(0, 45, 0), 2, Color.Red);
            CreateSphere(new Vector3(-15, 10, 15), new Vector3(0, 0, 90), 4, Color.Green);
            CreateSphere(new Vector3(20, 10, 20), new Vector3(90, 0, 0), 1.5f, Color.Yellow);
            CreateSphere(new Vector3(0, 10, 20), new Vector3(0, 90, 0), 2.5f, Color.Blue);
            CreatePlayer();
        }

        public override void MainUpdate(GameTime GameTime)
        {
            UIControls.DisplayFramerate(CurrentFrameRate);
        }
        public override void FixedUpdate(GameTime gameTime)
        {

        }


        public override void Render(GameTime GameTime)
        {
            Graphics.GraphicsDevice.Clear(Color.SkyBlue);
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
            ECSManager.Instance.AddComponent(CameraEntity, new Transform { Position = new Vector3(0, 5, 10) });
            Camera Cam = new Camera();
            ECSManager.Instance.AddComponent(CameraEntity, Cam);
        }

        // Function to create Directional Light
        private void CreateDirectionalLight()
        {
            DirectionalLightEntity = ECSManager.Instance.CreateEntity();
            ECSManager.Instance.AddComponent(DirectionalLightEntity, new LightComponent
            {
                LightType = LightType.Directional,
                Color = Color.White,
                Direction = new Vector3(-1, -1, -1),
                Intensity = 1
            });
        }

        // Function to create the Floor object
        private void CreateFloor()
        {
            int FloorObj = ECSManager.Instance.CreateEntity();
            StaticMesh FloorModel = PrimitiveModel.CreateBox(1, 1, 1);
            Texture2D CheckerTex = Content.Load<Texture2D>("GameContent/Textures/Default/checkerboard");
            Material FloorMaterial = new Material
            {
                DiffuseTexture = CheckerTex,
            };

            ECSManager.Instance.AddComponent(FloorObj, new Transform
            {
                Position = new Vector3(0, -2, 0),
                Scale = new Vector3(100, 1, 100)
            });
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
            int SphereObj = ECSManager.Instance.CreateEntity();
            StaticMesh SphereModel = PrimitiveModel.CreateSphere(1);
            Texture2D CheckerTex = Content.Load<Texture2D>("GameContent/Textures/Default/checkerboard");
            Material SphereMaterial = new Material {DiffuseTexture = CheckerTex,  DiffuseColor = Color };

            ECSManager.Instance.AddComponent(SphereObj, new Transform
            {
                Position = Position,
                Rotation = Quaternion.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z),
                Scale = new Vector3(Scale, Scale, Scale)
            });
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
            PlayerEntity = ECSManager.Instance.CreateEntity();
            ECSManager.Instance.AddComponent(PlayerEntity, new Transform
            {
                Position = Vector3.Up * 10,
                Rotation = Quaternion.Identity
            });

            RigidBody PlayerBody = new RigidBody
            {
                Mass = 70,
                Shapes = new[] { new BulletSharp.CapsuleShape(1, 5) },
                CollisionGroup = PhysicsManager.CreateCollisionMask([2]),
                CollisionMask = PhysicsManager.CreateCollisionMask([1]),
            };

            ECSManager.Instance.AddComponent(PlayerEntity, PlayerBody);
            PlayerController Controller = new PlayerController(PlayerBody, ECSManager.Instance.GetComponent<Camera>(CameraEntity), 1, 50, 5);
            ECSManager.Instance.AddComponent(PlayerEntity, Controller);
        }

    }
}
