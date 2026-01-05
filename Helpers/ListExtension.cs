using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.DataManage
{
    public static class ListExtension
    {
        /// <summary>
        /// Generic method to get amount of any specific ResourceType.
        /// </summary>
        public static int GetAmount(this List<ResourceAmount> resources, ResourceType targetType)
        {
            if (resources == null || resources.Count == 0) return 0;

            foreach (var resource in resources)
            {
                if (resource == null) continue;
                
                if (resource.resourceType == targetType)
                {
                    return resource.amount;
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the amount of coins (Wrapper for convenience).
        /// </summary>
        public static int GetCoinAmount(this List<ResourceAmount> resources)
        {
            return resources.GetAmount(ResourceType.COIN);
        }
    }
}