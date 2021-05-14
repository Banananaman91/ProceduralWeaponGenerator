using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaponGenerator.WeaponAsset;
using WeaponGenerator.WeaponAssetRarity;
using WeaponGenerator.WeaponAssetStats;

namespace Editor
{
    public class WeaponCreatorStatsRarity : ScriptableWizard
    {
                #pragma warning disable 0649
        [SerializeField] private string _fileName;
        [SerializeField] private string _saveFolder;
        [SerializeField] private RarityCalculationType _rarityCalculationType;
        [SerializeField] private Weapon _weapon = new Weapon();

        private List<string> _combinations = new List<string>();
        private Dictionary<int, List<string>> tags => WeaponCreatorMethods.ListBuilder(_weapon);
        private List<string> AllCombos => WeaponCreatorMethods.GetCombos(tags);
#pragma warning restore 0649

        //Wizard create window
        [MenuItem("Assets/Create/Weapon Generator/Weapon Assets With Stats & Rarity", false, 1)]
        private static void CreateWizard()
        {
            DisplayWizard<WeaponCreatorStatsRarity>("Create Gun Collection With Stats", "Create");
        }

        #region WeaponValidate
        //update information as values in wizard change
        private void OnWizardUpdate()
        {
            //reset create button validity
            isValid = false;
            
            //if no weapon parts, don't continue
            if (_weapon.Parts == null || _weapon.Parts.Count == 0) return;

            //reset weapon parts names
            foreach (var part in _weapon.Parts)
            {
                part.VariantName = "";
            }
            
            //remove previous collection
            //tags.Clear();

            //rebuild collection to generate new combinations
            //ListBuilder();

            //regenerate all combinations
            _combinations = AllCombos;

            //display amount of combinations
            //TODO validate combinations count is not too high
            helpString = "Amount of unique combinations: " + _combinations.Count;
            
            //validate weapon main body and parts
            ValidateWeaponMainBody();
        }

        private void ValidateWeaponMainBody()
        {
            //if the first weapon part has nothing added, inform user of what is needed
            if (_weapon.Parts[0].VariantPieces == null || _weapon.Parts[0].VariantPieces.Count <= 0)
            {
                errorString = "The first weapon part must contain an object with WeaponMainBody component";
                isValid = false;
                return;
            }
            
            var mainBodies = new List<WeaponMainBody>();
            
            //Validate none of the weapon pieces are empty
            if (_weapon.Parts[0].VariantPieces.Any(t => t == null))
            {
                errorString = "All weapon pieces in the first weapon part must contain an object with WeaponMainBody component";
                isValid = false;
                return;
            }

            //validate all of the weapon pieces contain the WeaponMainBody component
            foreach (var gunMono in _weapon.Parts[0].VariantPieces.Select(piece => piece.GetComponent<WeaponMainBody>()))
            {
                if (!gunMono)
                {
                    errorString = "All weapon pieces in the first weapon part must contain an object with WeaponMainBody component";
                    isValid = false;
                    break;
                }
                mainBodies.Add(gunMono);
            }

            //prevent continuation if all of the weapon pieces don't contain the WeaponMainBody component
            if (mainBodies.Count != _weapon.Parts[0].VariantPieces.Count) return;
            
            //validate all of the weapon pieces contain the WeaponStatsContribution component
            var partsCount = 0;
            var mainStats = new List<WeaponStatsContribution>();
            foreach (var t1 in _weapon.Parts.SelectMany(t => t.VariantPieces))
            {
                partsCount++;
                var weaponMono = t1.GetComponent<WeaponStatsContribution>();
                if (weaponMono) mainStats.Add(weaponMono);
            }

            if (mainStats.Count != partsCount)
            {
                errorString = "All weapon pieces and variants must contain an object with WeaponStatsContribution component";
                isValid = false;
                return;
            }
            
            //validate all of the weapon pieces contain the WeaponStatsContribution component
            partsCount = 0;
            var mainRarity = new List<WeaponRarityLevel>();
            foreach (var t1 in _weapon.Parts.SelectMany(t => t.VariantPieces))
            {
                partsCount++;
                var weaponMono = t1.GetComponent<WeaponRarityLevel>();
                if (weaponMono) mainRarity.Add(weaponMono);
            }

            if (mainRarity.Count != partsCount)
            {
                errorString = "All weapon pieces and variants must contain an object with WeaponRarityLevel component";
                isValid = false;
                return;
            }

            var names = new List<string>();

            //collect all of the attachment names for the rest of the weapon parts
            for (var i = 0; i < mainBodies[0].AttachmentNames.Count; i++)
            {
                for (var j = 1; j < mainBodies.Count; j++)
                {
                    //validate that all pieces with WeaponMainBody contain the same amount of attachments, as well as contain the same attachments
                    if (mainBodies[0].AttachmentNames.Count == mainBodies[j].AttachmentNames.Count &&
                        i <= mainBodies[j].AttachmentNames.Count &&
                        mainBodies[0].AttachmentNames[i] == mainBodies[j].AttachmentNames[i]) continue;
                    errorString = "All weapon attachments for main body pieces must match";
                    isValid = false;
                    return;
                }
                names.Add(mainBodies[0].AttachmentNames[i]);
            }

            //prevent continuation if all attachments aren't accounted for and inform the user
            if (_weapon.Parts.Count - 1 != names.Count)
            {
                errorString = "Incorrect amount of pieces created compared to weapon main body attachments";
                isValid = false;
                return;
            }

            //Set all weapon parts names from the attachment names. First weapon part must be the main body
            for (var i = 0; i < _weapon.Parts.Count; i++)
            {
                _weapon.Parts[i].VariantName = i == 0 ? "Weapon Main Body" : names[i - 1];
            }

            //Validate all of the weapon parts have at least one item to choose from
            for (var i = 1; i < _weapon.Parts.Count; i++)
            {
                if (_weapon.Parts[i].VariantPieces != null && _weapon.Parts[i].VariantPieces.Count != 0) continue;
                errorString = "Weapon parts are missing pieces";
                isValid = false;
                return;
            }

            if (_fileName == "")
            {
                errorString = "Please input file name";
                isValid = false;
                return;
            }
            
            if (_saveFolder == "")
            {
                errorString = "Please input folder directory";
                isValid = false;
                return;
            }
            
            //If made it this far, all validation checks have passed
            errorString = "";
            isValid = true;
        }
        #endregion

