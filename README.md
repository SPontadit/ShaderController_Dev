# Shader Controller for Unity

This is the **development repository** of the Shader Controller package available for Unity.

It contains samples and all the code source used to develop the package.

<br>

The Shader Controller allows you to **quickly and easily change or animate any properties** of a shader without additional coding.

It generates Monobehaviour Components based on shaders. Controllers are split into 2 categories: Post Process and Shader.

<br>

## Get the package

> + Clone or download the package repository and add it to the "Packages" folder of your Unity project: [https://github.com/SPontadit/ShaderController](https://github.com/SPontadit/ShaderController)

> + Add the package from the Package Manager with the git repository URL: [https://github.com/SPontadit/ShaderController.git](https://github.com/SPontadit/ShaderController.git)

## Create a controller

+ Select a shader asset into your Project Window
+ Right click on it
+ Create > Shader Controller > Post Process or Shader Controller

## Use a controller

+ Add a controller to a GameObject\* like any other component
+ **Animate or change** properties directly on inspector without anything more to do
+ **By default, material used is not saved!**
+ To use an existing material or save the current, check "Save and Use Material" toggle
+ To return to default mode, uncheck "Save and Use Material" toggle

If you changed shader property, just clicked to "Update Controller Button" on the component and the controller will be updated without any losses.

It is recommended to not modify generated controller scripts since all the changes made to a script will be overwritten if you update it.

\* Post Process controllers must be added to a GameObject that **has a Camera component**.<br>
Shader controllers must be added to a GameObject that **has a Renderer component**.

<br>
