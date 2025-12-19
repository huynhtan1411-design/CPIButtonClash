using System;

[Serializable]
public class PlayerData
{
    public PlayerMovementData playerMovementData;
    public PlayerStackData StackData;
}

[Serializable]
public class PlayerMovementData
{
    public float IdleSpeed = 4f;
    public float RotationSpeed = 230f;
}

[Serializable]
public class PlayerStackData
{
    public int StackLimit = 1000;
    public float StackoffsetX = 10;
    public float StackoffsetY = 10;
    public float StackoffsetZ = 10;
    public float AnimationDurition = 1;
}