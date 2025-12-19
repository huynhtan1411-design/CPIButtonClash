using UnityEngine;

namespace CLHoma.Upgrades
{
    public abstract class Upgrade<T> : BaseUpgrade where T : BaseUpgradeStage
    {
        [SerializeField]
        protected T[] upgrades;
        public override BaseUpgradeStage[] Upgrades => upgrades;

        public T GetCurrentStage(int UpgradeLevel)
        {
            if (upgrades.IsInRange(UpgradeLevel))
                return upgrades[UpgradeLevel];

            UpgradeLevel = upgrades.Length - 1;
            Debug.Log("[Perks]: Perk level is out of range!");

            return upgrades[UpgradeLevel];
        }

        public override void UpgradeStage()
        {

        }
    }
}