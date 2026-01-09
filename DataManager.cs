using System;
using System.Collections;
using System.IO;
using MoreMountains.Tools;
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
                    Debug.LogWarning($"[Singleton] Instance of {typeof(DataManager)} is already destroyed. Returning null.");
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

        protected void Awake()
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
        }

        public void OnDestroy()
        {
            Debug.Log(message:$"DataManager.OnDestroy()");
            _instance = null;
            OnExtinction();
            StopAllCoroutines();
        }
        
        public static bool Exists => _instance != null;
        
        protected virtual void OnInitialize()
        {
            // Override in derived classes for custom initialization
        }
        public virtual void OnExtinction() { }

        private void Start()
        {
            // DebugLogger.Log();
            // yield return null;
            
            _playerDataPath = $"{Application.persistentDataPath}/player.{DataConst.FILES_EXTENSION}";
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.FILES_EXTENSION}";
            _progressDataPath = $"{Application.persistentDataPath}/progress.{DataConst.FILES_EXTENSION}";
            _inventoryDataPath = $"{Application.persistentDataPath}/inventory.{DataConst.FILES_EXTENSION}";

            // yield return StartCoroutine(LoadData());
            LoadData();
            /*if (isUseRemoteConfig)
            {
                yield return StartCoroutine(levelDataLoader.LoadDataFromJson());
            }*/
        }

        private void OnEnable()
        {
            MMEventManager.RegistCurrentEvents(this);
        }

        private void OnDisable()
        {
            MMEventManager.UnregistCurrentEvents(this);
        }

        /*
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveData();
        }
        */

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            SaveData();
        }

        #endregion

        #region Save Methods

        public void SavePlayerData()
        {
            _playerDataPath = $"{Application.persistentDataPath}/player.{DataConst.FILES_EXTENSION}";

            //example: origin = "{"name":"NamTrinh","level":12,"currentExpPoint":31.0}"
            string data = JsonUtility.ToJson(cachedPlayerData);
            
            // Got problem when save/load with encrypt
            // origin = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);

            File.WriteAllText(_playerDataPath, data);
            // DebugLogger.Log(message:$"Save data success: {data}");
        }

        public void SaveSettingsData()
        {
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPlayerSettingsData);
            // origin = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            File.WriteAllText(_settingsDataPath, origin);
        }
        
        public void SaveProgressData()
        {
            _progressDataPath = $"{Application.persistentDataPath}/progress.{DataConst.FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPProgressData);
            // origin = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            File.WriteAllText(_progressDataPath, origin);
        }
        
        public void SaveInventoryData()
        {
            _inventoryDataPath = $"{Application.persistentDataPath}/inventory.{DataConst.FILES_EXTENSION}";
            // PInventoryData.SyncDictToListForSave();
            
            string origin = JsonUtility.ToJson(cachedPInventoryData);
            // origin = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            File.WriteAllText(_inventoryDataPath, origin);
        }

        #endregion

        #region  Load Methods

        private void LoadPlayerData()
        {
            // DebugLogger.Log();
            _playerDataPath = $"{Application.persistentDataPath}/player.{DataConst.FILES_EXTENSION}";
            if (File.Exists(_playerDataPath))
            {
                try
                {
                    /*
                 File.ReadAllText(_savePath) reads from disk
                 Disk operations are significantly slower than memory operations
                 Can cause frame drops if called during gameplay
                 */
                    string data = File.ReadAllText(_playerDataPath);

                    //Large string operations can be memory and CPU intensive
                    
                    // Got problem when save/load with encrypt
                    // data = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    cachedPlayerData = JsonUtility.FromJson<PlayerData>(data);
                    // DebugLogger.Log(message:$"Load data success: {data}");
                }
                catch (Exception e)
                {
                    Debug.LogError(message: $"Error: {e.Message}");
                    ResetPlayerData();
                }
            }
            else
                ResetPlayerData();

            // yield return null;
        }

        private void LoadSettingsData()
        {
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.FILES_EXTENSION}";
            if (File.Exists(_settingsDataPath))
            {
                try
                {
                    string data = File.ReadAllText(_settingsDataPath);
                    // data = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    cachedPlayerSettingsData = JsonUtility.FromJson<PlayerSettingsData>(data);
                    
                }
                catch (Exception e)
                {
                    // Debug.Log(e.Message);
                    ResetSettingsData();
                }
            }
            else
                ResetSettingsData();

            // yield return null;
        }
        
        private void LoadProgressData()
        {
            Debug.Log(message: $"DataManager.LoadProgressData()");
            _progressDataPath = $"{Application.persistentDataPath}/progress.{DataConst.FILES_EXTENSION}";
            if (File.Exists(_progressDataPath))
            {
                Debug.Log(message:$"DataManager.LoadProgressData() - {_progressDataPath} exist, load data");
                try
                {
                    string data = File.ReadAllText(_progressDataPath);
                    // data = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    cachedPProgressData = JsonUtility.FromJson<PProgressData>(data);
                    _isProgressDataLoaded = true;
                }
                catch (Exception e)
                {
                    Debug.Log(message:$"DataManager.LoadProgressData() - Exception: {e.Message}, reset progress data");
                    ResetProgressData();
                }
            }
            else
            {
                Debug.Log(message: $"DataManager.LoadProgressData() - {_progressDataPath} not exist, reset progress data");
                ResetProgressData();
            }

            // yield return null;
        }
        
        private void LoadInventoryData()
        {
            _inventoryDataPath = $"{Application.persistentDataPath}/inventory.{DataConst.FILES_EXTENSION}";
            if (File.Exists(_inventoryDataPath))
            {
                try
                {
                    string data = File.ReadAllText(_inventoryDataPath);
                    // data = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    cachedPInventoryData = JsonUtility.FromJson<PInventoryData>(data);
                }
                catch (Exception e)
                {
                    // Debug.Log(e.Message);
                    ResetInventoryData();
                }
            }
            else
                ResetInventoryData();

            // yield return null;
        }
        

        #endregion

        #region Reset Methods
        
        public void ResetPlayerData()
        {
            cachedPlayerData = new PlayerData();
            SavePlayerData();
        }

        public void ResetSettingsData()
        {
            cachedPlayerSettingsData = new PlayerSettingsData();
            SaveSettingsData();
        }
        
        public void ResetProgressData()
        {
            Debug.Log(message:$"DataManager.ResetProgressData()");
            cachedPProgressData = new PProgressData();
            SaveProgressData();
        }
        
        public void ResetInventoryData()
        {
            cachedPInventoryData = new PInventoryData();
            SaveInventoryData();
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
            Debug.Log(message:$"DataManager.Reset()");
            ResetPlayerData();
            ResetSettingsData();
            ResetProgressData();
            ResetInventoryData();
        }

        public void SaveData()
        {
            Debug.Log(message:$"DataManager.SaveData()");
            SavePlayerData();
            SaveSettingsData();
            SaveProgressData();
            SaveInventoryData();
        }

        public void LoadData()
        {
            Debug.Log(message:$"DataManager.LoadData()");
            /*yield return StartCoroutine(LoadPlayerData());
            yield return StartCoroutine(LoadResourceData());
            yield return StartCoroutine(LoadSettingsData());
            yield return StartCoroutine(LoadProgressData());*/
            LoadPlayerData();
            LoadSettingsData();
            LoadProgressData();
            LoadInventoryData();
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
            var n = PlayerData.GetBoosterNum(type);
            PlayerData.SetBoosterNum(type, Mathf.Max(0, n - 1));
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