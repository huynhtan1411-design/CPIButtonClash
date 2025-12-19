using CLHoma.Upgrades;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TemplateSystems;
using TMPro;
using UnityEngine;
namespace CLHoma.Combat
{
    public class CharacterBehaviour : MonoBehaviour, IEnemyDetector, IClickable
    {
        protected static CharacterBehaviour characterBehaviour;
        [SerializeField] protected EnemyDetector enemyDetector;
        [SerializeField] protected ParticleSystem rarityZone;
        [SerializeField] protected ParticleSystem upgradeParticle;
        [SerializeField] protected TextMeshPro textNoti;

        protected CharacterGraphics graphics;
        protected GameObject graphicsPrefab;
        protected BaseGunBehavior gunBehaviour;
        protected GameObject gunPrefabGraphics;
        protected Tween currentTween;
        protected Sequence currentSequence;
        public static Transform Transform => characterBehaviour.transform;


        public EnemyDetector EnemyDetector => enemyDetector;
        public bool IsCloseEnemyFound => closestEnemyBehaviour != null;

        protected BaseEnemyBehavior closestEnemyBehaviour;
        public BaseEnemyBehavior ClosestEnemyBehaviour => closestEnemyBehaviour;
        
        public ParticleSystem RarityZone => rarityZone;

        protected Transform playerTarget;
        public static bool NoDamage { get; private set; } = false;

        public HeroeType CharacterType => _characterData.Type;

        protected Character _characterData;
        public Character Data => _characterData;

        // For Card-based character upgrade
        protected HeroCardData currentHoveringCardData;

        public void Initialise(Character character, Vector3 pos)
        {
            characterBehaviour = this;
            GameObject tempTarget = new GameObject("[TARGET]");
            tempTarget.transform.position = transform.position;
            tempTarget.SetActive(true);

            playerTarget = tempTarget.transform;
            //enemyDetector.Initialise(this);

            _characterData = new Character(
                character.Id,
                character.Type,
                character.Prefab,
                character.WeaponType,
                character.ElementType,
                character.RarityType
            );

            //SetGraphics(character, character.Prefab, false, true);
            SetPosition(pos);
            //SetupWeapon(character.WeaponType, character.ElementType);
            //Reload();
            //StartCoroutine(GunUpdateCoroutine());
            if (character.Type != HeroeType.Main)
            {
                UnityEngine.Color rarityColor = DataManager.Instance.RarityTypeColorsHeroData.GetColor(character.RarityType);
                var mainModule = rarityZone.main;
                mainModule.startColor = rarityColor;
            }
            else
                rarityZone.gameObject.SetActive(false);

            Audio_Manager.instance.play("sfx_summon_hero");
        }
        public void SetDamage(int value)
        {
            gunBehaviour.SetDamage(value);
        }
        private void Reload(bool resetHealth = true)
        {
            enemyDetector.Reload();
            gunBehaviour.Reload();
            gameObject.SetActive(true);
        }

        private void SetPosition(Vector3 position)
        {
            playerTarget.position = position.AddToZ(10f);
            transform.position = position;
            transform.rotation = Quaternion.identity;
        }
        #region Gun
        public void UpgradeWeapon()
        {  
            gunBehaviour.UpgradeGun();
            gunBehaviour.OnLevelLoaded();

            _characterData.RarityType++;
            graphics.Initialise(_characterData, this);
            upgradeParticle.Play();
        }
        public void UpgradeWeapon(int upgradeLevel)
        {
            gunBehaviour.SetUpgradeLevelGun(upgradeLevel);
            gunBehaviour.OnLevelLoaded();

            TypeRarityHero type = (TypeRarityHero)upgradeLevel;
            _characterData.RarityType = type;
            graphics.Initialise(_characterData, this);
            upgradeParticle.Play();
        }
        public void SetupWeapon(WeaponType weaponType, ElementType elementType)
        {
            var gunUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponType);
            var currentStage = gunUpgrade.GetCurrentStage(0);
            if (gunPrefabGraphics != currentStage.WeaponPrefab)
            {
                gunPrefabGraphics = currentStage.WeaponPrefab;
                if (gunBehaviour != null)
                {
                    gunBehaviour.OnGunUnloaded();
                    Destroy(gunBehaviour.gameObject);
                }
                if (gunPrefabGraphics != null)
                {
                    GameObject gunObject = Instantiate(gunPrefabGraphics);
                    gunObject.SetActive(true);
                    gunBehaviour = gunObject.GetComponent<BaseGunBehavior>();

                    if (graphics != null)
                    {
                        gunBehaviour.PlaceGun(graphics);
                    }
                }
            }

