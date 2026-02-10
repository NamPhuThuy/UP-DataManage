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
        

        [SerializeField] private bool isAdsRemoved;
        public bool IsVIP;

        public bool IsAdsRemoved 
        {
            get => isAdsRemoved;
            set
            {
                isAdsRemoved = value;
                DataManager.Ins.MarkDirty();
            }
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

        public void RemoveAds()
        {
            IsAdsRemoved = true;
        }

       
    }
}