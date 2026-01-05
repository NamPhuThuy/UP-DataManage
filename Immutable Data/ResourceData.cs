using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NamPhuThuy.DataManage
{
    /// <summary>
    /// Testing, not being used yet
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceData", menuName = "Game/ResourceData")]
    
    public class ResourceData : ScriptableObject
    {
        [Header("Resource Data")]
        [SerializeField] private ResourceRecord[] resourceRecords;
        private Dictionary<ResourceType, ResourceRecord> _dictRecordByType;

        public Dictionary<ResourceType, ResourceRecord> Data
        {
            get
            {
                if (_dictRecordByType == null)
                {
                    EnsureDictInit();
                }
                return _dictRecordByType;
            }
        }

        private void EnsureDictInit()
        {
            if (_dictRecordByType != null) return;
            _dictRecordByType = new Dictionary<ResourceType, 
                ResourceRecord>(resourceRecords?.Length ?? 0);
            if (resourceRecords == null) return;
            foreach (var r in resourceRecords)
            {
                if (r == null) continue;
                _dictRecordByType[r.resourceType] = r;
            }
        }

        public ResourceRecord GetResourceRecord(ResourceType resourceType)
        {
            EnsureDictInit();
            return _dictRecordByType != null && _dictRecordByType.TryGetValue(resourceType, out var record) ? record : null;
        }
        
        public Sprite GetResourceGameplayImage(ResourceType resourceType)
        {
            EnsureDictInit();
            if (_dictRecordByType != null && _dictRecordByType.TryGetValue(resourceType, out var record))
            {
                return record.gameplayImage;
            }
            return null;
        }
        
        public int GetBoosterPrice(BoosterType boosterType)
        {
            EnsureDictInit();
            foreach (var record in _dictRecordByType.Values)
            {
                if (record.resourceType == ResourceType.BOOSTER && record.boosterType == boosterType)
                {
                    int price = 0;
                    foreach (var priceItem in record.priceInResources)
                    {
                        if (priceItem.resourceType == ResourceType.COIN)
                            price += priceItem.amount;
                    }
                    return price;
                }
            }
            return 150; // Not found
        }
    }

    [Serializable]
    public class ResourceRecord
    {
        public ResourceType resourceType;
        
        public BoosterType boosterType;
        
        public Sprite gameplayImage;
        public Sprite inventoryImage;
        
        public string description;
        public List<ResourceAmount> priceInResources;
    }
    
    [Serializable]
    public class ResourceAmount
    {
        public ResourceType resourceType;
        
        public BoosterType boosterType;

        public int amount;

        public ResourceAmount(ResourceType resourceType = ResourceType.COIN, BoosterType boosterType = BoosterType.NONE, int amount = 0)
        {
            this.resourceType = resourceType;
            this.boosterType = boosterType;
            this.amount = amount;
        }
    }
    
    // Cant rename to AssetType because of conflict with "Asset" term in Unity
    public enum ResourceType
    {
        NONE = 0,
        COIN = 1,
        BOOSTER = 2,
        HEART = 3,
        NO_ADS = 5,
        TIME = 6,
    }
    
}