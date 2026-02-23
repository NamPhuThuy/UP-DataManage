using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.DataManage
{
    [CreateAssetMenu(fileName = "IAPData", menuName = "Game/IAPData", order = 1)]
    public class IAPData : ScriptableObject
    {
        [Header("IAP Data")]
        [SerializeField] private IAPRecord[] records;
        public IAPRecord[] Records => records;
        
        private Dictionary<string, IAPRecord> _dictRecordByID;

        public Dictionary<string, IAPRecord> Data
        {
            get
            {
                if (_dictRecordByID == null)
                {
                    EnsureDictInit();
                }

                return _dictRecordByID;
            }
        }
        
        #region Private Methods

        private void EnsureDictInit()
        {
            if (_dictRecordByID != null) return;
            _dictRecordByID = new Dictionary<string, IAPRecord>(records?.Length ?? 0);
            if (_dictRecordByID == null) return;
            
            foreach (var r in records)
            {
                if (r == null)
                {
                    Debug.Log(message:$"IAPData.EnsureDictInit: null record found, skipping");
                    continue;
                }
                
                if (string.IsNullOrEmpty(r.BundleId))
                {
                    Debug.Log(message:$"IAPData.EnsureDictInit: bundleId is null, skipping");
                    continue;
                }
                _dictRecordByID[r.BundleId] = r; // last one wins if duplicates
            }
        }

        #endregion

        #region Public Methods

        public IAPRecord GetRecord(string bundleId)
        {
            EnsureDictInit();
            if (_dictRecordByID == null) return null;
            return _dictRecordByID.GetValueOrDefault(bundleId);
        }   

        #endregion
    }
    
    public enum IAPType
    {
        BUNDLE = 0,
        COIN = 1,
        NO_ADS = 2,
        BOOSTER = 3
    }
    
    [System.Serializable]
    public class IAPRecord
    {
        [SerializeField] private string bundleName;
        [SerializeField] private string bundleId;
        [SerializeField] private IAPType iapType;
        [SerializeField] private ProductType productType = ProductType.Consumable;
        [SerializeField] private Sprite titleImage;
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite subTitleImage;
        [SerializeField] private string description;
        [SerializeField] private string price;

        [SerializeField] private List<ResourceAmount> rewards;
        
        public string BundleName => bundleName;
        public string BundleId => bundleId;
        public IAPType Type => iapType;
        public ProductType ProductType => productType;
        public Sprite TitleImage => titleImage;

        public Sprite BackgroundSprite => backgroundSprite;

        public Sprite SubTitleImage => subTitleImage;
        public string Description => description;

        public string Price
        {
            get
            {
                return price;
            }

            set
            {
                price = value;
            }
        }
        public List<ResourceAmount> Rewards => rewards;
    }
}