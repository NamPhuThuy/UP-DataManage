using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

#if USE_MOREMOUNTAIN_EVENT
using MoreMountains.Tools;
#endif

namespace NamPhuThuy.DataManage
{
    [Serializable]
    public class PInventoryData
    {
        #region Public Methods

        #endregion

        #region Coin Helpers

        [SerializeField] private int coin;

        public int Coin
        {
            get => coin;
            private set
            {
                coin = value;
                coin = Math.Max(0, value);

                Debug.Log(message: $"DataManager.Coin: new value: {value}");
            }
        }

        public void AddCoins(int amount, bool isUseUpdateAnim = true)
        {
            if (amount <= 0) return;
            Coin = coin + amount;

            DataManager.Ins.MarkDirty();
            
#if USE_MOREMOUNTAIN_EVENT            
            MMEventManager.TriggerEvent(new EResourceUpdated()
            {
                ResourceType = ResourceType.COIN,
                IsUseUpdateAnim = isUseUpdateAnim
            });
#endif 
        }

        public bool TrySpendCoins(int amount, bool isUseUpdateAnim = true)
        {
            if (amount <= 0) return true;
            if (coin < amount) return false;
            Coin = coin - amount;

            DataManager.Ins.MarkDirty();

#if USE_MOREMOUNTAIN_EVENT
            MMEventManager.TriggerEvent(new EResourceUpdated()
            {
                ResourceType = ResourceType.COIN,
                IsUseUpdateAnim = isUseUpdateAnim
            });
#endif

            return true;
        }

        #endregion

        public void ClearAllCoins()
        {
            throw new NotImplementedException();
        }

        public void ClearBoosters()
        {
            throw new NotImplementedException();
        }

        #region Booster Helpers

        public List<PlayerBoosterData> boosters = new List<PlayerBoosterData>();

        public int GetBoosterNum(BoosterType type)
        {
            var entry = boosters.Find(b => b.boosterType == type);
            return entry?.amount ?? 0;
        }

        public void AddBooster(BoosterType type, int amount)
        {
            if (amount <= 0) return;

            var entry = boosters.Find(b => b.boosterType == type);
            if (entry != null)
            {
                entry.amount = Math.Max(0, entry.amount + amount);
            }
            else
            {
                boosters.Add(new PlayerBoosterData { boosterType = type, amount = Math.Max(0, amount) });
            }

            DataManager.Ins.MarkDirty();
#if USE_MOREMOUNTAIN_EVENT            
            MMEventManager.TriggerEvent(new EResourceUpdated()
            {
                ResourceType = ResourceType.BOOSTER
            });
#endif
        }

        public void SetBoosterNum(BoosterType type, int count)
        {
            var entry = boosters.Find(b => b.boosterType == type);
            if (entry != null)
                entry.amount = count;
            else
                boosters.Add(new PlayerBoosterData { boosterType = type, amount = count });

            DataManager.Ins.MarkDirty();
        }

        [Serializable]
        public class PlayerBoosterData
        {
            public BoosterType boosterType;
            public int amount;
        }

        #endregion


        #region General Helpers

        /// <summary>
        /// Apply a list of rewards to the player. Returns true if anything was granted.
        /// </summary>
        public bool TryApplyRewards(IList<ResourceAmount> rewards, int amountMultiplier = 1,
            bool isUseUpdateAnim = true)
        {
            Debug.Log(message: $"DataManager.TryApplyRewards()");
            if (rewards == null || rewards.Count == 0) return false;

            bool anyGranted = false;

            for (int i = 0; i < rewards.Count; i++)
            {
                var item = rewards[i];
                if (item == null) continue; // if ResourceReward is a class

                int amount = Math.Max(0, item.amount * Math.Max(1, amountMultiplier));
                switch (item.resourceType)
                {
                    case ResourceType.COIN:
                        if (amount <= 0) break;

                        DataManager.Ins.PInventoryData.AddCoins(amount, isUseUpdateAnim);
                        anyGranted = true;
                        break;

                    case ResourceType.BOOSTER:
                        if (amount <= 0) break;

                        SetBoosterNum(item.boosterType, GetBoosterNum(item.boosterType) + amount);
                        anyGranted = true;
                        break;

                    case ResourceType.NO_ADS:

                        DataManager.Ins.PProgressData.RemoveAds();
                        anyGranted = true;
                        break;
                }
            }

            if (anyGranted) DataManager.Ins.MarkDirty();
            return anyGranted;
        }

