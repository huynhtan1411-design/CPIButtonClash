using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using CLHoma.Combat;
namespace WD
{
    public class ResourceBuildingBehaviour : BaseBuildingBehaviour
    {
        private int resourcePerPhase;

        public override void Initialize(BuildingData config)
        {
            base.Initialize(config);
            UpdateResourcePerPhase();

            // Subscribe to phase start event to generate resources
            GameManager.Instance.OnBuildPhaseStart.AddListener(() => {
                GenerateResources();
            });
        }

        private void UpdateResourcePerPhase()
        {
            // Get resource amount from current level config
            var levelData = buildingConfig.GetLevelData(currentLevel);
            resourcePerPhase = currentLevel;
        }

        public override void Upgrade()
        {
            base.Upgrade();
            UpdateResourcePerPhase();
        }

        private void GenerateResources()
        {
            BuildingManager.Instance.AddResources(resourcePerPhase);
        }
    }
}