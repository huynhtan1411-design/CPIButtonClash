using System.Collections.Generic;
using UnityEngine;
public static class Define
{
    readonly public static List<int> heroOnlyLevels = new List<int> {1, 2};

    readonly public static int LIMIT_NUMBER_SKILL = 6;

    readonly public static List<HeroeType> heroesInTutorial = new List<HeroeType>
    {
        HeroeType.Ninja,
        HeroeType.Archer,
        HeroeType.IceWitch,
        HeroeType.Ninja,
        HeroeType.LightningMaster,
        HeroeType.Archer,
        HeroeType.Ninja,
        HeroeType.Archer,
        HeroeType.LightningMaster,
        HeroeType.Archer,
        HeroeType.Ninja,
        HeroeType.IceWitch,
        HeroeType.Archer,
        HeroeType.LightningMaster,
        HeroeType.Archer,

    };
}

public enum PlayerAnimationStates
{
    Idle,
    Run,
    Death,
    Gun,
    IdleGun,
    Attack,
    SawRotate,
    Carry
}

public enum PopupName
{
    ChooseSkill
}
public enum HeroeType
{
    Main = 0,
    LightningMaster = 1,
    FireMage = 2,
    Ninja = 3,
    Archer = 4,
    IceWitch = 5,
    Berserker = 6,
    Lancer = 7,
    GlacierArcher = 8

}
public enum SkillType
{
    SpawnHero,
    UpgradeSkill,
    Resource,
    SpecialAbility
}
public enum EnemyTier
{
    Regular = 0,
    Elite = 1,
    Boss = 2,
}

public enum WeaponType
{
    Crossbow = 0,
    Staff_Fire = 1,
    Shuriken = 2,
    Knife = 3,
    Bow = 4,
    Staff_Ice = 5,
    Staff_Lightning = 6,
    Sword = 7,
    Lance = 8,
    Sword_Ice = 9
}

public enum ElementType
{
    None = 0,
    Fire = 1,
    Ice = 2,
    Electric = 3,
    Wind = 4
}

public enum HitType
{
    Hit,
    HitResist,
    HitWeakness,
}

public enum TutorialStepID
{
    summon_hero = 0,
    merge_card = 1,
    merge_card_more = 2,
    Equipment_Menu = 3,
    Pick_Item = 4,
    Equip_Item = 5,
    Talent_Menu = 6,
    Learn_Talent = 7,
    ELEMENTAL = 8,
}
public enum NoticeKey
{
    Talent,
    Equipment,
    Equipment_Item,
    DeckBuilder
}