using UnityEngine;

namespace WD
{
    [System.Serializable]
    public class StatValue
    {
        [SerializeField] private float baseValue;
        private float currentValue;

        public float firstValue => baseValue;
        public float value => currentValue;

        public StatValue(float baseValue)
        {
            this.baseValue = baseValue;
            this.currentValue = baseValue;
        }

        public void SetBaseValue(float value)
        {
            baseValue = value;
            currentValue = value;
        }

        public void ModifyCurrentValue(float modifier)
        {
            currentValue = baseValue * modifier;
        }

        public void Reset()
        {
            currentValue = baseValue;
        }

        public float Random()
        {
            return UnityEngine.Random.Range(baseValue * 0.9f, baseValue * 1.1f);
        }
    }
} 