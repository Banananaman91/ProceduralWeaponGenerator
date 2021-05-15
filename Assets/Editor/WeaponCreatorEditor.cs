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
        private RarityCalculationType _rarityCalculationType;
        public Weapon _weapon = new Weapon();
        private bool _rarityToggle;
        private bool _statToggle;
        private string _errorString = "Error Display";
        private bool _isValid;
        private Dictionary<int, List<string>> tags => WeaponCreatorMethods.ListBuilder(_weapon);
        private List<string> AllCombos => WeaponCreatorMethods.GetCombos(tags);
        private int _comboCount;
        private int _comboDisplay;

        private PreviewRenderUtility _previewRenderUtility;
        private Transform _object;
        private float _backDistance = 10;
#pragma warning restore 0649
        
        //Wizard create window
        [MenuItem("Assets/Create/Weapon Generator/Weapon Editor", false, 1)]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            WeaponCreatorEditor window = (WeaponCreatorEditor)GetWindow(typeof(WeaponCreatorEditor));
            window.Show();
        }

        //Unity Editor Split View from Miguel12345 on Github
        EditorGUISplitView horizontalSplitView = new EditorGUISplitView (EditorGUISplitView.Direction.Horizontal);
        EditorGUISplitView verticalSplitView = new EditorGUISplitView (EditorGUISplitView.Direction.Vertical);
        
        private void OnGUI()
        {
            if (!focusedWindow && !mouseOverWindow) return;

            horizontalSplitView.BeginSplitView ();
            DrawWeaponPreviewArea();
            horizontalSplitView.Split ();
            verticalSplitView.BeginSplitView ();
            DrawWeaponPartsArea();
            verticalSplitView.Split ();
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
            GUILayout.Label("Number of unique combinations: " + _comboCount, EditorStyles.boldLabel);
            EditorWindow target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty weaponProperty = so.FindProperty("_weapon");
            EditorGUILayout.PropertyField(weaponProperty, true);
            so.ApplyModifiedProperties();
        }

        private void InitializeRenderUtility()
        {
            _previewRenderUtility = new PreviewRenderUtility();
            _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(0, 0, 0);
            _previewRenderUtility.camera.clearFlags = CameraClearFlags.Skybox;
            _previewRenderUtility.camera.nearClipPlane = 0.01f;
            _previewRenderUtility.camera.farClipPlane = 100f;
            _previewRenderUtility.camera.fieldOfView = 60f;
            _previewRenderUtility.lights[0].transform.rotation = FindDirectionalLights()[0].transform.rotation;
            _previewRenderUtility.lights[0].intensity = 1;
            for (int i = 1; i < _previewRenderUtility.lights.Length; ++i)
            {
                _previewRenderUtility.lights[i].intensity = 1;
                _previewRenderUtility.lights[i].transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
            }
        }

        private Light[] FindDirectionalLights() =>
            FindObjectsOfType<Light>().Where(light => light.type == LightType.Directional).ToArray();

        private void DrawWeaponPreviewArea()
        {
            if (_comboCount == 0) return;
            if (_previewRenderUtility == null) InitializeRenderUtility();
            var targetPos = _object ? _object.position : Vector3.zero;
            _previewRenderUtility.camera.transform.position = targetPos;
            _previewRenderUtility.camera.transform.Translate(new Vector3(0, 0, _backDistance));
            _previewRenderUtility.camera.transform.LookAt(targetPos);
            var boundaries = new Rect(0, 0, position.width / 2, position.height);
            _previewRenderUtility.BeginPreview(boundaries, GUIStyle.none);
            DrawPreviewMesh();
            var render = _previewRenderUtility.EndPreview();
            GUI.DrawTexture(new Rect(0, 0, boundaries.width, boundaries.height), render);

            GUILayout.Label("Weapon Preview Area", EditorStyles.largeLabel);

            if (mouseOverWindow && Event.current.type == EventType.ScrollWheel)
            {
                if (Event.current.delta.y > 0) _backDistance++;
                else _backDistance--;
            }
            
            if (_weapon.Parts.Count == 0) return;

            if (GUILayout.Button("Rotate Right", GUILayout.Width(100f)))
                _previewRenderUtility.camera.transform.RotateAround(targetPos, Vector3.up, Time.time);
            if (GUILayout.Button("Rotate Left", GUILayout.Width(100f)))
                _previewRenderUtility.camera.transform.RotateAround(targetPos, -Vector3.up, Time.time);
            if (GUILayout.Button("Rotate Up", GUILayout.Width(100f)))
                _previewRenderUtility.camera.transform.RotateAround(targetPos, Vector3.forward, Time.time);
            if (GUILayout.Button("Rotate Down", GUILayout.Width(100f)))
                _previewRenderUtility.camera.transform.RotateAround(targetPos, -Vector3.forward, Time.time);
            
            if (GUILayout.Button("Previous Skip 100", GUILayout.Width(120f))) _comboDisplay -= 100;
            if (GUILayout.Button("Previous Skip 10", GUILayout.Width(120f))) _comboDisplay -= 10;
            if (GUILayout.Button("Previous Weapon", GUILayout.Width(120f))) _comboDisplay--;
            if (GUILayout.Button("Next Weapon", GUILayout.Width(120f))) _comboDisplay++;
            if (GUILayout.Button("Next Skip 10", GUILayout.Width(120f))) _comboDisplay += 10;
            if (GUILayout.Button("Next Skip 100", GUILayout.Width(120f))) _comboDisplay += 100;
        }

        private void DrawPreviewMesh()
        {
            if (_comboDisplay > _comboCount - 1) _comboDisplay = 0;
            else if (_comboDisplay < 0) _comboDisplay = _comboCount - 1;
            if (_weapon.Parts.Count == 0) return;
            //split combination string into character array for use in indexing
            var idx = AllCombos[_comboDisplay].ToCharArray();
            var parts = idx.Select((t, i) => _weapon.Parts[i].VariantPieces[(int) Char.GetNumericValue(t)]).ToList();
            GameObject parent = null;
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

            //Assign all object materials
            var materials = parts.Select(t => t.GetComponent<MeshRenderer>().sharedMaterial).ToList();

            //get mesh filters of all objects
            var meshFilters = parts.Select(t => t.GetComponent<MeshFilter>()).ToList();

            for (int i = 0; i < materials.Count; i++)
            {
                var meshMatrix = parts[i].transform.localToWorldMatrix;
                _previewRenderUtility.DrawMesh(meshFilters[i].sharedMesh, meshMatrix, materials[i], 0);
            }
            _previewRenderUtility.camera.Render();
            _object = parent.transform;
        }

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

            //Generate guns from all combinations
            for (var g = 0; g < AllCombos.Count; g++)
            {
                GameObject parent = null;
                //split combination string into character array for use in indexing
                var idx = AllCombos[g].ToCharArray();
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
        }

        private void OnDisable()
        {
            _previewRenderUtility?.Cleanup();
        }
    }
}