            if (gunBehaviour != null)
            {
                gunBehaviour.Initialise(this, weaponType, elementType);
            }
            enemyDetector.SetRadius(currentStage.RangeRadius);
        }


        public void OnGunShooted()
        {
            graphics.OnShoot();
        }
        public void OnStopShoot()
        {
            graphics.OnStopShoot();
        }
        #endregion

        #region Graphics

        public void SetGraphics(Character characterInfo, GameObject newGraphicsPrefab, bool playParticle, bool playAnimation)
        {
            if (graphicsPrefab != newGraphicsPrefab)
            {
                graphicsPrefab = newGraphicsPrefab;

                if (graphics != null)
                {
                    if (gunBehaviour != null)
                        gunBehaviour.transform.SetParent(null);

                    Destroy(graphics.gameObject);
                }

                GameObject graphicObject = Instantiate(newGraphicsPrefab);

                graphicObject.transform.SetParent(transform);
                graphicObject.transform.ResetLocal();
                graphicObject.SetActive(true);

                graphics = graphicObject.GetComponent<CharacterGraphics>();
                graphics.Initialise(characterInfo, this);
                upgradeParticle.Play();
                if (gunBehaviour != null)
                {
                    gunBehaviour.PlaceGun(graphics);
                }
                if (playParticle)
                    graphics.PlayUpgradeParticle();

                if (playAnimation)
                    graphics.PlayBounceAnimation();
            }
        }
        #endregion

