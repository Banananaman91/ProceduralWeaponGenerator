# ProceduralWeaponGenerator
Procedurally generated weapons providing all unique combinations of parts and variants, outputting one complete mesh prefab with original materials.
This is a unity editor tool.

# Feature List
- [Editor Window split view](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/main/Assets/Editor/EditorGUISplitView.cs)
- [Pre-calculated unique combinations](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/3bcb35888981c18fe0a70db5b5e22b03c90ad402/Assets/Editor/WeaponCreatorMethods.cs#L34)
- [Output preview utility](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/3bcb35888981c18fe0a70db5b5e22b03c90ad402/Assets/Editor/WeaponCreatorEditor.cs#L132)
- [Combined mesh preview with live update](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/3bcb35888981c18fe0a70db5b5e22b03c90ad402/Assets/Editor/WeaponCreatorEditor.cs#L198)
- [Generate unique weapons with combined mesh and materials](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/3bcb35888981c18fe0a70db5b5e22b03c90ad402/Assets/Editor/WeaponCreatorEditor.cs#L391)
- [Rarity level calculation from weapon parts](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/3bcb35888981c18fe0a70db5b5e22b03c90ad402/Assets/Editor/WeaponCreatorMethods.cs#L78)
- [Weapon stats composition](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/3bcb35888981c18fe0a70db5b5e22b03c90ad402/Assets/Editor/WeaponCreatorMethods.cs#L47)
- [Component copying](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/3bcb35888981c18fe0a70db5b5e22b03c90ad402/Assets/Editor/WeaponCreatorMethods.cs#L106)
- [Weapon Part Identification](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/main/Assets/Scripts/WeaponGenerator/WeaponAsset/PartId.cs)
- [Compiled Weapon Identification](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/main/Assets/Scripts/WeaponGenerator/WeaponAsset/WeaponId.cs)
- [Integrated Thumbnail Creation by Yasirkula](https://github.com/yasirkula/UnityRuntimePreviewGenerator)
- - [Thumbnail Editor Window preview thumbnails and edit settings](https://github.com/Banananaman91/ProceduralWeaponGenerator/blob/main/Assets/Editor/ThumbnailCreator.cs)

[Preview Video](https://www.youtube.com/watch?v=p9onq1j1mTk)

# [Install through release package](https://github.com/Banananaman91/ProceduralWeaponGenerator/releases)

# How to use

## WeaponMainBody
All weapons must be build from a main body part. This acts as the origin point for part assembling.
Assign WeaponMainBody component to your intended main body part.
On your main body part, setup child transform objects for the attachment points of your intended weapon parts.
Name these transform objects to the parts name. The system will use this naming to display.
On the WeaponMainBody component, attach these transforms to the parts list. The order of this will correspond to the order that your parts will be assigned.

## WeaponId & PartId
Created weapons can use an id system to assist with identification of weapons that are created with specific parts.
Assign a PartId Component to all weapon parts, including the main weapon body. This component creates a hash id from the parts name.
On generation, the outputted weapon will be assigned a WeaponId component containing a list of the different weapon part ID's.

## WeaponStatsContribution
When using the stats feature all weapon parts must contain a WeaponStatsContribution component. With this, simply fill out the stat types and values. These will all be added to the main weapon upon completion, where a new WeaponStats component will be added with all the compiled stats for that weapon based on the generated parts.

## WeaponRarityLevel
When using the rarity feature all weapon parts must contain a WeaponRarityLevel component. With this, simply select a rarity level for that weapon part.

## Generate Weapons
Right click in the project folder, and Create > Weapon Generator > Weapon Editor. This will open a new editor window to assign all of your weapon parts.
Once all weapon parts are assigned the amount of unique combinations that will be generated are displayed at the top right of this window.

The window itself is divided into three sections.

### Weapon Preview Area
The largest section found to the left of the window provides a preview area for viewing weapons that can be generated.
You can use the scroll wheel to zoom in and out from the weapon.
Use the Camera Transform coontrols to move the camera along the respective axis. THe camera will maintain the weapon as its target view.
Use the Light Transform controls to adjust the scene lighting for the preview.
If desired, input your own skip value. This value is used when cycling through the weapons.
Click the cycle button to automatically cycle through the weapons.
Click previous weapon to skip backwards, or next to skip forwards through the weapon combinations.

## Weapon Assembly Area
The top right section of the editor window provides the area for assigning weapon parts and variants.
Weapon > Parts is the list of all the different parts for the weapon. This includes the weapon main body, and the first part must be the weapon main body to work.

Weapon > Parts > Variant Name is automatically filled on assignment of the weapon main body. The names from your attachments will be used here. This is an assistive display that helps to inform the user which part corresponds to which attachment and has no bearing on the output if altered.

Weapon > Parts > Variant Pieces are all of the variations of each weapon part you wish to generate with. At least 1 is required per part. If desired, a slot can be left null if you want some variations of the weapons without that part.

Weapon > Parts > Detachable allows all variants of this weapon part to remain separate. The weapon generator combines all parts into a singular mesh with materials, but will ignore parts set as detachable and instead keep them as children of the resulting prefab.

The outputted weapons will generate all unique combinations that consist of one variant from each weapon part.

## Weapon Creator Settings
The bottom right section of the editor window provides an area for creator settings.

File name is the name of the file you want each weapon to be named. The outputted name will be your filename_i where i corresponds to the weapon number.
Meshes for weapons will also generated under the same name with an additional _M. e.g. weapon_1 && weapon_1_M.

Folder is the directory folder you want these weapons to save to. It will create the folder for you if required. The root of this will automatically start within your Assets folder. Providing a subfolder directory can be achieved by typing "/" e.g. Weapons/Guns will create a subfolder Guns inside of a folder Weapons.

Toggle stats feature allows you to enable compilation of weapon stats if desired. Turning this on will require all weapon parts to contain a WeaponStatsContribution component.

Toggle rarity feature allows you to enable complitation of weapon rarity if desired. Turning this on will require all weapon parts to contain a WeaponRarityLevel component.
- Enabling rarity feature reveals a calculation type option. This option allows you to change how the compiled rarity type of the resulting weapon is calculated.
- - Middle will find the middle value. e.g. if a weapon of 4 parts consists of common, common, uncommon, legendary then middle will result in either common or uncommon. If a weapon of 5 parts consists of common, common, uncommon, rare, legendary then the middle result will be uncommon.
- - Most Common will find the rarity value that is most common from all the rarity parts. If there is an equal amount of two different rarity values from all of the weapon parts then it will simply pick the one it found first. e.g. common, common, legendary, legendary will be common, whereas legendary, legendary, common, common will be legendary. 
