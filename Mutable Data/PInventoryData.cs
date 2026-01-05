using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace NamPhuThuy.DataManage
{
    [Serializable]
    public class PInventoryData
    {
        /*[SerializeField] private List<ResourceAmount> resources = new()
        {
            new ResourceAmount() { resourceType = ResourceType.COIN, amount = 0 },
            new ResourceAmount() { resourceType = ResourceType.HEART, amount = 5 },
        };

        [SerializeField] private List<BoosterAmount> boosters = new()
        {
            new BoosterAmount() { boosterType = BoosterType.TIMER, amount = 0 },
            new BoosterAmount() { boosterType = BoosterType.MAGIC_PICK, amount = 0 },
            new BoosterAmount() { boosterType = BoosterType.SHUFFLE, amount = 0 },
        };
        
        private Dictionary<ResourceType, int> _dictResource;
        
        public Dictionary<ResourceType, int> Resource
        {
            get
            {
                if (resources == null)
                {
                    EnsureDictResourceInit();
                }
                return _dictResource;
            }
        }
        
        private Dictionary<BoosterType, int> _dictBooster;
        
        public Dictionary<BoosterType, int> Booster
        {
            get
            {
                if (boosters == null)
                {
                    EnsureDictResourceInit();
                }
                return _dictBooster;
            }
        }

        private void EnsureDictResourceInit()
        {
            if (_dictResource != null) return;
            _dictResource = new Dictionary<ResourceType, int>(resources?.Count ?? 0);
            if (resources == null) return;
            foreach (var r in resources)
            {
                if (r == null) continue;
                _dictResource[r.resourceType] = r.amount;
            }
        }
        
        private void EnsureDictBoosterInit()
        {
            if (_dictBooster != null) return;
            _dictBooster = new Dictionary<BoosterType, int>(boosters?.Count ?? 0);
            if (boosters == null) return;
            foreach (var b in boosters)
            {
                if (b == null) continue;
                _dictBooster[b.boosterType] = b.amount;
            }
        }*/
        
        /// <summary>
        /// Sync runtime dictionary values back into the serializable lists before saving.
        /// Call this right before JsonUtility.ToJson\(\).
        /// </summary>
        /*public void SyncDictToListForSave()
        {
            // Resources
            EnsureDictResourceInit();
            if (resources == null) resources = new List<ResourceAmount>();

            for (int i = 0; i < resources.Count; i++)
            {
                var r = resources[i];
                if (r == null) continue;

                if (_dictResource.TryGetValue(r.resourceType, out var amount))
                    r.amount = amount;
            }

            var existingResources = new HashSet<ResourceType>();
            for (int i = 0; i < resources.Count; i++)
            {
                var r = resources[i];
                if (r == null) continue;
                existingResources.Add(r.resourceType);
            }

            foreach (var kv in _dictResource)
            {
                if (existingResources.Contains(kv.Key)) continue;

                resources.Add(new ResourceAmount
                {
                    resourceType = kv.Key,
                    amount = kv.Value
                });
            }

            // Boosters
            EnsureDictBoosterInit();
            if (boosters == null) boosters = new List<BoosterAmount>();

            for (int i = 0; i < boosters.Count; i++)
            {
                var b = boosters[i];
                if (b == null) continue;

                if (_dictBooster.TryGetValue(b.boosterType, out var amount))
                    b.amount = amount;
            }

            var existingBoosters = new HashSet<BoosterType>();
            for (int i = 0; i < boosters.Count; i++)
            {
                var b = boosters[i];
                if (b == null) continue;
                existingBoosters.Add(b.boosterType);
            }

            foreach (var kv in _dictBooster)
            {
                if (existingBoosters.Contains(kv.Key)) continue;

                boosters.Add(new BoosterAmount
                {
                    boosterType = kv.Key,
                    amount = kv.Value
                });
            }
        }*/

        #region Public Methods

        

        #endregion

        #region Coin Helpers

        #region Coin Helpers

        
        [SerializeField] private int coin;
        public int Coin
        {
            get => coin;
            set
            {
                coin = value;
                coin = Math.Max(0, value);

                DataManager.Ins.MarkDirty();
                MMEventManager.TriggerEvent(new EResourceUpdated()
                {
                    ResourceType = ResourceType.COIN
                });
            }
        }
        
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Coin = coin + amount;
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (coin < amount) return false;
            Coin = coin - amount;
            return true;
        }

        #endregion

        #endregion
    }
    
}