        public void OnWizardCreate()
        {
            //Don't continue if we don't have parts
            if (_weapon.Parts == null || _weapon.Parts.Count == 0) return;

            //Don't continue if there is no file name
            if (_fileName == "") return;

            //Don't continue if there is no folder
            if (_saveFolder == "") return;

            //Open utility window to create save path for folder
            var path = "Assets/" + _saveFolder;

            //Don't continue if save window was closed without a path
            if (path.Length == 0) return;

            //Generate guns from all combinations
            for (var g = 0; g < _combinations.Count; g++)
            {
                GameObject parent = null;
                //split combination string into character array for use in indexing
                var idx = _combinations[g].ToCharArray();
                var parts = idx.Select((t, i) => (GameObject) PrefabUtility.InstantiatePrefab(_weapon.Parts[i].VariantPieces[(int) Char.GetNumericValue(t)])).ToList();
                //instantiate each part to be used in building the weapon

                //find the part with GunMainBody to use as parent object
                foreach (var part in parts.Where(part => part.GetComponent<WeaponMainBody>()))
                {
                    parent = part;
                }

                //if no parent found, previous validation checks failed. Throw error for user.
                if (!parent)
                {
                    throw new NullReferenceException("One weapon part requires WeaponMainBody script to be set up to create parent object");
                }

                //get main body component
                var WeaponMono = parent.GetComponent<WeaponMainBody>();

                //Generate attachments
                for (var i = 0; i < parts.Count; i++)
                {
                    if (parts[i] == parent) continue;
                    //Set its transform position and child it to the weapons main body
                    parts[i].transform.parent = parent.transform;
                    parts[i].transform.position = WeaponMono.AttachmentPoints[i - 1].position;
                    parts[i].name = WeaponMono.AttachmentPoints[i - 1].name;
                }

                //Destroy all child attachment points as they are no longer needed
                for (var i = WeaponMono.AttachmentPoints.Count - 1; i >= 0 ; i--)
                {
                    if (PrefabUtility.IsPartOfPrefabInstance(WeaponMono.AttachmentPoints[i])) PrefabUtility.UnpackPrefabInstance(parent, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    
                    DestroyImmediate(WeaponMono.AttachmentPoints[i].gameObject);
                }

                //Assign all object materials
              
                var materials = parts.Where((t, i) => !_weapon.Parts[i].Detachable).Select(t => t.GetComponent<MeshRenderer>().sharedMaterial).ToList();

                parent.GetComponent<MeshRenderer>().sharedMaterials = new Material[materials.Count];
                parent.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();

                var meshFilters = parts.Where((t, i) => !_weapon.Parts[i].Detachable).Select(t => t.GetComponent<MeshFilter>()).ToList();
                
                //get mesh filters of all objects

                //Generate all combine instances for meshes
                var combineInstance = new CombineInstance[meshFilters.Count];
                for (var i = 0; i < meshFilters.Count; i++)
                {
                    combineInstance[i].mesh = meshFilters[i].sharedMesh;
                    combineInstance[i].transform = meshFilters[i].transform.localToWorldMatrix;
                }

                //Generate final combine mesh and apply to main object
                var finalMesh = new Mesh();
                finalMesh.CombineMeshes(combineInstance, false);
                parent.GetComponent<MeshFilter>().sharedMesh = finalMesh;

                //Create folder if it doesn't exist
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                
                //Save mesh file
                var meshFile = "/" + _fileName + "_" + g + "_M" + ".asset";
                AssetDatabase.CreateAsset(finalMesh, path + meshFile);
                
                //Generate Weapon Stats
                var weaponStat = parent.AddComponent<WeaponStats>();
                weaponStat.Stats = WeaponCreatorMethods.GenerateWeaponStats(parts);
                
                //Generate Weapon Rarity
                var weaponRarity = parent.GetComponent<WeaponRarityLevel>();
                weaponRarity.Rarity = WeaponCreatorMethods.GenerateWeaponRarity(parts, _rarityCalculationType);

                //Destroy all of the child objects as they are no longer needed
                for (var i = parts.Count - 1; i > 0; i--)
                {
                    if (_weapon.Parts[i].Detachable)
                    {
                        var rarityComp = parts[i].GetComponent<WeaponRarityLevel>();
                        DestroyImmediate(rarityComp);
                        var statComp = parts[i].GetComponent<WeaponStatsContribution>();
                        DestroyImmediate(statComp);
                        if (PrefabUtility.IsPartOfPrefabInstance(parts[i])) PrefabUtility.UnpackPrefabInstance(parts[i], PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        continue;
                    }
                    DestroyImmediate(parts[i].gameObject);
                }
                
                //Destroy WeaponMainBody component as it is no longer needed
                DestroyImmediate(WeaponMono);
                DestroyImmediate(parent.GetComponent<WeaponStatsContribution>());

                //create file name for path
                var file = "/" + _fileName + "_" + g + ".prefab";

                //Save as new prefab
                PrefabUtility.SaveAsPrefabAsset(parent, path + file);
                //delete previous instantiation as it is no longer needed
                DestroyImmediate(parent);
            }
        }
    }
}
