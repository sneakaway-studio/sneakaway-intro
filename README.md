# Sneakaway Studio Intro

The Sneakaway Studio logo intro for Unity games. On black, the triangle outline draws itself from
the top, the **S** rises in and **sneakaway.studio** types out. Then everything fades to the game's
own background colour, and the game appears. It takes about 3.5 seconds, and a tap skips
to the fade.

It runs in plain code with no tweening library, and it's all sprites (no fonts or TextMesh Pro),
so the only requirement is Unity's UI.

## Requirements

- Unity 6 (6000.0 or later).

## Install

In Unity: **Window → Package Manager → + → Install package from git URL…** and paste:

```
https://github.com/sneakaway-studio/sneakaway-intro.git#v1.1.0
```

The `#v1.1.0` pins a version, so the project won't change until you choose to update (see
[Updating a project](#updating-a-project)). Without it, you get whatever is on `main`
when you install.

Instead of using Package Manager, you can add the same line to `Packages/manifest.json`:

```json
"com.sneakawaystudio.intro": "https://github.com/sneakaway-studio/sneakaway-intro.git#v1.1.0",
```

The package then appears in the Project window under **Packages → Sneakaway Studio Intro**.

## Use it: the simple way (its own scene)

For a game that has no loading code of its own. The intro plays, loads your first scene behind
itself, then fades away to reveal it.

1. Make a new scene, for example `Intro`.
2. From **Packages → Sneakaway Studio Intro → Prefabs**, drag **SneakawayIntro** into the scene.
3. Select it. In the **Sneakaway Intro** component, set **Next Scene** to the name of your first
   real scene (for example `MainMenu`).
4. Set the end colour: in the Hierarchy, expand **SneakawayIntro**, select **FadeToGame** and set
   its **Image** colour to your game's background colour. The intro fades to this colour, then
   fades away to show the game, so matching it makes the hand-over smooth. (Glitchword uses
   `#14072E`.) The change is saved in your scene, not in the package.
5. Add a **Camera** with **Clear Flags / Background Type** set to a solid black colour. The intro
   draws over everything, but without a camera the screen behind it is undefined, and the editor
   shows "No cameras rendering".
6. For tap-to-skip, the scene needs an **EventSystem**: **GameObject → UI → Event System**.
7. **File → Build Profiles → Scene List**: add the `Intro` scene and drag it to the **top** (scene
   0), so it's the scene the game starts with.

## Use it: from your own loading code

If your game already has a start-up scene that loads things (saved data, settings, other
scenes), the intro can play *while* that loading happens, so it adds no waiting. Glitchword
does this in its `Bootstrapper`.

1. Put the **SneakawayIntro** prefab in your start-up scene and leave **Next Scene** empty.
2. Set the **FadeToGame** colour as in step 4 above.
3. Reference it from your loader and call it like this:

```csharp
using System.Threading.Tasks;
using SneakawayStudio;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public SneakawayIntro intro;

    async void Start()
    {
        bool playIntro = intro != null && intro.ShouldPlay;
        if (intro != null && !playIntro)
            intro.Remove();

        // starts the animation and returns at once; the Task finishes when the screen has
        // faded to the game's colour
        Task introDone = playIntro ? intro.PlayAsync() : Task.CompletedTask;

        // ... your loading here (saved data, settings, additive scenes) ...

        await introDone;
        // ... load or switch to your first scene here ...

        if (playIntro)
            _ = intro.HideAsync(); // fades the intro away, revealing the scene, then deletes it
    }
}
```

`PlayAsync()` starts the animation. `HideAsync()` fades the intro out and deletes it. `Skip()`
jumps to the fade, the same as a tap. `Remove()` deletes it straight away. `ShouldPlay` is false
in the editor when **Play In Editor** is off.

The intro must stay alive while your first scene loads. Keep it in a scene you don't unload
(load your first scene additively), or call `DontDestroyOnLoad` on it. Standalone mode
does the second for you.

## Settings

All of these are on the **Sneakaway Intro** component.

| Setting | What it does |
|---|---|
| **Next Scene** | Standalone mode. Leave it empty when your own code calls `PlayAsync`. |
| **Draw Seconds** | How long the triangle takes to draw (1). |
| **Letter At / Letter Seconds** | When the S starts rising, and for how long (0.75, 0.5). |
| **Url At / Url Seconds** | When sneakaway.studio starts typing, and for how long (1.25, 0.7). |
| **Fade At / Fade Seconds** | When the fade to the game's colour starts, and for how long (2.9, 0.5). The logo holds until then. |
| **Hide Seconds** | How long `HideAsync` takes to fade the intro away (0.4). |
| **Url Char Ends** | Where each character of the URL image ends, so the URL types out a letter at a time. |
| **Letter Rise** | How far the S rises, as a share of the triangle's height (0.06). |
| **Play In Editor** | Untick to skip the intro while you work in the editor. Builds always play it. |

**Screen shape:** the canvas scales to the screen width from a 1284 × 2778 portrait reference, so
the logo is the same share of the width on every phone. For a landscape game, select the prefab
in your scene and set the **Canvas Scaler**'s **Match** to 1 (height) so the logo doesn't get too
big.

**On top of everything:** the canvas uses Sort Order 1000, so it covers your game's own UI.

**Unity's own splash:** the Unity logo plays *before* any scene. To have the game open on this
intro instead, untick **Project Settings → Player → Splash Image → Show Splash Screen**. Unity 6
allows that on every plan. In the same section, set the background to black so the launch screen
matches the intro.

## Changing the intro for one game

Files inside an installed package are read-only. To change the prefab itself (colours, images,
layout), right-click **SneakawayIntro** in the package and choose **Create → Prefab Variant**. That
saves a copy into your `Assets` folder that you can change freely. It still gets updates
to the parts you didn't change.

## Updating a project

1. In `Packages/manifest.json`, change the tag at the end of the URL to the new version, for
   example `#v1.0.0` → `#v1.1.0`.
2. Switch back to Unity; it downloads the new version.

If you installed without a tag, Unity sticks to the commit it first downloaded. That commit is
recorded in `Packages/packages-lock.json`. To get the newest `main`, remove the package's entry
from that file, or reinstall the package.

## Working on the package

The easiest way to change the intro is from a game that uses it, through a **local** install, so
you can see each change straight away:

1. Clone this repo next to the game's project, for example `…/Unity/sneakaway-intro`.
2. In the game's `Packages/manifest.json`, point the package at the folder instead of GitHub:

   ```json
   "com.sneakawaystudio.intro": "file:../../sneakaway-intro",
   ```

   The path is relative to the game's `Packages` folder. Package Manager can do the same: **+ →
   Install package from disk…** and pick this repo's `package.json`.
3. Edit the files under **Packages → Sneakaway Studio Intro** in Unity. They're editable in this
   mode, and the changes land in this repo.
4. Commit **every `.meta` file** Unity creates here. A package installed from git is read-only, so
   Unity can't make the `.meta` files itself, and any file without one is ignored.

To release a version:

1. Raise `"version"` in `package.json`, for example `1.0.0` → `1.1.0`, and add a line to
   `CHANGELOG.md`.
2. Commit, tag and push:

   ```sh
   git tag v1.1.0
   git push && git push --tags
   ```

3. In each game, change its manifest back to the GitHub URL with the new tag (see
   [Updating a project](#updating-a-project)).

## What's inside

```
package.json           name, version, Unity version, dependency on com.unity.ugui (UI)
Runtime/
  SneakawayIntro.cs    the animation (namespace SneakawayStudio)
  SneakawayStudio.Intro.asmdef
Prefabs/
  SneakawayIntro.prefab  Canvas → Background (black, tap to skip), Logo (Triangle, Letter, Url), FadeToGame
Art/
  sneakaway-triangle.png   the triangle outline, drawn by a radial fill
  sneakaway-s.png          the S
  sneakaway.studio.png     the URL, typed out by a horizontal fill
```

How the triangle draws: it's a single Image set to **Filled → Radial 360**, starting at the top and
going clockwise. As its fill amount goes from 0 to 1, the sweep reveals the outline around the
triangle: apex, right side, base, left side, like a pen.

How the URL types: it's an Image set to **Filled → Horizontal**, from the left. Instead of
sliding smoothly, its fill amount jumps from the right edge of one character to the next. Those
edges are the **Url Char Ends** list on the component (shares of the image's width). If you
replace `sneakaway.studio.png`, update that list, or empty it for a smooth wipe.

The three images share one scale: each Image is its sprite's pixel size × 0.417 (624 canvas units
for the 1497 px triangle), so the S and URL keep their size relative to the triangle.
