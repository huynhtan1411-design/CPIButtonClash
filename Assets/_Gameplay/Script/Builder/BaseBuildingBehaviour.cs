using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CLHoma.Combat;
using TemplateSystems;
namespace WD
{
    public class BaseBuildingBehaviour : MonoBehaviour
    {
        [SerializeField] protected Transform modelContainer;

        protected int currentLevel = 1;
        protected int currentUpgradeCost;
        protected GameObject currentModel;
        protected BuildingData buildingConfig;
        protected BuildingCollectionItem buildingCard;
        protected BaseDamageableEntity baseDamageableEntity;
        protected BuildingGraphics buildingGraphics;

        public int CurrentUpgradeCost => currentUpgradeCost;

        public int CurrentLevel
        {
            get
            {
                return currentLevel;
            }
            set
            {
                currentLevel = value;
            }
        }
        public BuildingData BuildingConfig => buildingConfig;
        public BuildingGraphics BuildingGraphics => buildingGraphics;
        public BaseDamageableEntity BaseDamageableEntity => baseDamageableEntity;
        private int levelUnlockCondition;
        public virtual void Initialize(BuildingData config)
        {
            buildingConfig = config;
            currentUpgradeCost = buildingConfig.GetCostUpgrade(currentLevel);
            SpawnModel();
            buildingCard = DataManager.Instance.GetBuildingCollectionItem(buildingConfig.ID);
            if (buildingCard != null)
                buildingConfig.LevelCard = buildingCard.Level;
            WD.GameManager.Instance.OnBuildPhaseStart.AddListener(() =>
            {
                Rebuild();
            });
            if (baseDamageableEntity != null)
                baseDamageableEntity.onDeath.AddListener(OnDestroyBuilding);

            if (buildingConfig.Type == BuildingType.Base)
            {
                BuildingManager.Instance.HallTowerBuilding = this;
            }
        }
        protected virtual void OnDestroyBuilding()
        {
            Audio_Manager.instance.play("sfx_building_destroy");
        }
        public virtual bool CanUpgrade()
        {
            if (currentLevel >= buildingConfig.levelConfigs.Length)
            {
                return false;   
            }
            int levelUnlockCondition = buildingConfig.GetLevelData(currentLevel + 1).levelUnlockCondition;
            bool flagUnlockCondition = levelUnlockCondition <= SafeZoneController.Instance.CurrentZoneLevel;
            return flagUnlockCondition;
        }

        protected virtual void SpawnModel()
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
            }
            var levelData = buildingConfig.GetLevelData(currentLevel);
            currentModel = Instantiate(levelData.towerModel, modelContainer);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            currentModel.transform.localScale = Vector3.one;
            currentModel.transform.localScale = new Vector3(1f, 0.5f, 1f);
            currentModel.transform.DOScaleY(1, 0.1f).SetEase(Ease.OutQuad);

            baseDamageableEntity = currentModel.GetComponent<BaseDamageableEntity>();
            if (baseDamageableEntity != null)
            {
                baseDamageableEntity.onDeath.AddListener(() => {
                    if (buildingConfig.Type == BuildingType.Base)
                    {
                        WD.GameManager.Instance.HandleLevelFailed();
                    }
                });
                baseDamageableEntity.Initialize(levelData.Health);
            }

            buildingGraphics = currentModel.GetComponent<BuildingGraphics>();
        }
        public void SetModelWall(int lv)
        {
            if (buildingGraphics != null)
            {
                levelUnlockCondition = lv;
                buildingGraphics.SetModel(lv);
            }
        }
        public virtual void Upgrade()
        {
            currentLevel++;
            currentUpgradeCost = buildingConfig.GetCostUpgrade(currentLevel);
            SpawnModel();
            modelContainer.DOScaleY(1.2f, 0.2f).SetEase(Ease.OutBack).OnComplete(() => {
                modelContainer.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            });
        }

        public bool IsMaxLevel()
        {
            return currentLevel >= buildingConfig.levelConfigs.Length;
        }

        public virtual void Rebuild()
        {
            if (baseDamageableEntity != null && !baseDamageableEntity.IsDead)
            {
                baseDamageableEntity.HealAll();
                return;
            }
            SpawnModel();
            if (buildingConfig.Type == BuildingType.Wall)
                SetModelWall(levelUnlockCondition);
        }
    }
}