        private void Update()
        {
            if (IsCloseEnemyFound)
            {
                playerTarget.position = Vector3.Lerp(playerTarget.position, new Vector3(closestEnemyBehaviour.transform.position.x, transform.position.y, closestEnemyBehaviour.transform.position.z), Time.deltaTime);
                transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));
            }
            if (textNoti != null && textNoti.gameObject.activeInHierarchy)
            {
                Vector3 directionToCamera = (textNoti.transform.position - Camera.main.transform.position).normalized;
                textNoti.transform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            }
        }

        private IEnumerator GunUpdateCoroutine()
        {
            while (true)
            {
                if (gunBehaviour != null)
                    gunBehaviour.GunUpdate();

                yield return new WaitForSeconds(0.1f);
            }
        }
        public void OnCloseEnemyChanged(BaseEnemyBehavior enemyBehavior)
        {
            if (enemyBehavior != null)
            {
                if (closestEnemyBehaviour == null)
                {
                    playerTarget.position = transform.position + transform.forward * 5;
                }
                closestEnemyBehaviour = enemyBehavior;
                return;
            }
            closestEnemyBehaviour = null;
        }
        public void TryAddClosestEnemy(BaseEnemyBehavior enemy)
        {
            EnemyDetector.TryAddClosestEnemy(enemy);
        }

        public List<BaseEnemyBehavior> GetRandomTargets(int count)
        {
            return enemyDetector.GetRandomTargets(count);
        }

        /// <summary>
        /// Handles click event from IClickable interface
        /// </summary>
        public void OnClick(Vector3 hitPoint)
        {
            if (currentHoveringCardData != null)
            {
                if (currentHoveringCardData.HeroType == _characterData.Type && 
                    currentHoveringCardData.RarityType == _characterData.RarityType)
                {
                    UpgradeWeapon();
                }
            }
        }

        public void UpgradeHero(HeroCardData cardData = null)
        {
            if (currentHoveringCardData != null)
            {
                if (currentHoveringCardData.HeroType == _characterData.Type &&
                    currentHoveringCardData.RarityType == _characterData.RarityType)
                {
                    if (cardData.SkillData != null)
                    {
                        string noti = cardData.SkillData.effects[0].GetEffectValueString(_characterData.RarityType.GetHashCode() + 2);
                        ShowNoti(noti);
                    }
                    PlayerController.Instance.CountSpawnHero++;
                    UpgradeWeapon();
                }
            }
        }

        /// <summary>
        /// Called by DraggableCard to check if the card can upgrade this character
        /// </summary>
        public bool CanUpgradeWithCard(HeroCardData cardData)
        {
            if (_characterData.RarityType == TypeRarityHero.Godlike)
                return false;
            if (cardData.HeroType == _characterData.Type && 
                cardData.RarityType == _characterData.RarityType)
            {
                currentHoveringCardData = cardData;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clears the current hovering card data
        /// </summary>
        public void ClearHoveringCard()
        {
            currentHoveringCardData = null;
        }

        internal void ApplySkill(HeroCardData cardData)
        {
            string noti = cardData.SkillData.effects[0].GetEffectValueString(cardData.Level + 1);
            ShowNoti(noti);
            ApplyPassiveEffects(cardData);
        }

        private void ApplyPassiveEffects(SkillSpec skillSpec)
        {
            if (skillSpec.skillData.abilityType != AbilityType.Passive) return;
            foreach (PassiveSkillEffect effect in skillSpec.skillData.effects)
            {
                float valueChange = effect.GetEffectValue(skillSpec.level);
                UpdateStat(effect.statType, valueChange);
            }
        }
        public void ApplyPassiveEffects(HeroCardData cardData)
        {
            SkillSpec skillSpec = new SkillSpec(cardData.SkillData, (int)cardData.RarityType + 1);
            ApplyPassiveEffects(skillSpec);
        }

        public void UpdateStat(StatType type, float value)
        {
            Debug.LogWarning("ApplySkill Special " + type + " = " +value);
            Vector3 size = transform.localScale;
            Vector3 sizeEfeect = size * 1.2f;
            transform.DOScale(sizeEfeect, 0.2f)
            .OnComplete(() =>
            {
                transform.DOScale(size, 0.2f);

            });
            switch (type)
            {
                case StatType.AttackPower:
                    Data.AttackPower += value;
                    //Debug.LogError("AttackPower " + Data.AttackPower);
                    break;
                case StatType.CooldownReduction:
                    Data.CooldownReduction += value;
                    //Debug.LogError("AttackPower " + Data.CooldownReduction);
                    break;
                case StatType.ProjectileSpeed:
                    Data.MoveSpeed += value;
                    //Debug.LogError("MoveSpeed " + Data.MoveSpeed);
                    break;
                default:
                   // Debug.LogError("Stat type not found: " + type.ToString());
                    return;
            }
           
        }

        public void ShowNoti(string message)
        {
            currentTween?.Kill();
            currentSequence?.Kill();

            textNoti.text = message;
            textNoti.color = new UnityEngine.Color(textNoti.color.r, textNoti.color.g, textNoti.color.b, 1f);
            textNoti.transform.localScale = Vector3.zero;
            textNoti.transform.localPosition = Vector3.up * 1;

            textNoti.gameObject.SetActive(true);

            currentSequence = DOTween.Sequence();

            currentSequence.Append(textNoti.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack))
                           .Append(textNoti.transform.DOLocalMoveY(1.5f, 3.5f).SetEase(Ease.OutQuad))
                           .Join(textNoti.DOFade(0f, 4f))
                           .OnComplete(() => textNoti.gameObject.SetActive(false));
        }
    }
}