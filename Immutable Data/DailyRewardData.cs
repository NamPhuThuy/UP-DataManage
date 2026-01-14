using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.DataManage
{
    [CreateAssetMenu(fileName = "DailyRewardData", menuName = "Game/DailyRewardData")]
    public class DailyRewardData : ScriptableObject
    {
        public List<DailyRewardRecord> data;

        [NonSerialized] 
        private Dictionary<int, DailyRewardRecord> _dictIntDailyReward;

        public Dictionary<int, DailyRewardRecord> DictIntDailyReward
        {
            get
            {
                EnsureDict();
                return _dictIntDailyReward;
            }
        }

        #region Private Fields

        private void EnsureDict()
        {
            if (_dictIntDailyReward != null) return;
            _dictIntDailyReward = new Dictionary<int, DailyRewardRecord>(data?.Count ?? 0);
            if (data == null) return;

            for (int i = 0; i < data.Count; i++)
            {
                _dictIntDailyReward[data[i].dayId] = data[i];
            }
        }

        #endregion

        #region Callbacks

        private void OnValidate()
        {
            if (data == null) return;
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] != null)
                {
                    data[i].dayId = i; // auto-increment, matches list index
                }
            }
        }

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods

        public DailyRewardRecord GetReward(int dayId)
        {
            EnsureDict();
            return _dictIntDailyReward != null && _dictIntDailyReward.TryGetValue(dayId, out var cfg) ? cfg : new DailyRewardRecord();
        }

        public bool TryApplyReward(int dayId, int amountMultiplier = 1)
        {
            EnsureDict();
            if (_dictIntDailyReward == null) return false;
            if (!_dictIntDailyReward.TryGetValue(dayId, out var reward)) return false;
            if (reward.rewards == null) return false;
            
            return DataManager.Ins.PInventoryData.TryApplyRewards(reward.rewards);
        }
        
        #endregion

        [Serializable]
        public class DailyRewardRecord
        {
            public int dayId;
            public List<ResourceAmount> rewards;
        }
    }

}