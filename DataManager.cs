using System;
using System.Collections;
using System.IO;
using MoreMountains.Tools;
using NamPhuThuy.Common;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.Data
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

    public partial class DataManager : Singleton<DataManager>
    {
        #region Private Fields

        private Coroutine _saveDebounce;

        #endregion
        
        #region MonoBehaviour Callbacks

        protected override void Awake()
        {
            // DebugLogger.Log();
            base.Awake();
        }

        public override void OnDestroy()
        {
            DebugLogger.Log();
            base.OnDestroy();
        }

        private IEnumerator Start()
        {
            // DebugLogger.Log();
            yield return null;
            
            _playerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";

            yield return StartCoroutine(LoadData());

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
            SaveData();
        }

        #endregion

        #region Private Methods

        public void SavePlayerData()
        {
            DebugLogger.Log();
            _playerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";

            //example: origin = "{"name":"NamTrinh","level":12,"currentExpPoint":31.0}"
            string data = JsonUtility.ToJson(cachedPlayerData);
            
            // Got problem when save/load with encrypt
            // origin = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);

            File.WriteAllText(_playerDataPath, data);
            // DebugLogger.Log(message:$"Save data success: {data}");
        }

        public void SaveSettingsData()
        {
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPlayerSettingsData);
            string encrypted = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            File.WriteAllText(_settingsDataPath, encrypted);
        }

        private IEnumerator LoadPlayerData()
        {
            // DebugLogger.Log();
            _playerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";
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
                    DebugLogger.Log(message: $"Error: {e.Message}");
                    ResetPlayerData();
                }
            }
            else
                ResetPlayerData();

            yield return null;
        }

        private IEnumerator LoadSettingsData()
        {
            _settingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";
            if (File.Exists(_settingsDataPath))
            {
                try
                {
                    string data = File.ReadAllText(_settingsDataPath);
                    string decrypted = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    cachedPlayerSettingsData = JsonUtility.FromJson<PlayerSettingsData>(decrypted);
                }
                catch (Exception e)
                {
                    // Debug.Log(e.Message);
                    ResetSettingsData();
                }
            }
            else
                ResetSettingsData();

            yield return null;
        }

        public void ResetPlayerData()
        {
            DebugLogger.Log();
            cachedPlayerData = new PlayerData()
            {
                health = DataConst.MAX_HEALTH
            };
            SavePlayerData();
        }

        public void ResetSettingsData()
        {
            cachedPlayerSettingsData = new PlayerSettingsData();
            SaveSettingsData();
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
            yield return YieldHelper.GetRealtime(delay);
            SaveData();
            _saveDebounce = null;
        }

        public void ResetData()
        {
            ResetPlayerData();
            ResetSettingsData();
        }

        public void SaveData(bool postData = true)
        {
            SavePlayerData();
            SaveSettingsData();
        }

        public IEnumerator LoadData()
        {
            // DebugLogger.Log();
            yield return StartCoroutine(LoadPlayerData());
            yield return StartCoroutine(LoadSettingsData());
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

#if UNITY_EDITOR
    [CustomEditor(typeof(DataManager), true)]
    public class DataManagerCheckedInspector : Editor
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
#endif
}