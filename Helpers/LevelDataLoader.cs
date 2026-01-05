using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace NamPhuThuy.DataManage
{
    public class LevelDataLoader : MonoBehaviour
    {

        [Header("Native JSON Override")] [SerializeField]
        private TextAsset jsonFileOverride;

        [Header("Remote Config Override")] [SerializeField]
        private string remoteConfigKey = "level_data";


        #region Event

        public event Action<string> OnLoadLevelDataFailed;

        #endregion

        #region MonoBehaviour Callbacks

        private void OnDestroy()
        {
           
        }

        #endregion

        /*#region Firebase Remote Config

        /// <summary>
        /// Fetches and loads level data from Firebase Remote Config
        /// </summary>
        public void LoadDataFromFirebaseRemoteConfig()
        {
            StartCoroutine(LoadDataFromFirebaseRemoteConfigRoutine());
        }

        private IEnumerator LoadDataFromFirebaseRemoteConfigRoutine()
        {
            // Fetch and activate Remote Config
            Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);

            yield return new WaitUntil(() => fetchTask.IsCompleted);

            if (fetchTask.IsFaulted || fetchTask.IsCanceled)
            {
                string errorMsg = $"Failed to fetch Firebase Remote Config: {fetchTask.Exception}";
                Debug.LogError(errorMsg);
                OnLoadLevelDataFailed?.Invoke(errorMsg);
                yield break;
            }

            // Activate fetched data
            Task activateTask = FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
            yield return new WaitUntil(() => activateTask.IsCompleted);

            if (activateTask.IsFaulted || activateTask.IsCanceled)
            {
                string errorMsg = $"Failed to activate Firebase Remote Config: {activateTask.Exception}";
                Debug.LogError(errorMsg);
                OnLoadLevelDataFailed?.Invoke(errorMsg);
                yield break;
            }

            // Get JSON string from Remote Config
            string jsonText = FirebaseRemoteConfig.DefaultInstance.GetValue(remoteConfigKey).StringValue;

            if (string.IsNullOrEmpty(jsonText))
            {
                string errorMsg = $"Remote Config key '{remoteConfigKey}' is empty or not found. Using default data.";
                Debug.LogWarning(errorMsg);
                OnLoadLevelDataFailed?.Invoke(errorMsg);
                yield break;
            }

            // Load data from JSON
            LoadDataFromRemoteConfig(jsonText);
        }*/
        
        
        /// <summary>
        /// Async version using async/await
        /// </summary>
        /*public async Task LoadDataFromFirebaseRemoteConfigAsync()
        {
            try
            {
                // Fetch Remote Config
                await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
                
                // Activate fetched data
                bool activated = await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                
                if (!activated)
                {
                    Debug.LogWarning("Remote Config activation returned false");
                }

                // Get JSON string
                string jsonText = FirebaseRemoteConfig.DefaultInstance.GetValue(remoteConfigKey).StringValue;

                if (string.IsNullOrEmpty(jsonText))
                {
                    string errorMsg = $"Remote Config key '{remoteConfigKey}' is empty or not found";
                    Debug.LogWarning(errorMsg);
                    OnLoadLevelDataFailed?.Invoke(errorMsg);
                    return;
                }

                // Load data from JSON
                LoadDataFromRemoteConfig(jsonText);
            }
            catch (Exception e)
            {
                string errorMsg = $"Failed to load from Firebase Remote Config: {e.Message}";
                Debug.LogError(errorMsg);
                OnLoadLevelDataFailed?.Invoke(errorMsg);
            }
        }*/

        #region Load Data

        /*public IEnumerator LoadDataFromJson()
        {
            if (defaultLevelData == null)
            {
                Debug.LogError("Default LevelData reference is not assigned!");
                yield break;
            }

            // If JSON override exists, create a runtime copy
            if (jsonFileOverride != null)
            {
                _runtimeLevelData = ScriptableObject.CreateInstance<LevelData>();
                _runtimeLevelData.ImportFromJsonRuntime(jsonFileOverride.text);
                Debug.Log(
                    $"Loaded level data from override JSON file. Total levels: {_runtimeLevelData.GetTotalLevels()}");
            }

            yield return null;

            OnLoadLevelDataCompleted?.Invoke(_runtimeLevelData);
        }*/


        #endregion
    }
}