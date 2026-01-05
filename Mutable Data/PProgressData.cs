using System;
using UnityEngine;

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

       
    }
}