using UnityEngine;

namespace CLHoma.Upgrades
{
    public abstract class BaseUpgrade : ScriptableObject, IUpgrade
    {
        [SerializeField]
        protected WeaponType upgradeType;
        public WeaponType UpgradeType => upgradeType;

        public abstract BaseUpgradeStage[] Upgrades { get; }
        public int UpgradesCount => Upgrades.Length;


        public abstract void Initialise();
        public abstract void UpgradeStage();
    }
}