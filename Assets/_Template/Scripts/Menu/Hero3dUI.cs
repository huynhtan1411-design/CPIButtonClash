using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero3dUI : MonoSingleton<Hero3dUI>
{
    public List<GameObject> heroObjects = new List<GameObject>();

    public void ShowHeroById(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            Debug.LogWarning("Hero ID is null or empty!");
            return;
        }
        bool found = false;
        int index = 0;
        int idHeroShow = int.Parse(heroId);
        foreach (var hero in heroObjects)
        {
            if (hero != null && index == idHeroShow)
            {
              
                hero.SetActive(true);
                found = true;
            }
            else
            {
                if (hero != null)
                    hero.SetActive(false);
            }
            index++;
        }

        if (!found)
        {
            Debug.LogWarning($"No hero found with ID: {heroId}");
        }
    }
}
