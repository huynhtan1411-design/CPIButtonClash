using UnityEngine;
public interface ISkillAction
{
    void Execute();
}

public class SpawnHeroAction : ISkillAction
{
    private string heroId;

    public SpawnHeroAction(string heroId)
    {
        this.heroId = heroId;
    }

    public void Execute()
    {
        Debug.Log($"Spawn Hero with ID: {heroId}");
    }
}