using CLHoma.DGTween;
using TemplateSystems;
using UnityEngine;

namespace CLHoma.Combat
{

    public class CharacterGraphics : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Shoot,
        }
        private static readonly int PARTICLE_UPGRADE = ParticlesController.GetHash("Upgrade");
        private static readonly float SCALE_FACTOR = 0.8f;

        [SerializeField]
        protected Animator characterAnimator;

        [Space]
        [SerializeField] SkinnedMeshRenderer meshRenderer;
        public SkinnedMeshRenderer MeshRenderer => meshRenderer;
        [SerializeField] SpriteRenderer circleRarityHero;
        [Header("Weapon")]
        [SerializeField] Transform weaponsTransform;

        [SerializeField] Transform minigunHolderTransform;
        public Transform MinigunHolderTransform => minigunHolderTransform;

        [SerializeField] Transform shootGunHolderTransform;
        public Transform ShootGunHolderTransform => shootGunHolderTransform;

        [SerializeField] Transform rocketHolderTransform;
        public Transform RocketHolderTransform => rocketHolderTransform;

        [SerializeField] Transform teslaHolderTransform;
        public Transform TeslaHolderTransform => teslaHolderTransform;


        protected CharacterBehaviour characterBehaviour;

        private State currentState;

        public virtual void Initialise(Character characterInfo, CharacterBehaviour characterBehaviour)
        {
            this.characterBehaviour = characterBehaviour;
            characterAnimator.Play("Idle");
            currentState = State.Idle;
            transform.localScale = Vector3.one * SCALE_FACTOR;
            SetRarityColorZone(characterInfo);
        }
        public void Grunt()
        {
            var strength = 0.1f;
            var durationIn = 0.1f;
            var durationOut = 0.15f;

            weaponsTransform.DOMoveY(weaponsTransform.position.y - strength, durationIn).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                weaponsTransform.DOMoveY(weaponsTransform.position.y + strength, durationOut).SetEasing(Ease.Type.SineInOut);
            });
        }

        public void OnShoot()
        {
            //if (currentState == State.Shoot) return;
            characterAnimator.SetTrigger("Shoot");
            currentState = State.Shoot;
        }
        public void OnStopShoot()
        {
            //if (currentState == State.Idle) return;
            //currentState = State.Idle;
            //characterAnimator.SetTrigger("Idle");
        }
        public void PlayBounceAnimation()
        {
            transform.localScale = Vector3.one * 0.3f;
            transform.DOScale(Vector3.one * SCALE_FACTOR, 0.2f).SetEasing(Ease.Type.BackOut);
        }

        public void PlayUpgradeParticle()
        {
            ParticleCase particleCase = ParticlesController.PlayParticle(PARTICLE_UPGRADE).SetPosition(transform.position + new Vector3(0, 0.5f, 0)).SetScale((5).ToVector3());
            particleCase.ParticleSystem.transform.Rotate(Vector3.up, 180);
        }

        private void SetRarityColorZone(Character characterInfo)
        {
            Color rarityColor = DataManager.Instance.RarityTypeColorsHeroData.GetColor(characterInfo.RarityType);
            circleRarityHero.color = rarityColor;
            
            ParticleSystem particleSystem = characterBehaviour.RarityZone;
            if (particleSystem != null)
            {
                ParticleSystem[] allParticleSystems = particleSystem.GetComponentsInChildren<ParticleSystem>(true);
                
                foreach (ParticleSystem ps in allParticleSystems)
                {
                    var main = ps.main;
                    main.startColor = rarityColor;
                    
                    var colorOverLifetime = ps.colorOverLifetime;
                    if (colorOverLifetime.enabled)
                    {
                        Gradient gradient = new Gradient();
                        gradient.SetKeys(
                            new GradientColorKey[] { 
                                new GradientColorKey(rarityColor, 0.0f),
                                new GradientColorKey(rarityColor, 1.0f) 
                            },
                            new GradientAlphaKey[] { 
                                new GradientAlphaKey(1.0f, 0.0f),
                                new GradientAlphaKey(0.0f, 1.0f) 
                            }
                        );
                        colorOverLifetime.color = gradient;
                    }
                }
            }
        }
    }
}