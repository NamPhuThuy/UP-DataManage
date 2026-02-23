using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace NamPhuThuy.DataManage
{
    [Serializable]
    public class PProgressData 
    {
        [SerializeField] private int levelId;
        public int LevelId
        {
            get => levelId;
            set
            {
                levelId = value;
                levelId = Math.Max(0, value);
                Debug.Log(message:$"levelId: {levelId}");
                DataManager.Ins.MarkDirty();
            }
        }

        public void IncreaseCurrentLevel()
        {
            Debug.Log(message:$"PProgressData.IncreaseCurrentLevel()");
            LevelId++;
        }

        [SerializeField] private bool isFirstTimePlay = true;

        public bool IsFirstTimePlay
        {
            get => isFirstTimePlay;
            set
            {
                isFirstTimePlay = value;
                DataManager.Ins.MarkDirty();
            }
        }
        
        [SerializeField] private bool isVIP;
        public bool IsVIP
        {
            get => isVIP;
            set
            {
                isVIP = value;
                DataManager.Ins.MarkDirty();
            }
        }

        [SerializeField] private bool isAdsRemoved;
        public bool IsAdsRemoved 
        {
            get => isAdsRemoved;
            set
            {
                isAdsRemoved = value;
                DataManager.Ins.MarkDirty();
            }
        }
        
        public void RemoveAds()
        {
            IsAdsRemoved = true;
        }

        [SerializeField] private int currentBackgroundId;

        public int CurrentBackgroundId
        {
            get
            {
                return currentBackgroundId;
            }
            set
            {
                currentBackgroundId = value;
                DataManager.Ins.MarkDirty();
            }
            
        }
        
        [SerializeField] private int currentFavoriteStyleId;

        public int CurrentFavoriteStyleId
        {
            get
            {
                return currentFavoriteStyleId;
            }
            set
            {
                currentFavoriteStyleId = value;
                DataManager.Ins.MarkDirty();
            }
            
        }

        [SerializeField] private int currentAlbumRewardId;
        [SerializeField] private float currentCoinRewardProgress;

        public float CurrentCoinRewardProgress
        {
            get
            {
                return currentCoinRewardProgress;
            }
            set
            {
                currentCoinRewardProgress = value;
                DataManager.Ins.MarkDirty();
            }
        }

        [SerializeField] private string tileSetName = DataConst.DEFAULT_TILE_SET_NAME;
        public string TileSetName
        {
            get => tileSetName;
            set
            {
                tileSetName = value;
                DataManager.Ins.UpdateTileRecord();
                DataManager.Ins.MarkDirty();
            }
        }
    }
}