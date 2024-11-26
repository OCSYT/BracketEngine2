# Bracket Engine 2

## Setup
- Clone the repository
- Open with Visual Studio 2022
- Set build mode to Release x64

## Info
- Main Engine loop is in **MainEngine.cs**
- UI Form Code is in UI/UIControls.cs
- Custom Components go in Components Folder
- Assets such as sounds, images, etc go in the Content Folder `(keep in mind defaultFont.spritefont is required for UI to work)`
- Core engine code and components is found in Core folder

## Engine Loop
- Awake (Monogame Initalization)
- Start (Monogame LoadContent)
- MainUpdate (Monogame Update)
- FixedUpdate (Monogame Update running on FixedTimeStep)
- Render (Monogame Draw)
- DrawGUI (Monogame Draw - Called after Render after SpriteBatch.Begin)
- OnDestroy (Monogame UnloadContent)

## Component Loop
- Awake (Called on frame component is created)
- Start (Called on next frame before MainUpdate and FixedUpdate)
- MainUpdate (Called on Monogame Update)
- FixedUpdate (Monogame Update running on FixedTimeStep)
- Render (Called Manually in Render Function in Engine loop as Camera Matricies need to be defined)
- DrawGUI (Called after Render - Called after Render after SpriteBatch.Begin)
- OnDestroy (Called on frame component is destroyed)

## Physics
- Physics (Handled using BulletSharp) runs on the FixedUpdate loop

## Useful links
- [Monogame](https://docs.monogame.net/)
- [Monogame.UI.Forms](https://github.com/csharpskolan/MonoGame.UI.Forms)
- [BulletSharp](https://andrestraks.github.io/BulletSharp/)
