using System.Collections;
using NamPhuThuy.DataManage;
using TMPro;
using UnityEngine;


namespace NamPhuThuy.DataManage
{
    public partial class DataManager
    {
        #region Components

        [Header("Components")]
        [SerializeField] private LevelDataLoader levelDataLoader;
        [SerializeField] private TMP_FontAsset defaultFont;
        public TMP_FontAsset DefaultFont => defaultFont;

        #endregion
        
        #region Mutable Datas

        [Header("Mutable Datas")] 
        private string _playerDataPath;
        private string _settingsDataPath;
        private string _progressDataPath;
        private string _inventoryDataPath;
        

        private bool _isPlayerDataLoaded;
        private bool _isSettingsDataLoaded;
        private bool _isProgressDataLoaded;
        private bool _isInventoryDataLoaded;
      
        

        

        [SerializeField] private PProgressData cachedPProgressData;
        
        public PProgressData PProgressData
        {
            get
            {
                if (!_isProgressDataLoaded)
                {
                    // StartCoroutine(LoadProgressData());
                    LoadProgressData();
                    _isProgressDataLoaded = true;
                }

                return cachedPProgressData;
            }
        }


        [SerializeField] private PSettingsData cachedPSettingsData;

        public PSettingsData PSettingsData
        {
            get
            {
                if (!_isSettingsDataLoaded)
                {
                    // StartCoroutine(LoadSettingsData());
                    LoadSettingsData();
                    _isSettingsDataLoaded = true;
                }

                return cachedPSettingsData;
            }
        }
        
        [SerializeField] private PInventoryData cachedPInventoryData;
        
        public PInventoryData PInventoryData
        {
            get
            {
                if (!_isInventoryDataLoaded)
                {
                    // StartCoroutine(LoadProgressData());
                    LoadProgressData();
                    _isInventoryDataLoaded = true;
                }

                return cachedPInventoryData;
            }
        }
        
        #endregion
        
        #region Immutable Datas


        [Header("Immutable Datas")] 
        [Header("Level Data")] 
        [SerializeField] private bool isUseRemoteConfig;
      
      

        [SerializeField] private IAPData iapData_Shop;

        public IAPData IAPDataShop
        {
            get
            {
                if (iapData_Shop == null)
                {
                    iapData_Shop = Resources.Load<IAPData>("IAPData - Shop");
                }

                return iapData_Shop;
            }
        }

        [SerializeField] private ResourceData resourceData;

        public ResourceData ResourceData
        {
            get
            {
                if (resourceData == null)
                {
                    resourceData = Resources.Load<ResourceData>("ResourceData");
                }

                return resourceData;
            }
        }
        
        [SerializeField] private BoosterData boosterData;

        public BoosterData BoosterData
        {
            get
            {
                if (boosterData == null)
                {
                    boosterData = Resources.Load<BoosterData>("BoosterData");
                }

                return boosterData;
            }
        }

        [SerializeField] private EventRewardData eventRewardData;

        public EventRewardData EventRewardData
        {
            get
            {
                if (eventRewardData == null)
                {
                    eventRewardData = Resources.Load<EventRewardData>("EventRewardData");
                }

                return eventRewardData;
            }
        }

        #endregion

    }
}