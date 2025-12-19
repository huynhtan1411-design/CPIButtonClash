namespace CLHoma.Upgrades
{
    public interface IUpgrade
    {
        public WeaponType UpgradeType { get; }
        public BaseUpgradeStage[] Upgrades { get; }

        public void Initialise();
        public void UpgradeStage();
    }
}