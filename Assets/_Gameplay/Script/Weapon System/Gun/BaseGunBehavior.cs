using CLHoma.DGTween;
using UnityEngine;

namespace CLHoma.Combat
{
    public abstract class BaseGunBehavior : MonoBehaviour
    {
        private static readonly int PARTICLE_UPGRADE = ParticlesController.GetHash("Gun Upgrade");


        [Space]
        [SerializeField] Transform leftHandHolder;
        [SerializeField] Transform rightHandHolder;

        [Space]
        [SerializeField]
        protected Transform shootPoint;

        [Header("Upgrade")]
        [SerializeField] Vector3 upgradeParticleOffset;
        [SerializeField] float upgradeParticleSize = 1.0f;

        protected CharacterBehaviour characterBehaviour;
        protected WeaponType weaponType;
        protected ElementType elementType;

        protected float factorDamage;

        protected int upgradeLevel = 0;
        protected int baseDamage;
        public virtual void Initialise(CharacterBehaviour characterBehaviour, WeaponType weaponType, ElementType elementType)
        {
            this.characterBehaviour = characterBehaviour;
            this.weaponType = weaponType;
            this.elementType = elementType;
        }
        public void UpgradeGun()
        {
            upgradeLevel++;
        }
        public void SetUpgradeLevelGun(int upgradeLevel)
        {
            this.upgradeLevel = upgradeLevel;
        }

        public void SetDamage(int value)
        {
            baseDamage = value;
        }
        public virtual void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public virtual void GunUpdate()
        {

        }

        public abstract void Reload();
        public abstract void OnGunUnloaded();
        public abstract void PlaceGun(CharacterGraphics characterGraphics);

        public abstract void RecalculateDamage();

        public virtual void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.6f;
            transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
        }

        public void PlayUpgradeParticle()
        {
            ParticleCase particleCase = ParticlesController.PlayParticle(PARTICLE_UPGRADE).SetPosition(transform.position + upgradeParticleOffset).SetScale(upgradeParticleSize.ToVector3());
            particleCase.ParticleSystem.transform.Rotate(Vector3.up, 180);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + upgradeParticleOffset, upgradeParticleSize.ToVector3());
        }

        protected float GetDamage()
        {
            float value = baseDamage * factorDamage;
            return value;
        }
    }
}