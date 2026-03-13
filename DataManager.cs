using System;
using System.Collections;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.DataManage
{
    /*
ScriptableObjects for:
- Static game configuration
- Level design data
- Item/weapon/character definitions
- Anything that needs Unity asset references

JSON for:
- Player progress/saves
- User settings
- Dynamic content updates
- Anything that needs to change after build
*/

    public partial class DataManager : MonoBehaviour
    {
        private bool isCheatMode = true;

        public bool IsCheatMode => isCheatMode;
        public bool isGrantRewardsAfterLevel;
        public int directPlayLevel = 4;

        #region Private Fields

        private Coroutine _saveDebounce;

        private static DataManager _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        public static DataManager Ins
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning(
                        $"[Singleton] Instance of {typeof(DataManager)} is already destroyed. Returning null.");
                    return null;
                }

                /*lock (_lock)
                {*/
                if (_instance == null)
                {
                    // Try to find existing instance in scene
                    _instance = FindFirstObjectByType<DataManager>();

                    if (_instance == null)
                    {
                        // Create new GameObject with the singleton component
                        GameObject singletonObj = new GameObject($"{typeof(DataManager).Name} (Singleton)");
                        _instance = singletonObj.AddComponent<DataManager>();

                        Debug.Log($"[Singleton] Created new instance of {typeof(DataManager)}");
                    }
                }
                //}

                return _instance;
            }
        }

        #endregion

        #region MonoBehaviour Callbacks

        protected async void Awake()
        {
            if (_instance == null)
            {
                _instance = this as DataManager;
                DontDestroyOnLoad(gameObject);
                OnInitialize();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(DataManager)} detected. Destroying.");
                Destroy(gameObject);
            }


            _ = LoadAlbumDataAsync();
        }

        public void OnDestroy()
        {
            Debug.Log(message: $"DataManager.OnDestroy()");
            _instance = null;
            OnExtinction();
            StopAllCoroutines();
        }

        public static bool Exists => _instance != null;

        protected virtual void OnInitialize()
        {
            // Override in derived classes for custom initialization
        }

        public virtual void OnExtinction()
        {
        }

        private void Start()
        {
            // DebugLogger.Log();
            // yield return null;

            _playerDataPath = $"{Application.persistentDataPath}/player.{DataConst.FILES_EXTENSION}";
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.FILES_EXTENSION}";
            _progressDataPath = $"{Application.persistentDataPath}/progress.{DataConst.FILES_EXTENSION}";
            _inventoryDataPath = $"{Application.persistentDataPath}/inventory.{DataConst.FILES_EXTENSION}";
            _albumDataPath = $"{Application.persistentDataPath}/album.{DataConst.FILES_EXTENSION}";

            // yield return StartCoroutine(LoadData());
            LoadData();
            /*if (isUseRemoteConfig)
            {
                yield return StartCoroutine(levelDataLoader.LoadDataFromJson());
            }*/
        }


        // [FIX 5.1] Save on pause — OS can kill backgrounded apps without calling OnApplicationQuit
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveData();
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            SaveData();
        }

        #endregion

        #region Save Methods

        /// <summary>
        /// [FIX 7.1] Atomic file write: writes to a .tmp file first, then replaces the target.
        /// Prevents data corruption if the app is killed mid-write (low battery, OS kill).
        ///   Here's what happens with direct write:                                                                                                                                                                                            
        /// File.WriteAllText("inventory.json", data)                                                                                                                                                                                         
        /// 1. OS opens inventory.json and truncates it to 0 bytes                                                                                                                                                                            
        /// 2. OS starts writing new data...                                                                                                                                                                                                  
        /// 3. App killed here → inventory.json is now empty or half-written                                                                                                                                                                  
        /// 4. Next launch: JsonUtility.FromJson fails → ResetInventoryData() → all player data gone
        ///
        ///  Here's what happens with the temp-file approach:                                                                                                                                                                                  
        ///  File.WriteAllText("inventory.json.tmp", data)   // step 1                                                                                                                                                                         
        ///  File.Move("inventory.json", "inventory.json.bak") // step 2
        ///  File.Move("inventory.json.tmp", "inventory.json")  // step 3                                                                                                                                                                      
        ///  If app is killed during:                                                                                                                                                                                                          
        ///  - Step 1 → .tmp is corrupted, but inventory.json is untouched and valid                                                                                                                                                           
        ///  - Step 2 → .json was renamed to .bak, .tmp is complete — both are valid copies                                                                                                                                                    
        ///  - Step 3 → .bak exists with old data, .json is the new data — both valid  
        /// </summary>
        private void SafeWriteJson(string path, string json)
        {
            string tempPath = path + ".tmp"; // "inventory.json.tmp"
            try
            {
                File.WriteAllText(tempPath, json);

                if (File.Exists(path))
                {
                    string backupPath = path + ".bak"; //inventory.json.bak
                    // Delete old backup if it exists, then rename current → backup, temp → current
                    if (File.Exists(backupPath)) 
                        File.Delete(backupPath);
                    File.Move(sourceFileName:path, destFileName:backupPath); // File.Move("inventory.json", "inventory.json.bak")
                    File.Move(tempPath, path); // File.Move("inventory.json.tmp", "inventory.json")
                    File.Delete(backupPath); // File.Delete("inventory.json.bak"); 
                }
                else
                {
                    File.Move(tempPath, path); // File.Move("inventory.json.tmp", "inventory.json")
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"DataManager.SafeWriteJson failed for {path}: {e.Message}");
                // Fallback: direct write is better than losing data entirely
                try
                {
                    File.WriteAllText(path, json);
                }
                catch (Exception fallbackEx)
                {
                    Debug.LogError($"DataManager.SafeWriteJson fallback also failed: {fallbackEx.Message}");
                }
            }
        }

        public void SaveSettingsData()
        {
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPSettingsData);
            SafeWriteJson(_settingsDataPath, origin);
        }

        public void SaveProgressData()
        {
            _progressDataPath = $"{Application.persistentDataPath}/progress.{DataConst.FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPProgressData);
            SafeWriteJson(_progressDataPath, origin);
        }

        public void SaveInventoryData()
        {
            _inventoryDataPath = $"{Application.persistentDataPath}/inventory.{DataConst.FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPInventoryData);
            SafeWriteJson(_inventoryDataPath, origin);
        }

        public void SavePAlbumData()
        {
            _albumDataPath = $"{Application.persistentDataPath}/album.{DataConst.FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPAlbumData);
            SafeWriteJson(_albumDataPath, origin);
        }

        #endregion

        #region Load Methods

        /// <summary>
        /// Companion to SafeWriteJson — recovers from interrupted writes.
        /// Checks for leftover .bak and .tmp files from SafeWriteJson and restores valid data.
        ///
        /// Recovery scenarios:
        /// 1. path exists                        → normal case, read it
        /// 2. path missing, .bak exists           → killed during step 2-3 of SafeWriteJson, restore from .bak
        /// 3. path missing, .tmp exists           → killed during step 2-3, .tmp has the newest data, promote it
        /// 4. path missing, .bak + .tmp both exist → .bak is the safe fallback (step 2 completed, step 3 didn't)
        /// 5. nothing exists                      → first launch, return null so caller can reset
        /// </summary>
        private string SafeReadJson(string path)
        {
            string tmpPath = path + ".tmp";
            string bakPath = path + ".bak";

            // Normal case: main file exists
            if (File.Exists(path))
            {
                // Clean up leftover temp files from a previous successful write
                if (File.Exists(tmpPath)) File.Delete(tmpPath);
                if (File.Exists(bakPath)) File.Delete(bakPath);
                
                
                   /*
                  File.ReadAllText(path) reads from disk
                  Disk operations are significantly slower than memory operations
                  Can cause frame drops if called during gameplay
                  */
                return File.ReadAllText(path);
            }

            // Main file is missing — try to recover
            // If .bak exists, SafeWriteJson completed step 2 (rename .json → .bak) but not step 3
            if (File.Exists(bakPath))
            {
                Debug.LogWarning($"DataManager.SafeReadJson: Recovering {path} from .bak (interrupted save)");
                File.Move(bakPath, path);
                if (File.Exists(tmpPath)) File.Delete(tmpPath);
                return File.ReadAllText(path);
            }

            // If only .tmp exists, SafeWriteJson completed step 1 (write to .tmp) but main file
            // was somehow lost. The .tmp should have valid newest data.
            if (File.Exists(tmpPath))
            {
                Debug.LogWarning($"DataManager.SafeReadJson: Recovering {path} from .tmp");
                File.Move(tmpPath, path);
                return File.ReadAllText(path);
            }

            // Nothing exists — first launch
            return null;
        }

        private void LoadSettingsData()
        {
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.FILES_EXTENSION}";
            try
            {
                string data = SafeReadJson(_settingsDataPath);
                if (data != null)
                    cachedPSettingsData = JsonUtility.FromJson<PSettingsData>(data);
                else
                    ResetSettingsData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"DataManager.LoadSettingsData() failed: {e.Message}");
                ResetSettingsData();
            }
        }

        private void LoadProgressData()
        {
            Debug.Log(message: $"DataManager.LoadProgressData()");
            _progressDataPath = $"{Application.persistentDataPath}/progress.{DataConst.FILES_EXTENSION}";
            try
            {
                string data = SafeReadJson(_progressDataPath);
                if (data != null)
                {
                     //Large string operations can be memory and CPU intensive
                    
                    // Got problem when save/load with encrypt
                    // data = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    cachedPProgressData = JsonUtility.FromJson<PProgressData>(data);
                    _isProgressDataLoaded = true;
                }
                else
                {
                    Debug.Log($"DataManager.LoadProgressData() - no save file found, resetting");
                    ResetProgressData();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"DataManager.LoadProgressData() - Exception: {e.Message}, reset progress data");
                ResetProgressData();
            }
        }

        private void LoadInventoryData()
        {
            _inventoryDataPath = $"{Application.persistentDataPath}/inventory.{DataConst.FILES_EXTENSION}";
            try
            {
                string data = SafeReadJson(_inventoryDataPath);
                if (data != null)
                    cachedPInventoryData = JsonUtility.FromJson<PInventoryData>(data);
                else
                    ResetInventoryData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"DataManager.LoadInventoryData() failed: {e.Message}");
                ResetInventoryData();
            }
        }

        private void LoadPAlbumData()
        {
            _albumDataPath = $"{Application.persistentDataPath}/album.{DataConst.FILES_EXTENSION}";
            try
            {
                string data = SafeReadJson(_albumDataPath);
                if (data != null)
                    cachedPAlbumData = JsonUtility.FromJson<PAlbumData>(data);
                else
                    ResetPAlbumData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"DataManager.LoadPAlbumData() failed: {e.Message}");
                ResetPAlbumData();
            }
        }

        #endregion

        #region Reset Methods

        public void ResetSettingsData()
        {
            cachedPSettingsData = new PSettingsData();
            SaveSettingsData();
        }

        public void ResetProgressData()
        {
            Debug.Log(message: $"DataManager.ResetProgressData()");
            cachedPProgressData = new PProgressData();
            SaveProgressData();
        }

        public void ResetInventoryData()
        {
            cachedPInventoryData = new PInventoryData();
            SaveInventoryData();
        }

        public void ResetPAlbumData()
        {
            cachedPAlbumData = new PAlbumData();
            SavePAlbumData();
        }

        #endregion

        #region Data Management

        public void MarkDirty()
        {
            if (_saveDebounce != null) StopCoroutine(_saveDebounce);
            _saveDebounce = StartCoroutine(SaveAfterDelay(DataConst.SAVE_INTERVAL));
        }

        private IEnumerator SaveAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SaveData();
            _saveDebounce = null;
        }

        public void ResetData()
        {
            Debug.Log(message: $"DataManager.Reset()");
            ResetSettingsData();
            ResetProgressData();
            ResetInventoryData();
            ResetPAlbumData();
        }

        public void SaveData()
        {
            Debug.Log(message: $"DataManager.SaveData()");
            SaveSettingsData();
            SaveProgressData();
            SaveInventoryData();
            SavePAlbumData();
        }

        public void LoadData()
        {
            Debug.Log(message: $"DataManager.LoadData()");
            /*yield return StartCoroutine(LoadPlayerData());
            yield return StartCoroutine(LoadResourceData());
            yield return StartCoroutine(LoadSettingsData());
            yield return StartCoroutine(LoadProgressData());*/
            LoadSettingsData();
            LoadProgressData();
            LoadInventoryData();
            LoadPAlbumData();
        }

        #endregion

        /*
         #region Generic Save/Load/Reset Methods

        private void SaveMutableData<T>(T data, string fileName)
        {
            string path = $"{Application.persistentDataPath}/{fileName}.{DataConst.DATA_FILES_EXTENSION}";

            //example: origin = "{"name":"NamTrinh","level":12,"currentExpPoint":31.0}"
            string origin = JsonUtility.ToJson(data);
            // string encrypted = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            // File.WriteAllText(path, encrypted);

            File.WriteAllText(path, origin);
        }

        private T LoadMutableData<T>(string fileName, Func<T> resetAction) where T : class
        {
            string path = $"{Application.persistentDataPath}/{fileName}.{DataConst.DATA_FILES_EXTENSION}";

            if (File.Exists(path))
            {
                try
                {
                    /*
                 File.ReadAllText(_savePath) reads from disk
                 Disk operations are significantly slower than memory operations
                 Can cause frame drops if called during gameplay
                 #1#
                    string data = File.ReadAllText(path);

                    //Large string operations can be memory and CPU intensive
                    // string decrypted = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    return JsonUtility.FromJson<T>(data);
                }
                catch (Exception e)
                {
                    DebugLogger.LogWarning($"DataManager.LoadMutableData() {e}");
                    return resetAction();
                }
            }

            DebugLogger.LogWarning($"DataManager.LoadMutableData() - file not exist: {path}");
            return resetAction();
        }

        private void ResetMutableData<T>(ref T cachedData, ref bool isLoaded, string fileName, Action<T> postResetAction = null) where T : new()
        {
            isLoaded = false;
            cachedData = new T();
            isLoaded = true;

            postResetAction?.Invoke(cachedData);
            SaveMutableData(cachedData, fileName);
        }
        #endregion
         */

        #region Event Listen

        private void MinusBoosterAmount(BoosterType type)
        {
            ResourceAmount amount = new ResourceAmount()
            {
                resourceType = ResourceType.BOOSTER,
                boosterType = type,
                amount = -1
            };

            /*if (PlayerData.TryApplyReward(reward))
            {
                DebugLogger.Log($"DataManager.MinusBoosterAmount() done", Color.black);
            }

            else
            {
                DebugLogger.Log($"DataManager.MinusBoosterAmount() failed", Color.black);
            }*/
            var n = PInventoryData.GetBoosterNum(type);
            PInventoryData.SetBoosterNum(type, Mathf.Max(0, n - 1));
        }

        #endregion
    }


/*#if UNITY_EDITOR
    [CustomEditor(typeof(DataManager), true)]
    public class DataManagerInspector : Editor
    {
        private DataManager dataManager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            dataManager = (DataManager)target;

            //Interact with dynamic datas
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Data")) dataManager.SaveData();
            if (GUILayout.Button("Load Data")) dataManager.LoadData();
            if (GUILayout.Button("Reset Data")) dataManager.ResetData();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Player Data")) dataManager.SavePlayerData();
            if (GUILayout.Button("Save Settings Data")) dataManager.SaveSettingsData();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Player Data")) dataManager.ResetPlayerData();
            if (GUILayout.Button("Reset Settings Data")) dataManager.ResetSettingsData();

            GUILayout.EndHorizontal();
        }
    }
#endif*/
}
