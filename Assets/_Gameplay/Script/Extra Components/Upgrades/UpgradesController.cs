using System.Collections.Generic;
using UnityEngine;

namespace CLHoma
{
    using Upgrades;

    public class UpgradesController : MonoBehaviour
    {
        private const string SAVE_IDENTIFIER = "upgrades:{0}";

        [SerializeField] UpgradesDatabase upgradesDatabase;

        private static BaseUpgrade[] activeUpgrades;
        public static BaseUpgrade[] ActiveUpgrades => activeUpgrades;

        private static Dictionary<WeaponType, BaseUpgrade> activeUpgradesLink;

        public void Initialise()
        {
            activeUpgrades = upgradesDatabase.Upgrades;

            activeUpgradesLink = new Dictionary<WeaponType, BaseUpgrade>();
            for (int i = 0; i < activeUpgrades.Length; i++)
            {
                var upgrade = activeUpgrades[i];

                var hash = string.Format(SAVE_IDENTIFIER, upgrade.UpgradeType.ToString()).GetHashCode();

                if (!activeUpgradesLink.ContainsKey(upgrade.UpgradeType))
                {
                    upgrade.Initialise();

                    activeUpgradesLink.Add(upgrade.UpgradeType, activeUpgrades[i]);
                }
            }
        }

        [System.Obsolete]
        public static BaseUpgrade GetUpgradeByType(WeaponType perkType)
        {
            if (activeUpgradesLink.ContainsKey(perkType))
                return activeUpgradesLink[perkType];

            Debug.LogError($"[Perks]: Upgrade with type {perkType} isn't registered!");

            return null;
        }

        public static T GetUpgrade<T>(WeaponType type) where T : BaseUpgrade
        {
            if (activeUpgradesLink.ContainsKey(type))
                return activeUpgradesLink[type] as T;

            Debug.LogError($"[Perks]: Upgrade with type {type} isn't registered!");

            return null;
        }
    }
}