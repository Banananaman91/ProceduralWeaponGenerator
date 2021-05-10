# ProceduralWeaponGenerator
Procedurally generated weapons providing all unique combinations of parts and variants, outputting one complete mesh prefab with original materials.
This is a unity editor tool.

# How to use

## WeaponMainBody
All weapons must be build from a main body part. This acts as the origin point for part assembling.
Assign WeaponMainBody component to your intended main body part.
On your main body part, setup child transform objects for the attachment points of your intended weapon parts.
Name these transform objects to the parts name. The system will use this naming to display.
On the WeaponMainBody component, attach these transforms to the parts list. The order of this will correspond to the order that your parts will be assigned.

## Generate Weapons
Right click in the project folder, and Create > Weapon Generator. This will open a window to assign all of your weapon parts.
Once all weapon parts are assigned the amount of unique combinations that will be generated are displayed at the top of this window.

File name is the name of the file you want each weapon to be name. The outputted name will be your filename_i where i corresponds to the weapon number.
Meshes for weapons will also generated under the same name with an additional _M.

Folder is the directory folder you want these weapons to save to. It will create the folder for you if required.

Weapon > Parts is the list of all the different parts for the weapon. This includes the weapon main body, and the first part must be the weapon main body to work.

Weapon > Parts > Variant Name is automatically filled on assignment of the weapon main body. The names from your attachments will be used here. This is an assistive display that helps to inform the user which part corresponds to which attachment.

Weapon > Parts > Variant Pieces are all of the variations of each weapon part you wish to generate with. At least 1 is required per part.

The outputted weapons will generation combinations that consist of one variant from each weapon part.