        /// <summary>
        /// Apply a single reward item. Returns true if granted.
        /// </summary>
        public bool TryApplyReward(ResourceAmount item, int amountMultiplier = 1)
        {
            if (item == null)
            {
                return false;
            }

            bool anyGranted = false;

            int amount = Math.Max(0, item.amount * Math.Max(1, amountMultiplier));
            switch (item.resourceType)
            {
                case ResourceType.COIN:
                    if (amount <= 0) break;

                    DataManager.Ins.PInventoryData.AddCoins(amount);
                    anyGranted = true;
                    break;

                case ResourceType.BOOSTER:
                    if (amount <= 0) break;

                    SetBoosterNum(item.boosterType, GetBoosterNum(item.boosterType) + amount);
                    anyGranted = true;
                    break;

                case ResourceType.NO_ADS:

                    DataManager.Ins.PProgressData.RemoveAds();
                    anyGranted = true;
                    break;
            }

            if (anyGranted) DataManager.Ins.MarkDirty();
            return anyGranted;
        }

        public bool TrySpendResource(ResourceType resourceType, int amount, BoosterType boosterType = BoosterType.NONE)
        {
            switch (resourceType)
            {
                case ResourceType.COIN:
                    return DataManager.Ins.PInventoryData.TrySpendCoins(amount);
                case ResourceType.BOOSTER:
                    int currentNum = GetBoosterNum(boosterType);
                    if (currentNum < amount) return false;
                    AddBooster(boosterType, -amount);
                    return true;
                case ResourceType.HEART:
                    if (health < amount) return false;
                    Health -= amount;
                    return true;
            }

            return false;
        }

        public void AddResource(ResourceType type, int amount, BoosterType boosterType = BoosterType.NONE)
        {
            if (amount <= 0) return;
            switch (type)
            {
                case ResourceType.COIN:
                    DataManager.Ins.PInventoryData.AddCoins(amount);
                    break;
                case ResourceType.BOOSTER:
                    AddBooster(boosterType, amount);
                    break;
                case ResourceType.NO_ADS:
                    DataManager.Ins.PProgressData.RemoveAds();
                    break;
                case ResourceType.HEART:
                    Health += amount;
                    break;
                default:
                    Debug.LogWarning($"PlayerData.AddResource() - Unsupported ResourceType: {type}");
                    break;
            }
        }

        public void PrintDebugInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== PlayerData ===");
            sb.AppendLine($"Health: {Health}");

            sb.AppendLine("Boosters:");
            if (boosters == null || boosters.Count == 0)
            {
                sb.AppendLine("  (none)");
            }
            else
            {
                for (int i = 0; i < boosters.Count; i++)
                {
                    var b = boosters[i];
                    if (b == null) continue;
                    sb.AppendLine($"  - Type: {b.boosterType}, Amount: {b.amount}");
                }
            }

            Debug.Log(message: $"{sb.ToString()}");
        }

        #endregion

        #region Health Helpers

        public float remainTimeForNextHeart;
        public long lastSessionTimestamp;
        [SerializeField] private int health;

        public int Health
        {
            get => health;
            set
            {
                health = value;
                health = Mathf.Clamp(health, 0, DataConst.MAX_HEALTH);

                DataManager.Ins.MarkDirty();
#if USE_MOREMOUNTAIN_EVENT                
                MMEventManager.TriggerEvent(new EResourceUpdated()
                {
                    ResourceType = ResourceType.HEART
                });
#endif
            }
        }

        public void UpdateWithTimePassed(float deltaTime)
        {
            if (health >= DataConst.MAX_HEALTH)
            {
                remainTimeForNextHeart = 0;
                return;
            }

            int healthsToRegen = (int)(deltaTime / DataConst.HEALTH_REGEN_TIME);
            if (healthsToRegen > 0)
            {
                health += healthsToRegen;
                health = Math.Min(health, DataConst.MAX_HEALTH);
                deltaTime -= healthsToRegen * DataConst.HEALTH_REGEN_TIME;
            }

            remainTimeForNextHeart -= (long)deltaTime;
            if (remainTimeForNextHeart <= 0 && health < DataConst.MAX_HEALTH)
            {
                health++;
                remainTimeForNextHeart += DataConst.HEALTH_REGEN_TIME;
            }

            DataManager.Ins.MarkDirty();
#if USE_MOREMOUNTAIN_EVENT            
            MMEventManager.TriggerEvent(new EResourceUpdated()
            {
                ResourceType = ResourceType.HEART
            });
#endif
        }

        #endregion
    }
}