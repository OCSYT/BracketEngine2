# Bracket Engine 2

---

## Setup

### Prerequisite
Ensure you have [Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-gb/download/details.aspx?id=40784) installed.
Ensure you have [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download) installed. 

### Visual Studio 2022
1. Clone the repository.
2. Open the `.sln` file with **Visual Studio 2022**.
3. Set the build mode to **Release x64**.
4. Build the project.

### Visual Studio Code
1. Clone the repository.
2. Open the folder in **Visual Studio Code**.
3. Install the following extensions:
   - **C# Dev Kit Extension**
   - **C# Extension**
   - **.NET Install Tool Extension**
   - **MonoGame Content Builder (Editor)**
4. Build the project using `CTRL + SHIFT + B`.

---

## Information Overview

### File Structure
- **Main Game Loop**: `Game.Game.cs`
- **UI Loop**: `Game.UI.cs`
- **Custom Components**: `Components` folder
- **Assets (e.g., sounds, images)**: `Content` folder
- **Core Engine Code & Components**: `Core` folder

---

## Game Lifecycle

1. **Awake**: MonoGame initialization.
2. **Start**: MonoGame `LoadContent`.
3. **MainUpdate**: MonoGame `Update`.
4. **FixedUpdate**: MonoGame `Update` with `FixedTimeStep`.
5. **Render**: MonoGame `Draw`.
6. **DrawGUI**: MonoGame `Draw`, called after `Render` and `SpriteBatch.Begin`.
7. **OnDestroy**: MonoGame `UnloadContent`.

---

## Component Lifecycle

1. **Awake**: Called on the frame the component is created.
2. **Start**: Called on the next frame before `MainUpdate` and `FixedUpdate`.
3. **MainUpdate**: Called during MonoGame `Update`.
4. **FixedUpdate**: Runs on MonoGame `Update` with `FixedTimeStep`.
5. **Render**: Called manually in the engine's `Render` function (requires camera matrices).
6. **DrawGUI**: Called after `Render` (after `SpriteBatch.Begin`).
7. **OnDestroy**: Called on the frame the component is destroyed.

---

## Physics
- Physics is handled using [BulletSharp](https://andrestraks.github.io/BulletSharp/) and runs in the `FixedUpdate` loop.

---

## UI
- UI Code resides in `Game.UI.cs` and utilizes [Myra](https://github.com/rds1983/Myra/wiki).
- **UI Initialization**: `Start`.
- **UI Rendering**: `Render`.

---

## Animation
- Uses [Aether.Animation](https://github.com/nkast/Aether.Extras/tree/main/Animation).
- Refer to the built-in sample project for usage.
- **`Core.Components.Meshrenderer.cs`** supports CPU animated models by default.

---

## Useful Classes

### Core
- **`Core.EngineManager.cs` (singleton)**  
  Access key features like `Content` (ContentManager), `DefaultShader` (Effect), `UIControls` (UIControls), and `Graphics` (GraphicsDeviceManager).

### ECS
- **`Core.ECS.ECSManager.cs` (singleton)**  
  Manages entities and components.
- **`Core.ECS.Component.cs`**  
  All components must inherit from this class. Override lifecycle methods like `Start`, `MainUpdate`, `FixedUpdate`, etc.

### Rendering
- **`Core.Rendering.LightManager.cs` (singleton)**  
  Configure ambient light.
- **`Core.Rendering.PostFxManager.cs` (singleton)**  
  Allows adding rendering of fullscreen shaders after main Render function has been called.
- **`Core.Rendering.StaticMesh.cs`**  
  Generate procedural models compatible with MeshRenderer.
- **`Core.Rendering.PrimitiveModel.cs`**  
  Create primitives like boxes and spheres.
- **`Core.Rendering.Material.cs`**  
  Define materials for MeshRenderers (defaults to `DefaultShader`).

### Physics
- **`Core.Rendering.PhysicsManager.cs` (singleton)**  
  Provides collision shapes, masks, and raycasting utilities.

### Audio
- **`Core.Audio.SoundManager.cs` (singleton)**  
  Manages sound creation and playback via the content pipeline.

---

## Useful Components

### Core
- **`Core.Components.Transform.cs`**  
  Defines 3D position, scale, rotation, and directional vectors (`Up`, `Right`, `Forward`).  
  *Automatically included in all entities.*

### Physics
- **`Core.Components.Physics.Rigidbody.cs`**  
  Integrates with [BulletSharp](https://andrestraks.github.io/BulletSharp/) for physics interactions.

### Rendering
- **`Core.Components.Camera.cs`**  
  Controls the scene camera (perspective or orthographic view).
- **`Core.Components.LightComponent.cs`**  
  Adds directional and point lights.
- **`Core.Components.MeshRenderer.cs`**  
  Renders models or meshes with specified materials.

---

## Useful Links

- [Aether.Animation](https://github.com/nkast/Aether.Extras/tree/main/Animation)
- [BulletSharp Documentation](https://andrestraks.github.io/BulletSharp/)
- [MonoGame Documentation](https://docs.monogame.net/)
- [Myra](https://github.com/rds1983/Myra/wiki)
- [MonoGame Samples](https://docs.monogame.net/articles/samples.html)
- [ShaderToy](https://www.shadertoy.com/)
