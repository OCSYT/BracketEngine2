# Bracket Engine 2

![Bracket Engine](https://github.com/user-attachments/assets/58626242-ae5b-4299-9871-8cb5880ffc89)

---

## Setup

*Make sure you have [Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-gb/download/details.aspx?id=40784)*
# Visual Studio 2022
1. Clone the repository.
2. Open the `.sln` file with **Visual Studio 2022**.
3. Set the build mode to **Release x64**.
4. Build the project
# Visual Studio Code
1. Clone the repository.
2. Open the folder in Visual Studio Code
3. Install the C# Dev Kit Extension, C# Extension and .NET Install Tool Extension
4. Build the project with CTRL + ALT + B

---

## Information Overview

### File Structure
- **Main Engine Loop**: `MainEngine.cs`
- **UI Form Code**: `UI/UIControls.cs`
- **Custom Components**: `Components` folder
- **Assets (e.g., sounds, images)**: `Content` folder  
  *Note*: `defaultFont.spritefont` is required for UI functionality.
- **Core Engine Code & Components**: `Core` folder

---

## Engine Lifecycle

1. **Awake**: MonoGame initialization.
2. **Start**: MonoGame `LoadContent`.
3. **MainUpdate**: MonoGame `Update`.
4. **FixedUpdate**: MonoGame `Update` with `FixedTimeStep`.
5. **Render**: MonoGame `Draw`.
6. **DrawGUI**: MonoGame `Draw`, called after `Render` and `SpriteBatch.Begin`.
7. **OnDestroy**: MonoGame `UnloadContent`.

---

## Component Lifecycle

1. **Awake**: Called on the frame when the component is created.
2. **Start**: Called on the next frame before `MainUpdate` and `FixedUpdate`.
3. **MainUpdate**: Called during MonoGame `Update`.
4. **FixedUpdate**: Runs on MonoGame `Update` with `FixedTimeStep`.
5. **Render**: Called manually in the engine's `Render` function (requires camera matrices).
6. **DrawGUI**: Called after `Render` (after `SpriteBatch.Begin`).
7. **OnDestroy**: Called on the frame the component is destroyed.

---

## Physics

- Physics is handled using **BulletSharp** and runs in the `FixedUpdate` loop.

---

## Useful Classes

### Core
- **`Core.EngineManager.cs` (singleton)**  
  Access key features like `Content` (ContentManager), `DefaultShader` (Effect), `UIControls` (UIControls), and `Graphics` (GraphicsDeviceManager).

### UI
- **`Core.UI.UIControls.cs`**  
  Manages renderable UI elements, similar to WinForms.

### ECS
- **`Core.ECS.ECSManager.cs` (singleton)**  
  For creating and destroying entities and components.
- **`Core.ECS.Component.cs`**  
  *All components should inherit from this class.*  
  Override methods like `Start`, `MainUpdate`, `FixedUpdate`, etc.

### Rendering
- **`Core.Rendering.LightManager.cs` (singleton)**  
  Set ambient light colors.
- **`Core.Rendering.StaticMesh.cs`**  
  Generate procedural models compatible with MeshRenderer.
- **`Core.Rendering.PrimitiveModel.cs`**  
  Provides functions to create primitives like boxes, spheres, etc.
- **`Core.Rendering.Material.cs`**  
  Defines materials for MeshRenderers (uses `DefaultShader` by default).

### Physics
- **`Core.Rendering.PhysicsManager.cs` (singleton)**  
  Create collision shapes, masks, and raycasting.

### Audio
- **`Core.Audio.SoundManager.cs` (singleton)**  
  Handles sound creation and playback using the content pipeline.

---

## Useful Components

### Core
- **`Core.Components.Transform.cs`**  
  Defines 3D position, scale, rotation, and directional vectors (`Up`, `Right`, `Forward`).  
  *All entities have a `Transform` component by default.*

### Physics
- **`Core.Components.Physics.Rigidbody.cs`**  
  Integrates entities with the BulletSharp physics system for interactions.

### Rendering
- **`Core.Components.Camera.cs`**  
  Controls the camera in the scene (perspective or orthographic view).
- **`Core.Components.LightComponent.cs`**  
  Adds directional and point lights to the scene.
- **`Core.Components.MeshRenderer.cs`**  
  Renders models or static meshes with a specified material.

---

## Useful Links

- [MonoGame Documentation](https://docs.monogame.net/)
- [MonoGame.UI.Forms](https://github.com/csharpskolan/MonoGame.UI.Forms)
- [BulletSharp Documentation](https://andrestraks.github.io/BulletSharp/)
- [MonoGame Samples](https://docs.monogame.net/articles/samples.html)
- [ShaderToy](https://www.shadertoy.com/)
