using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using NamPhuThuy.Common;
using UnityEditor;
#endif

namespace NamPhuThuy.DataManage
{
    
    
    [CreateAssetMenu(fileName = "BoosterData", menuName = "Game/BoosterData")]
    public class BoosterData : ScriptableObject
    {
        [FormerlySerializedAs("allBoosters")] public BoosterRecord[] data;
        
        // internal cache for faster lookups
        private Dictionary<BoosterType, BoosterRecord> _dataDict;

        #region Callbacks

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (data == null || data.Length == 0) return;

            var typesSeen = new HashSet<BoosterType>();
            var duplicates = new List<BoosterType>();

            foreach (var booster in data)
            {
                if (booster == null) continue;
        
                if (booster.BoosterType == BoosterType.NONE) continue;

                if (!typesSeen.Add(booster.BoosterType))
                {
                    if (!duplicates.Contains(booster.BoosterType))
                    {
                        duplicates.Add(booster.BoosterType);
                    }
                }
            }

            if (duplicates.Count > 0)
            {
                DebugLogger.LogError(message:$"[BoosterData] Duplicate BoosterTypes found: {string.Join(", ", duplicates)}", context: this);
            }

            // Clear the cached dictionary to force rebuild on next lookup
            _dataDict = null;
#endif
        }

        #endregion
        
        private void BuildIndex()
        {
            if (_dataDict != null) return;

            _dataDict = new Dictionary<BoosterType, BoosterRecord>();
            if (data == null) return;

            foreach (var booster in data)
            {
                if (booster == null) continue;
                _dataDict[booster.BoosterType] = booster; // last one wins if duplicates
            }
        }
        
        public Sprite GetGamePlaySprite(BoosterType type)
        {
            var data = GetBoosterRecord(type);
            if (data == null) return null;
            return data.gameplayImage;  
        }

        /// <summary>
        /// Finds booster info by its type.
        /// Returns null if not found.
        /// </summary>
        public BoosterRecord GetBoosterRecord(BoosterType type)
        {
            BuildIndex();
            return _dataDict.TryGetValue(type, out var data) ? data : null;
        }
    }
    
    [Serializable]
    public class BoosterAmount
    {
        public BoosterType boosterType;
        public int amount = 0;

        public BoosterAmount(BoosterType boosterType = BoosterType.NONE, int amount = 0)
        {
            this.boosterType = boosterType;
            this.amount = amount;
        }
    }
    
    [Serializable]
    public class BoosterRecord
    {
        [SerializeField] private BoosterType boosterType;
        [SerializeField] private int unlockLevel;
        [SerializeField] private int stackPerBuy;
        [SerializeField] private List<ResourceAmount> price;
        
        public string boosterName;
        public string offerDescription;
        public Sprite gameplayImage;
        public Sprite inventoryImage;

        public BoosterType BoosterType => boosterType;
        public int UnlockLevel => unlockLevel;
        public int StackPerBuy => stackPerBuy;
        public List<ResourceAmount> Price => price;
    }
    
    public enum BoosterType
    {
        NONE = 0,
        TIMER = 1,
        MAGIC_PICK = 2,
        SHUFFLE = 3,
        MORE_GRILL = 7,
        EXTRA_HEART = 8

    }
}