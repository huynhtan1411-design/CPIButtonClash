using System;
using UnityEngine;

public static class Utils
{
    public static Vector3 GetRandomPositionAroundX(Vector3 center, float radius)
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
        float distance = UnityEngine.Random.Range(0f, radius);
        float x = center.x + distance * Mathf.Cos(angle);
        //float z = center.z + distance * Mathf.Sin(angle);
        return new Vector3(x, center.y, center.z);
    }
    public static Vector3 GetRandomPositionAround(Vector3 center, float radius)
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
        float distance = UnityEngine.Random.Range(0f, radius);
        float x = center.x + distance * Mathf.Cos(angle);
        float y = center.y + distance * Mathf.Cos(angle);
        float z = center.z + distance * Mathf.Sin(angle);
        return new Vector3(x, y, z);
    }

    public static double CalculateExpForNextLevel(int level, int baseExp = 100, float growthFactor = 1.2f)
    {
        if (level < 1)
            throw new ArgumentException("Level must be at least 1.");
        double requiredExp = baseExp * Math.Pow(growthFactor, level - 1);
        return (double)Math.Round(requiredExp);
    }

    public static T GetRandomFromList<T>(T[] list)
    {
        return list[UnityEngine.Random.Range(0, list.Length)];
    }


    public static int RandomInt(int num, int duration)
    {
        int result = UnityEngine.Random.Range(num - duration, num + duration);
        return Mathf.Abs(result);
    }

    public static int CalculateEnemyStat(int baseValue, int level, float growthPerLevel = 0.5f)
    {
        if (level < 1) return baseValue;
        float calculatedValue = baseValue + (level - 1) * baseValue * growthPerLevel;
        return Mathf.RoundToInt(calculatedValue);
    }

    public static float CalculateEnemyStatExact(float baseValue, int level, float growthPerLevel = 2.5f)
    {
        if (level < 1) return baseValue;
        return Mathf.Round(baseValue + (level - 1) * growthPerLevel);
    }

    public static (float, HitType) CalculateElementalDamage(
        float baseDamage,
        ElementType attackerElement,
        ElementType defenderElement,
        float resistanceFactor = 0.5f,
        float weaknessFactor = 0.5f
    )
    {
        if (attackerElement == ElementType.None || defenderElement == ElementType.None)
            return (baseDamage, HitType.Hit);

        if (attackerElement == defenderElement)
            return (baseDamage * resistanceFactor, HitType.HitResist);

        switch (attackerElement)
        {
            case ElementType.Fire:
                if (defenderElement == ElementType.Ice) 
                    return (baseDamage * (1f + weaknessFactor), HitType.HitWeakness);
                if (defenderElement == ElementType.Wind) 
                    return (baseDamage * resistanceFactor, HitType.HitResist);
                break;

            case ElementType.Ice:
                if (defenderElement == ElementType.Electric) 
                    return (baseDamage * (1f + weaknessFactor), HitType.HitWeakness);
                if (defenderElement == ElementType.Fire) 
                    return (baseDamage * resistanceFactor, HitType.HitResist);
                break;

            case ElementType.Electric:
                if (defenderElement == ElementType.Wind) 
                    return (baseDamage * (1f + weaknessFactor), HitType.HitWeakness);
                if (defenderElement == ElementType.Ice) 
                    return (baseDamage * resistanceFactor, HitType.HitResist);
                break;

            case ElementType.Wind:
                if (defenderElement == ElementType.Fire) 
                    return (baseDamage * (1f + weaknessFactor), HitType.HitWeakness);
                if (defenderElement == ElementType.Electric) 
                    return (baseDamage * resistanceFactor, HitType.HitResist);
                break;
        }

        return (baseDamage, HitType.Hit);
    }

    public static int RandomByWeight(float[] weights)
    {
        if (weights == null || weights.Length == 0)
            throw new ArgumentException("Weights array cannot be null or empty");

        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            if (weight < 0)
                throw new ArgumentException("Weights cannot be negative");
            totalWeight += weight;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentSum = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            currentSum += weights[i];
            if (randomValue <= currentSum)
                return i;
        }

        return weights.Length - 1; // Fallback to last index
    }

    public static int RandomByWeight(int[] weights)
    {
        if (weights == null || weights.Length == 0)
            throw new ArgumentException("Weights array cannot be null or empty");

        float[] floatWeights = new float[weights.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i] < 0)
                throw new ArgumentException("Weights cannot be negative");
            floatWeights[i] = weights[i];
        }

        return RandomByWeight(floatWeights);
    }
}
