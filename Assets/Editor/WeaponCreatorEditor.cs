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
    public class WeaponCreatorEditor : EditorWindow
    {
#pragma warning disable 0649
        private string _fileName;
        private string _saveFolder;
        private string _errorString = "Error Display";
        private RarityCalculationType _rarityCalculationType = RarityCalculationType.Middle;
        public Weapon _weapon = new Weapon();
        private bool _rarityToggle;
        private bool _statToggle;
        private bool _isValid;
        private int _comboCount;
        private int _comboDisplay;
        private int _skipAmount = 1;
        private float _lightIntensity = 1f;
        private Color _lightColor = Color.white;
        private Vector3 _lightRotation;
        private Vector3 _cameraPosition = new Vector3(10, 0, 0);
        private Transform _object;
        private PreviewRenderUtility _previewRenderUtility;
        private Dictionary<int, List<string>> tags => WeaponCreatorMethods.ListBuilder(_weapon);
        private List<string> AllCombos => WeaponCreatorMethods.GetCombos(tags);
#pragma warning restore 0649
        
        //Wizard create window
        [MenuItem("Assets/Create/Weapon Generator/Weapon Editor", false, 1)]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            WeaponCreatorEditor window = (WeaponCreatorEditor)GetWindow(typeof(WeaponCreatorEditor));
            window.Show();
        }

        #region GUI
        EditorGUISplitView horizontalSplitView = new EditorGUISplitView (EditorGUISplitView.Direction.Horizontal);
        EditorGUISplitView verticalSplitView = new EditorGUISplitView (EditorGUISplitView.Direction.Vertical);
        
        private void OnGUI()
        {
            if (!focusedWindow && !mouseOverWindow) return;
            
            horizontalSplitView.BeginSplitView ();
            //Draw first GUI area for weapon preview
            DrawWeaponPreviewArea();
            horizontalSplitView.Split ();
            verticalSplitView.BeginSplitView ();
            //Draw second GUI area for assembling weapon parts
            DrawWeaponPartsArea();
            verticalSplitView.Split ();
            //Draw third GUI area for core weapon creation settings
            DrawSettingsArea();
            verticalSplitView.EndSplitView ();
            horizontalSplitView.EndSplitView ();
            Repaint();
            
            //reset create button validity
            _isValid = false;
            
            //if no weapon parts, don't continue
            if (_weapon.Parts == null || _weapon.Parts.Count == 0) return;
        
            _comboCount = AllCombos.Count;
            
            //reset weapon parts names
            foreach (var part in _weapon.Parts)
            {
                part.VariantName = "";
            }

            ValidateWeapon();
        }

        private void DrawSettingsArea()
        {
            GUILayout.Label("Weapon File Name", EditorStyles.boldLabel);
            _fileName = EditorGUILayout.TextField(_fileName, GUILayout.Width(200f));
            
            GUILayout.Label("Folder Directory", EditorStyles.boldLabel);
            _saveFolder = EditorGUILayout.TextField(_saveFolder, GUILayout.Width(200f));
            
            GUILayout.Label("Use Rarity and Stat features", EditorStyles.boldLabel);
            _statToggle = EditorGUILayout.Toggle("Toggle stats feature", _statToggle);
            _rarityToggle = EditorGUILayout.Toggle("Toggle rarity feature", _rarityToggle);
            if (_rarityToggle)
                _rarityCalculationType = (RarityCalculationType) EditorGUILayout.EnumPopup("Rarity calculation type", _rarityCalculationType, GUILayout.Width(300f));

            if (!_isValid) EditorGUILayout.HelpBox(_errorString, MessageType.Error);
            else if(GUILayout.Button("Generate Weapons")) CreateWeapons();
        }

        private void DrawWeaponPartsArea()
        {
            //Create serialized property for weapon variable and display, including children
            GUILayout.Label("Number of unique combinations: " + _comboCount, EditorStyles.boldLabel);
            var target = this;
            var so = new SerializedObject(target);
            var weaponProperty = so.FindProperty("_weapon");
            EditorGUILayout.PropertyField(weaponProperty, true);
            //ensure modifications to variables by user are applied
            so.ApplyModifiedProperties();
        }

        private void InitializeRenderUtility()
        {
            //create render utility for weapon preview area
            _previewRenderUtility = new PreviewRenderUtility();
            //Reset rotation
            _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(0, 0, 0);
            //Set background to skybox
            _previewRenderUtility.camera.clearFlags = CameraClearFlags.Skybox;
            //Ensure clipping planes are suitable
            _previewRenderUtility.camera.nearClipPlane = 0.01f;
            _previewRenderUtility.camera.farClipPlane = 100f;
            //Set field of view to default
            _previewRenderUtility.camera.fieldOfView = 60f;
        }

        //finds directional lights. Not even sure this actually finds lights? This script doesn't exist in the scene
        private Light[] FindDirectionalLights() =>
            FindObjectsOfType<Light>().Where(light => light.type == LightType.Directional).ToArray();

        private void DrawWeaponPreviewArea()
        {
            //If we have no combinations, don't draw
            if (_comboCount == 0) return;
            //Initialize render utility with default settings if we have none
            if (_previewRenderUtility == null) InitializeRenderUtility();
            //Adjust lighting
            _previewRenderUtility.lights[0].transform.rotation = Quaternion.Euler(_lightRotation);
            _previewRenderUtility.lights[0].color = _lightColor;
            _previewRenderUtility.lights[0].intensity = _lightIntensity;

            //find target, if we have no object then look at zero
            var targetPos = _object ? _object.position : Vector3.zero;
            //get screen boundaries. We use half the width as the split view covers half of the preview by default.
            var boundaries = new Rect(0, 0, position.width / 2, position.height);
            //Begin preview and create mesh to draw as texture
            _previewRenderUtility.BeginPreview(boundaries, GUIStyle.none);
            DrawPreviewMesh();
            var render = _previewRenderUtility.EndPreview();
            GUI.DrawTexture(new Rect(0, 0, boundaries.width, boundaries.height), render);

            
            //Apply GUI components for preview area over the texture
            GUILayout.Label("Weapon Preview Area", EditorStyles.largeLabel);

            if (mouseOverWindow && Event.current.type == EventType.ScrollWheel)
            {
                if (Event.current.delta.y > 0) _previewRenderUtility.camera.transform.Translate(0, 0, 1, Space.Self);
                else _previewRenderUtility.camera.transform.Translate(0, 0, -1, Space.Self);
                _cameraPosition = _previewRenderUtility.camera.transform.position;
            }
            
            //This check prevents crashing when parts is reduced back to zero
            if (_weapon.Parts.Count == 0) return;

            
            _cameraPosition = EditorGUILayout.Vector3Field("Camera Position", _cameraPosition, GUILayout.Width(300f));
            //Adjust camera position
            _previewRenderUtility.camera.transform.position = _cameraPosition;
            //Ensure camera looks at target object
            _previewRenderUtility.camera.transform.LookAt(targetPos);
            
            //Light controls
            _lightRotation = EditorGUILayout.Vector3Field("Light Rotation", _lightRotation, GUILayout.Width(300f));
            GUILayout.Label("Light Intensity");
            _lightIntensity = EditorGUILayout.FloatField(_lightIntensity, GUILayout.Width(50f));
            GUILayout.Label("Light Colour");
            _lightColor = EditorGUILayout.ColorField(_lightColor, GUILayout.Width(100f));
            
            //Skip through weapon previews
            EditorGUILayout.LabelField("Value for next/previous weapon");
            _skipAmount = EditorGUILayout.IntField(_skipAmount,GUILayout.Width(100f));
            if (GUILayout.Button("Previous Weapon", GUILayout.Width(120f))) _comboDisplay -= _skipAmount;
            if (GUILayout.Button("Next Weapon", GUILayout.Width(120f))) _comboDisplay += _skipAmount;
        }

        private void DrawPreviewMesh()
        {
            //Ensure combo display is reset when cycling through weapon preview combinations
            if (_comboDisplay > _comboCount - 1) _comboDisplay = 0;
            else if (_comboDisplay < 0) _comboDisplay = _comboCount - 1;
            //This check prevents crashing when weapon parts are reduced back to zero
            if (_weapon.Parts.Count == 0) return;
            //split combination string into character array for use in indexing
            var idx = AllCombos[_comboDisplay].ToCharArray();

            var parts = idx.Select((t, i) => _weapon.Parts[i].VariantPieces[(int) Char.GetNumericValue(t)]).ToList();
            
            //find the part with GunMainBody to use as parent object
            var parent = parts[0];

            //if no parent found, previous validation checks failed. Throw error for user.
            if (!parent)
            {
                throw new NullReferenceException("One weapon part requires WeaponMainBody script to be set up to create parent object");
            }

            //Assign all object materials
            var materials = parts.Select(t => t != null ? t.GetComponent<MeshRenderer>().sharedMaterial : null).ToList();

            //get mesh filters of all objects
            var meshFilters = parts.Select(t => t != null ? t.GetComponent<MeshFilter>() : null).ToList();

            var weaponMono = parent.GetComponent<WeaponMainBody>();

            for (var i = 0; i < materials.Count; i++)
            {
                if (materials[i] == null || meshFilters[i] == null) continue;
                var trans = i == 0
                    ? parts[i].transform
                    : weaponMono.AttachmentPoints[i - 1].transform;
                _previewRenderUtility.DrawMesh(meshFilters[i].sharedMesh, trans.position, parts[i].transform.localScale, parts[i].transform.rotation, materials[i], 0, null, trans, true);
            }
            _previewRenderUtility.camera.Render();
            _object = parent.transform;
        }
        #endregion

        #region WeaponCreatorValidation
        private void ValidateWeapon()
        {
                        //if the first weapon part has nothing added, inform user of what is needed
            if (_weapon.Parts[0].VariantPieces == null || _weapon.Parts[0].VariantPieces.Count <= 0)
            {
                _errorString = "The first weapon part must contain an object with WeaponMainBody component";
                _isValid = false;
                return;
            }
            
            var mainBodies = new List<WeaponMainBody>();
            
            //Validate none of the weapon pieces are empty
            if (_weapon.Parts[0].VariantPieces.Any(t => t == null))
            {
                _errorString = "All weapon pieces in the first weapon part must contain an object with WeaponMainBody component";
                _isValid = false;
                return;
            }

            //validate all of the weapon pieces contain the WeaponMainBody component
            foreach (var gunMono in _weapon.Parts[0].VariantPieces.Select(piece => piece.GetComponent<WeaponMainBody>()))
            {
                if (!gunMono)
                {
                    _errorString = "All weapon pieces in the first weapon part must contain an object with WeaponMainBody component";
                    _isValid = false;
                    break;
                }
                mainBodies.Add(gunMono);
            }

            //prevent continuation if all of the weapon pieces don't contain the WeaponMainBody component
            if (mainBodies.Count != _weapon.Parts[0].VariantPieces.Count) return;

            if (_statToggle)
            {
                //validate all of the weapon pieces contain the WeaponStatsContribution component
                var partsCount = 0;
                var mainStats = new List<WeaponStatsContribution>();
                foreach (var t1 in _weapon.Parts.SelectMany(t => t.VariantPieces))
                {
                    if (!t1) continue;
                    partsCount++;
                    var weaponMono = t1.GetComponent<WeaponStatsContribution>();
                    if (weaponMono) mainStats.Add(weaponMono);
                }

                if (mainStats.Count != partsCount)
                {
                    _errorString =
                        "All weapon pieces and variants must contain an object with WeaponStatsContribution component";
                    _isValid = false;
                    return;
                }
            }

            if (_rarityToggle)
            {
                //validate all of the weapon pieces contain the WeaponStatsContribution component
                var partsCount = 0;
                var mainRarity = new List<WeaponRarityLevel>();
                foreach (var t1 in _weapon.Parts.SelectMany(t => t.VariantPieces))
                {
                    if (!t1) continue;
                    partsCount++;
                    var weaponMono = t1.GetComponent<WeaponRarityLevel>();
                    if (weaponMono) mainRarity.Add(weaponMono);
                }

                if (mainRarity.Count != partsCount)
                {
                    _errorString =
                        "All weapon pieces and variants must contain an object with WeaponRarityLevel component";
                    _isValid = false;
                    return;
                }
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
                    _errorString = "All weapon attachments for main body pieces must match";
                    _isValid = false;
                    return;
                }
                names.Add(mainBodies[0].AttachmentNames[i]);
            }

            //prevent continuation if all attachments aren't accounted for and inform the user
            if (_weapon.Parts.Count - 1 != names.Count)
            {
                _errorString = "Incorrect amount of pieces created compared to weapon main body attachments";
                _isValid = false;
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
                _errorString = "Weapon parts are missing pieces";
                _isValid = false;
                return;
            }

            if (_fileName == "")
            {
                _errorString = "Please input file name";
                _isValid = false;
                return;
            }
            
            if (_saveFolder == "")
            {
                _errorString = "Please input folder directory";
                _isValid = false;
                return;
            }
            
            //If made it this far, all validation checks have passed
            _errorString = "";
            _isValid = true;
        }
        #endregion

        #region WeaponCreator
        private void CreateWeapons()
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
            
            var destroyObject = new GameObject();
            destroyObject.AddComponent<ToDestroy>();
            destroyObject.AddComponent<MeshRenderer>();
            destroyObject.GetComponent<MeshRenderer>().sharedMaterial =
                _weapon.Parts[0].VariantPieces[0].GetComponent<MeshRenderer>().sharedMaterial;
            destroyObject.AddComponent<MeshFilter>();
            destroyObject.name = "Destroy Object";

            //Generate guns from all combinations
            for (var g = 0; g < AllCombos.Count; g++)
            {
                //split combination string into character array for use in indexing
                var idx = AllCombos[g].ToCharArray();
                var parts = idx.Select((t, i) => _weapon.Parts[i].VariantPieces[(int) Char.GetNumericValue(t)] ? (GameObject) PrefabUtility.InstantiatePrefab(_weapon.Parts[i].VariantPieces[(int) Char.GetNumericValue(t)]) : Instantiate(destroyObject)).ToList();
                //instantiate each part to be used in building the weapon

                //find the part with GunMainBody to use as parent object
                var parent = parts[0];

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
                    if (!parts[i] || parts[i] == parent) continue;
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

                //get mesh filters of all objects
                var meshFilters = parts.Where((t, i) => !_weapon.Parts[i].Detachable).Select(t => t.GetComponent<MeshFilter>()).ToList();
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

                if (_statToggle)
                {
                    //Generate Weapon Stats
                    var weaponStat = parent.AddComponent<WeaponStats>();
                    weaponStat.Stats = WeaponCreatorMethods.GenerateWeaponStats(parts);
                }

                if (_rarityToggle)
                {
                    //Generate Weapon Rarity
                    var weaponRarity = parent.GetComponent<WeaponRarityLevel>();
                    weaponRarity.Rarity = WeaponCreatorMethods.GenerateWeaponRarity(parts, _rarityCalculationType);
                }

                //Destroy all of the child objects as they are no longer needed
                for (var i = parts.Count - 1; i > 0; i--)
                {
                    if (_weapon.Parts[i].Detachable)
                    {
                        if (parts[i].GetComponent<ToDestroy>())
                        {
                            DestroyImmediate(parts[i]);
                            continue;
                        }
                        if (_rarityToggle)
                        {
                            var rarityComp = parts[i].GetComponent<WeaponRarityLevel>();
                            DestroyImmediate(rarityComp);
                        }

                        if (_statToggle)
                        {
                            var statComp = parts[i].GetComponent<WeaponStatsContribution>();
                            DestroyImmediate(statComp);
                        }

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
            DestroyImmediate(destroyObject);
        }
        #endregion

        private void OnDisable()
        {
            //clean up render utility
            _previewRenderUtility?.Cleanup();
        }
    }
}
