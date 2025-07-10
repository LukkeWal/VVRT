# Project Documentation

Welcome to the documentation for this Unity project! Hopefully this markdown file will help you through the main scripts, unity components, and conventions used in the codebase. Use the table of contents below to navigate to the topic you need.

# Table of Contents

1. [Main Scripts](#main-scripts)  
    - [Camera Controls](#camera-controls)  
    - [Scene Manager](#scene-manager)  
    - [Ray Managers & Ray Algorithms](#ray-managers--ray-algorithms)  
      - [Ray Tracing Algorithm & Ray Manager](#ray-tracing-algorithm--ray-manager)
        - [RayManager.cs](#raymanagercs)  
        - [UnityRayTracer.cs](#unityraytracercs)  
      - [Ray Casting Algorithm & Ray Caster Manager](#ray-casting-algorithm--ray-caster-manager)  
        - [RayCasterManager.cs](#raycastermanagercs)  
        - [UnityRayCaster.cs](#unityraycastercs)  
    - [UI Control Panel](#ui-control-panel)  
    - [Shaders](#shaders)  
      - [Ray tracing shaders](#ray-tracing-shaders)  
      - [Ray casting live preview shader](#ray-casting-live-preview-shader)  
2. [Adding New Voxel Grids](#adding-new-voxel-grids)  
3. [Editing Tutorials](#editing-tutorials)  
    - [Tutorial information location](#tutorial-information-location)
    - [Tutorial per level](#tutorial-per-level)
    - [Tasks](#tasks)
    - [Creating a non-skippable tasks](#creating-a-non-skippable-task)
4. [Running the Testing Suite](#running-the-testing-suite)  
5. [Code Style Recommendations](#code-style-reccomendations)  

# Main Scripts

This section describes the core scripts of the project, including how the camera, scene managers, ray algorithms, and UI handlers are structured.

## Camera Controls

The `CameraController` script is attached to the **Main Camera** and emulates Unity Editor–style controls (orbit, pan, zoom) across both mouse and touch input. It integrates with the `GlobalManager` for cursor changes and uses an `inputBlocker` overlay to prevent unintended clicks.

### `OnlyOneInputPicker()`

Here the controls for orbit, zoom and panning are set. It makes sure only one of these is active at a time, and that they do not activate when you are trying to interact with objects or the UI instead. It activates them by setting the `orbit`, `zoom` or `pan` bools to true. When orbit, zoom or panning is to true it calls their respective functions. In these functions are checks to see when the action should be ended by setting the respective bool back to false.

## Scene Manager
`RTSceneManager.cs` manages all objects in the scene. For example spawning new spheres and lights in a ray tracing scene. But also sending OnTranslationMode, OnRotationMode, and OnScaleMode events so other components can update themselves accordingly. Addittionally it controls which control panel is currenly visisble.

## Ray Managers & Ray Algorithms
So far there are two main algorithms: ray tracing and ray casting. Since ray tracing was first the ray casting was implemented by inheriting ray tracing classes and extending / overwriting them where needed.

### Ray Tracing Algorithm & Ray Manager

The ray tracing is managed in `RayManager.cs`, with the actual ray tracing algorithm being located in `UnityRayTracer.cs`. 

#### `RayManager.cs`
Here all the settings of the ray tracing algorithm are exposed and handled. It handles re-casting the rays when there has been a change in the scene, like the user changing a setting or an object being moved. It uses `UnityRayTracer.cs` to calculate the new rays. 

#### `UnityRayTracer.cs`
`Render()` sets up `Trace()` to generate a `RayTree` for every pixel of the virtual screen. This is a computationally expensive thing to do and is not feasible for the high resolution image. So there is `RenderImage()` which sets up one of multiple possible `ImageTraceFunc` for every pixel of the high resolution image, which only return the color for that pixel. 

### Ray Casting Algorithm & Ray Caster Manager

The ray casting uses inheritance to extend and sometimes overwrite the functionalty of the ray tracer, with `RayCasterManager.cs` being a child of `RayManager.cs` and `UnityRayCaster.cs` being a child of `UnityRayTracer.cs`. Therefore the structure of these classes is very similar to their parent scripts.

#### `RayCasterManager.cs`
This class inherits from [`RayManager.cs`](#raycastermanagercs) and is therefore very similar. It has added functionaly to handle Samples as well as Rays.

#### `UnityRayCaster.cs`
This class inherits from [`UnityRayTracer.cs`](#unityraycastercs) and is therefor very similar. However `RenderImage()` is heavily changed: is does not generate a high resolution image using an efficient ray casting function, instead it snaps a screenshot of the live preview from the perspective of the virtual camera instead. Therefore it is very imporant that the CPU ray casting algorithm used by `Render()` is the same as the GPU ray casting algorithm in the [Ray casting shader](#ray-casting-live-preview-shader). If you edit one take care to implement the same changes in the other.

## UI Control Panel

The most important UI script is the `ControlPanel.cs`. This is the main script controlling the settings menu on the right of the screen, through which users controll the level they are in. For each tab aka panel you have a `Properties` script and a linked button to open to said properties script. 

The `Properties` script specify what settings are in its panel, eg. sliders, checkboxes and buttons. Additionally it handles changes to these settings and makes sure to call the right (update) functions when needed.

## Shaders
There are a couple of custom shaders. These shaders can be attached to an object and fully controll what it looks like. 

### Ray tracing shaders
Ray tracing has 4 costom shaders: `LightShader`, `RayTracerShader`, `RayTracderShaderTransparent` and `RayTracerShaderTransparentFast`. All of these use the `Generalshader` as a baseline and add their own twist. Most of them are quite self explanatory and have clear naming.

### Ray casting live preview shader
For ray casting there is one complicated shader `Raycast` that creates the live preview of the volume. It has its own raycasting algorithm so the volume is raycast from the perspective of the user camera. This shader is tempramental, and was a bachelor thesis on its own. I want to write down the most notable caveats and pitfalls if you ever need to work / edit this shader:
 - Some editors (looking at you Rider...) may automatically insert a DX11 shader exlude. This breaks the shader. If you see the following line appear make sure to delete it: `#pragma exclude_renderers d3d11`
 - The shader uses a camera depth texture to get an idea of other objects that are in front, inside or behind the volume (like rays and samples that we visualize). It uses information to display the correct depth perception. The order in which Unity renders objects may exlude certain objects from this depth texture, and they may then be incorrectly visualized behind or in front of our shaded object. Therefore it is important that the object that this shader is attached to is placed in the `Transparent` section of the render pipeline, any other objects that are also in this `Transparent` pipeline will **not** be invisible to the camera depth texture.
 - This camera depth texture needs to be enabled by a seperate script. Make sure to attach `DepthTexMaker` to the same object as this shader.
 - Even though early ray termination is explained in the ray casting level as a more efficient approach. This shader does not have this functionality. This is because breaking one run of a loop in the main ray casting loop early causes a loop unrolling error. Luckily thus far this shader has never been observed to be a performance bottleneck.
 - Even though super sampling is explained and visualized in the scene, this shader does not support super sampling. Even when SS is enabled it will only shoot one ray per pixel
 - There is one magical number used for the `first` compositing method. This is an epsilon *e* that accepts density value *v +/- e* as close enough to the set density matching value. This value cannot be edited or viewed by the user.


# Adding New Voxel Grids

Adding a new voxelgrid is (currently) not very intuitive. I have listed the full steps below:

  * **Source your data** Find a new voxel grid and note its dataformat. If neccesary convert whatever format you have into one single file and place it in the StreamingAssets folder. Keep in mind that larger grids will take longer to load. On top of that there is limited space for grids (specifically their 3D Textures) in the web build becuase we need to stay under the 100MB file size limit that GitHub sets. Also make sure that it is allowed to use this grid for our project under the MIT license.
  * **In Make3dTex**
    - Add your grid in `CreateAllTextures()`, make sure to fill in the right size, encoding and if needed a scaling factor. If your file's encoding is not included you will have write it yourself.
    - Also add your grid `ExportAllCSVs()`
    - these functions are editor functions, they are available in the top bar of the unity editor, make sure to run Create3DTex, and if you want to use the CSV for creating a histogram image then run CreateCSV as well.
  * **Unity Editor folders**
    - Generate a histogram image for your volume. To do this you can create the csv as explained above, and then use the below R script, or you can create your own from scratch.
    ```R
      # make_histograms.R
      # 1) set this to the folder containing your CSV files
      data_dir <- "PATH TO YOUR CSV DIRECTORY"
      # 2) get all CSVs
      csv_files <- list.files(data_dir, pattern = "\\.csv$", full.names = TRUE)
      # 3) parameters for plotting
      threshold <- 0.07
      n_bins <- 256
      breaks <- seq(threshold, 1, length.out = n_bins + 1)
      # 4) loop over each file
      for (csv in csv_files) {
        # read and filter
        df <- read.csv(csv, header = TRUE)
        values <- df$value[df$value >= threshold]
        # prepare output filename
        png_file <- sub("\\.csv$", ".png", csv)
        png(filename = png_file, width = 800, height = 600)
        # draw the histogram
        hist(values,
        breaks = breaks,
        xlim = c(threshold, 1),
        axes = FALSE,
        xlab = "",
        ylab = "",
        main = "")
        # draw axes
        axis(1, at = c(threshold, seq(0.1,1,by=0.1)), # include 0.07 + then 0.1,0.2,...
        labels = c(sprintf("%.2f", threshold),
        sprintf("%.2f", seq(0.1,1,by=0.1))))
        axis(2) # default y-axis
        # finish writing the file
        dev.off()
        cat("Saved", png_file, "\n")
      }
    ```
    - `_Project` > `Ray Caster` > `Scripts` > `UI` paste the png for the histogram image of your volume here, then turn it into a sprite by clicking on your image then changing `Texture Type` to `Sprite (2D and UI)` and copy the rest of the settings from one of the other histogram sprites.
  * **In the Unity Editor Ray Casting Level**
    - `Caster Main Canvas` > `Hidable` > `Caster Control Panel` > `Panel` > `Scroll view` > `Content` > `Ray Caster Properties` Make sure to open caster control panel prefab if you want your changes applied to all levels
    - Under `VoxelGrid` > `VoxelGridDropdown` add a name for your grid to the dropdown
  * **In `RayCasterProperties.cs`**
    - Add your voxelgrid in `setVoxelDens()`
    - add the path to the histogram image sprite in `setHistogramImage()`
  * **In `VoxelGrid.cs`**
    - add a variable to cache your grid `private double[,,] myNewGrid = null;`
    - Add a name for your grid to the enum
    - Add a reccomended color lookup table for your grid in `getReccomendedColorLookupTable()`
    - Add a reccomended rotation in `setReccommendedRotation()`
    - add your grid to `SelectedVoxelGridDoneLoading()`
    - add the size of your grid to `setGridSizes()`
    - add your grid as an option to `LoadKnownGrid()`
    - add your grids filename to `LoadNewGrid` as well as saving its finished grid to the cashing variable at the end of this function. If needed you will also have to add your decoding algorithm here (I know this is all super duplicate with `Make3dTex`)
    - add the name of the 3Dtexture you made earlier to `loadGrid()`
  




# Editing Tutorials

## Tutorial information location

Tutorial information is stored under the `Game Manager` object. This prefab only spawns in the scene during play mode, it does not have to be added to levels manually. You can find it by simply searching its name in the editors file system. When you open this prefab you will see the `GlobalManager.cs` script, do not open the script code itself, instead inspect it through the unity editor and open the `Tutorial Tasks` dropdown.

## Tutorial per level
In the `Tutorial Tasks` dropdown you see a bunch of elements with a number. There is one element for each level, kind of like an enum. The order of the levels is the same as in the build window. 

## Tasks
For each level you have to specify how many tasks there are as well as how many of these are optional. A task is considered optional if you do not have to click on anything in the game to progress (you can simply click the next button on the tutorial).

## Creating a non-skippable task
If you want to create a task that requires completing a certain action before the user can progress you make sure the `Skippable` checkbox is **unchecked**. Then you have to give the task an Identifier. This identifier is attached to the action you want the user to perform to continue. 

You attach the identifier to an action by going to the UI element you want your user to interact with and adding an `On value Changed` event. Set this event to `Runtime Only`, the object to `TutorialManager.cs` and the function to `TutorialManager.CompleteTask`. Then fill in the Identifier of your non-skibbable task in the Identifier box. Whenever the user changes the value of this UI element, your identifier is send to the tutorial manager. If this identifier matches the tutorial tasks that the user is currently on, this tutorial task is set to completed. 

There are limitations to this tutorialmanager approach. The tutorial can only check if the user changed a value, it cannot check wheter the user changed it to a specific value. Keep this in mind when designing your tutorials.



# Running the Testing Suite

There is a testing suite for the project. It lives in `Assets` > `Tests`. You may need to install the unity test package through the package manager to run them, while you're at it you might want to download the test coverage package if you're interested. 

You can run the tests by going to `Window` > `Genearal` > `Test Runner`. There are currently two types of tests: Edit mode tests which are done without actually starting any level. and Play mode tests which are done by loading a level and running it as if a user had started it. 

If you are interested you can generate test coverage results from unity, it generates a index.html page for you to open with data on your test coverage.

When you are working on the project make sure you do not accidently break other peoples code. The test suite is only a tool to help you in this, it does not replace actually going though the game and testing things yourself.

**When you are finishing your project please check that all existing tests still run, and if they are broken try to figure out if this was intended or not and fix it. It would also be very helpfull to future students if you add some tests for your additions to VRT, and it helps ensure that your cool additions stay functional!**


# Code style reccomendations

As projects are regularly created in parallel they may need to be merged at later dates. To facilitate this it is reccomended to use inheritance instead of editing classes. Take inspiration from the ray casting level. We tried to keep to the following goals:
- Create a folder for your level that your new files, prefabs objects etc live in
- Instead of changing original classes in scripts, use inheritance to extend them and overwrite functions where needed. ofcourse you can make private classes and methods protected or public when needed. 
- These inherited classes can be dragged onto game objects in the unity editor to replace their parent classes. Use this in the level(s) you wish to edit.
- Keep in mind that changes to prefabs are applied accross all levels, while changes to a prefab in a level without entering prefab mode will only be applied to that level.
- When using inheriatnce to change UI like extending `RayTracerProperties.cs` you can remove unwanted UI elements by hiding them in the unity editor. Navigate to their game object and click the little eye icon. 

## Take inspiration from the ray casting level
We tried to keep to these code style reccomendations for the ray casting level. We created child classes and replaced their parent classes in the unity editor for the ray casting level only: 
- `UnityRayTracer.cs` -> `UnityRayCaster.cs`
- `RayManager.cs` -> `RayCasterManager.cs`
- `RayTracerProperties.cs` -> `RayCasterProperties.cs`
- etc.

Have a look at the Ray Casting level if you want to see how this works.


