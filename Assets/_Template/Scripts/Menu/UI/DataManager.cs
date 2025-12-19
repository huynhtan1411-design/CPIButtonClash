using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using UISystems;
using System;
using Unity.VisualScripting;
using System.Collections;
using System.Security.Cryptography;
using CLHoma;
using WD;
namespace TemplateSystems
{
    public enum TypeEquipment
    {
        SubWeapon,
        Ring,
        Necklace,
        Hat,
        Shirt,
        Gloves
    }

    public enum TypeRarity
    {
        Common,
        Rate,
        Epic,
        Epic1,
        Epic2,
        Legenrary,
        Legenrary1,
        Legenrary2,
        Immortal,
        Immortal1, 
        Immortal2
    }
    public enum TypeRarityHero
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3,
        Godlike = 4
    }
    public enum TypeTalents
    {
        Attack,
        HP,
        Defense
    }
    public enum TypeBonus
    {
        BonusDamage,
        BonusAtkRate,
        BonusAtkRange,
        BonusCritChance,
        BonusCritDamage,
        BonusResourceGenerate,
        BonusBulletSpd,
        BonusReduceDmgFromMob,
        BonusReduceCooldown,
        BonusGoldFromBattle,
        BonusMaxHP
    }
    public enum BonusItemType
    {
        Active,
        Lock
    }
    public enum ElementalType
    {
        Electric,
        Fire,
        Ice,
        Wind
    }
    public enum CardState { 
        Build,
        Available, 
        Lock,
        WaitReplace
    }
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance;
        private const string HERO_EQUIPMENT_KEY = "HeroEquipment";
        private const string BAG_EQUIPMENT_KEY = "BagEquipment";
        private const string BUILDING_COLLECTION_KEY = "BuildingCollection";
        private const string LEVEL_KEY = "CLHoma_Level";
        private const string PROGRESS_REWARD_STATE_KEY = "ProgressReward_Level_{0}_Milestone_{1}";
        private const string PROGRESS_VALUE_KEY = "ProgressReward_Level_{0}_Value";
        private const string LONGEST_TIME_SURVIVED_VALUE_KEY = "LongestTimeSurvival_Level_{0}_Value";
        private const string HERO_COLLECTION_KEY = "HeroCollection";
        public static int COINS;
        public static int LEVEL;
        [SerializeField] private HerosInfoConfig herosInfoData;
        [SerializeField] private EquipmentInfoConfig equipmentInfoData;
        [SerializeField] private RarityTypeColors rarityTypeColorsData;
        [SerializeField] private RarityTypeColorsHero rarityTypeColorsHeroData;
        [SerializeField] private UpgradeCostInfoConfig upgradeCostInfoConfigData;
        [SerializeField] private RankUpBonusInfoConfig rankUpBonusInfoConfigData;
        [SerializeField] private TalentsInfoConfig talentsInfoData;
        [SerializeField] private HeroCardRarityConfig rarityHeroCardConfig;
        [SerializeField] private HeroCardUpgradeCostConfig heroCardUpgradeCostConfig;
        [SerializeField] private SkillDatabase skillPassiveDatabaseConfig;
        [SerializeField] private SkillDatabase skillSpecialDatabaseConfig;
        [SerializeField] private SkillDatabase skillHeroDatabaseConfig;
        [SerializeField] private WaveDataConfig waveDataConfig;
        [SerializeField] private BuildingConfig buildingConfig;
        private Dictionary<string, EquipmentHeroData> heroEquipmentData = new Dictionary<string, EquipmentHeroData>();
        private Dictionary<string, TalentData> heroTalentData = new Dictionary<string, TalentData>();
        private List<EquipmentItemData> bagEquipmentData = new List<EquipmentItemData>();
        private List<HeroCardData> cardsCollectionData = new List<HeroCardData>();
        private List<BuildingCollectionItem> buildingCollectionData = new List<BuildingCollectionItem>();

       
        public List<EquipmentItemData> BagEquipmentData { get => bagEquipmentData; set => bagEquipmentData = value; }
        public Dictionary<string, EquipmentHeroData> HeroEquipmentData { get => heroEquipmentData; set => heroEquipmentData = value; }
        public List<BuildingCollectionItem> BuildingCollectionData { get => buildingCollectionData; set => buildingCollectionData = value; }
        public HerosInfoConfig HerosInfoData { get => herosInfoData; set => herosInfoData = value; }
        public EquipmentInfoConfig EquipmentInfoData { get => equipmentInfoData; set => equipmentInfoData = value; }
        public RarityTypeColors RarityTypeColorsData { get => rarityTypeColorsData; set => rarityTypeColorsData = value; }
        public UpgradeCostInfoConfig UpgradeCostInfoConfigData { get => upgradeCostInfoConfigData; set => upgradeCostInfoConfigData = value; }
        public TalentsInfoConfig TalentsInfoData => talentsInfoData;

        public RarityTypeColorsHero RarityTypeColorsHeroData { get => rarityTypeColorsHeroData; set => rarityTypeColorsHeroData = value; }
        public HeroCardRarityConfig RarityHeroCardConfig { get => rarityHeroCardConfig; }
        public List<HeroCardData> CardsCollectionData { get => cardsCollectionData; set => cardsCollectionData = value; }
        public HeroCardUpgradeCostConfig HeroCardUpgradeCostConfig { get => heroCardUpgradeCostConfig;}
        public SkillDatabase SkillPassiveDatabaseConfig { get => skillPassiveDatabaseConfig; }
        public SkillDatabase SkillSpecialDatabaseConfig { get => skillSpecialDatabaseConfig; }
        public SkillDatabase SkillHeroDatabaseConfig { get => skillHeroDatabaseConfig;}
        public WaveDataConfig WaveDataConfig { get => waveDataConfig; set => waveDataConfig = value; }
        public BuildingConfig BuildingConfig { get => buildingConfig; set => buildingConfig = value; }

        private ProgressRewardData progressRewardData = new ProgressRewardData();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            LoadData();
        }

        void Start()
        {
            //UIManager.onLevelCompleteSet += Reward;
        }

        private void OnDestroy()
        {
            //UIManager.onLevelCompleteSet -= Reward;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.R))
            {
                var rewards = GetRandomRewards();
                foreach (var item in rewards)
                {
                    EquipmentItemData data = new EquipmentItemData();
                    EquipmentInfData infData = DataManager.Instance.GetEquipmentInfo(item.ID);
                    data.Id = item.ID;
                    data.HP = item.HP;
                    data.Damage = item.Damage;
                    bagEquipmentData.Add(data);
                }
                SaveData();
            }
#endif
        }
        public void SaveData()
        {
            SaveHeroEquipment();
            SaveBagEquipment();
            SaveBuildingCollection();
        }

        public List<EquipmentItemData> SortBagEquipmentData(List<EquipmentItemData> equipmentItemData)
        {
            return equipmentItemData
                .Where(item => item != null)
                .OrderByDescending(item => item.Rarity)
                .ThenBy(item =>
                {
                    string middleTwoDigits = item.Id.Substring(2, 2);
                    int middleNumber = int.Parse(middleTwoDigits);
                    return middleNumber;
                })
                .ThenByDescending(item => item.Level)
                .ThenBy(item =>
                     {
                         string middleTwoDigits = item.Id.Substring(0, 2);
                         int middleNumber = int.Parse(middleTwoDigits);
                         return middleNumber;
                     })
                .ToList();
        }

        public HerosInfoData GetInfoDataHero(string idHero)
        {
            foreach (var hero in herosInfoData.Data)
                if (hero.Id == idHero)
                    return hero;
            return null;
        }
        public EquipmentInfData GetEquipmentInfo(string id)
        {
            foreach (var item in equipmentInfoData.Data)
                if (item.ID == id)
                    return item;
            return null;
        }

        public int GetPriceInfo(int indexlevel)
        {
            foreach (var item in upgradeCostInfoConfigData.Data)
                if (item.Level == (indexlevel + 1))
                    return item.Coin;
            return 0;
        }

        public List<EquipmentInfData> GetRandomRewards(int count = 5)
        {
            if (equipmentInfoData == null || equipmentInfoData.Data == null || equipmentInfoData.Data.Count == 0)
            {
                Debug.LogError("null data");
                return new List<EquipmentInfData>();
            }

            return equipmentInfoData.Data.OrderBy(x => 0).Take(3).ToList();
        }
        public int GetStar(TypeRarity type)
        {
            int star =0;
            switch(type)
            {

                case TypeRarity.Epic1:
                case TypeRarity.Immortal1:
                case TypeRarity.Legenrary1:
                star =1;
                break;
                case TypeRarity.Epic2:
                case TypeRarity.Immortal2:
                case TypeRarity.Legenrary2:
                star =2;
                 break;
            }
           return star;
        }


        public TypeEquipment GetEquipmentType(string id)
        {
            int idNumber = int.Parse(id);
            if (idNumber < 110000)
                return TypeEquipment.SubWeapon;
            else if (idNumber < 120000)
                return TypeEquipment.Ring;
            else if (idNumber < 130000)
                return TypeEquipment.Necklace;
            else if (idNumber < 140000)
                return TypeEquipment.Shirt;
            else if (idNumber < 150000)
                return TypeEquipment.Hat;
            else if (idNumber < 160000)
                return TypeEquipment.Gloves;
            return TypeEquipment.SubWeapon;
        }

        public ElementalType GetElementalHero(HeroeType heroeType)
        {
            switch (heroeType)
            {
                case HeroeType.Ninja: return ElementalType.Wind;
                case HeroeType.Archer: return ElementalType.Wind;
                case HeroeType.FireMage: return ElementalType.Fire;
                case HeroeType.IceWitch: return ElementalType.Ice;
                case HeroeType.LightningMaster: return ElementalType.Electric;
                case HeroeType.Berserker: return ElementalType.Fire;
            }
            return ElementalType.Wind;
        }
        public int GetTalentBonus(int level, string type)
        {
            int totalBonus = 0;
            if (TalentsInfoData != null)
            {
                foreach (var data in TalentsInfoData.Data)
                {
                    if (data.Level <= level)
                    {
                        switch (type)
                        {
                            case "HP":
                                totalBonus += data.HP;
                                break;
                            case "Attack":
                                totalBonus += data.Attack;
                                break;
                            case "Defense":
                                totalBonus += data.Defense;
                                break;
                        }
                    }
                }
            }
            return totalBonus;
        }
        public string GetImageID(string itemID)
        {
            if (string.IsNullOrEmpty(itemID) || itemID.Length < 2)
            {
                return string.Empty;
            }

            return itemID.Substring(itemID.Length - 2);
        }
        public Color GetColor(TypeRarity type)
        {
            var pair = Array.Find(rarityTypeColorsData.colorMapping, p => p.type == type);
            return pair.color;
        }

        public RankUpBonusInfoData GetRankUpBonusInfoData(TypeRarity type)
        {
            foreach (var item in rankUpBonusInfoConfigData.Data)
                if (item.Rarity == type)
                    return item;
            return null;
        }
        public static void AddCoins(int amount)
        {
            // Increase the amount of coins
            COINS += amount;

            // Update the coins
            UIManager.instance.UpdateCoins();

            // Save the amount of coins
            PlayerPrefsManager.SaveCoins(COINS);
        }

        public static int GetCoins()
        {
            COINS = PlayerPrefsManager.GetCoins();
            return COINS;
        }

        public void AddEquipmentIntoBag(List<EquipmentInfData > rewards)
        {
            foreach (var item in rewards)
            {
                EquipmentItemData data = new EquipmentItemData();
                EquipmentInfData infData = DataManager.Instance.GetEquipmentInfo(item.ID);
                data.Id = item.ID;
                data.HP = item.HP;
                data.Damage = item.Damage;
                bagEquipmentData.Add(data);
            }
            SaveData();
        }
        //Bonus
        public string GetInfoBonusEqupment(TypeBonus typeBonus)
        {
            string info = "";
            switch (typeBonus)
            {
                case TypeBonus.BonusDamage: info = "Damage +{0}%"; break;
                case TypeBonus.BonusAtkRate: info = "Attack Speed +{0}%"; break;
                case TypeBonus.BonusAtkRange: info = "Attack Range +{0}%"; break;
                case TypeBonus.BonusCritChance: info = "Critical Chance +{0}%"; break;
                case TypeBonus.BonusCritDamage: info = "Critical Damage +{0}%"; break;
                case TypeBonus.BonusResourceGenerate: info = "Resource Gain Per Tap +{0}%"; break;
                case TypeBonus.BonusBulletSpd: info = "Projectile Speed +{0}%"; break;
                case TypeBonus.BonusReduceDmgFromMob: info = "Reduce Damage from Mob +{0}%"; break;
                case TypeBonus.BonusReduceCooldown: info = "Reduce Cooldown +{0}%"; break;
                case TypeBonus.BonusGoldFromBattle: info = "Gold Gain +{0}%"; break;
                case TypeBonus.BonusMaxHP: info = "Max HP +{0}%"; break;
            }
            return info;
        }
        public (List<EquipmentInfData> Active, List<EquipmentInfData> Locked) GetActiveAndLockedEquipmentInfData(EquipmentInfData selectedEquipment)
        {
            var relatedList = GetRelatedEquipmentInfDataList(selectedEquipment);
            var activeList = relatedList
                .Where(equip => (int)equip.Rarity <= (int)selectedEquipment.Rarity)
                .ToList();
            var lockedList = relatedList
                .Where(equip => (int)equip.Rarity > (int)selectedEquipment.Rarity)
                .ToList();
             
            return (activeList, lockedList);
        }
        public List<EquipmentInfData> GetActiveEquipmentInfData(EquipmentInfData selectedEquipment)
        {
            var relatedList = GetRelatedEquipmentInfDataList(selectedEquipment);
            var activeList = relatedList
                .Where(equip => (int)equip.Rarity <= (int)selectedEquipment.Rarity)
                .ToList();

            return activeList;
        }
        public List<EquipmentInfData> GetRelatedEquipmentInfDataList(EquipmentInfData selectedEquipment)
        {
            return equipmentInfoData.Data
                .Where(equip => equip.Name == selectedEquipment.Name)
                .ToList();
        }
        //
        public int GetHeroHP(string idHero)
        {
            HerosInfoData hero = GetInfoDataHero(idHero);
            int hp = 0;
            hp = hero.HP;
            if (HeroEquipmentData == null)
                return hp;
            if (!HeroEquipmentData.ContainsKey(idHero))
                return hp;
            EquipmentHeroData equipmentHeroDataCur = HeroEquipmentData[idHero];
            hp += GetTalentBonus(equipmentHeroDataCur.Talents.LevelHP, TypeTalents.HP.ToString());
            foreach (var item in equipmentHeroDataCur.Equipment)
            {
                if (item.Value == null || string.IsNullOrEmpty(item.Value.Id))
                {
                    Debug.LogWarning($"Equipment item is null or has an empty ID! Key: {item.Key}");
                    continue;
                }
                if (!string.IsNullOrEmpty(item.Value.Id))
                {
                    hp += item.Value.HP;
                }
            }
            int hpMax = hp;
            foreach (var item in equipmentHeroDataCur.Equipment)
            {
                if (item.Value == null || string.IsNullOrEmpty(item.Value.Id))
                {
                    Debug.LogWarning($"Equipment item is null or has an empty ID! Key: {item.Key}");
                    continue;
                }
                if (!string.IsNullOrEmpty(item.Value.Id))
                {
                    EquipmentInfData equipmentInf = GetEquipmentInfo(item.Value.Id);
                    if (equipmentInf != null)
                    {
                        var activeList = GetActiveEquipmentInfData(equipmentInf);
                        float hpBonus = 0;
                        foreach (var bonus in activeList)
                            hpBonus += bonus.BonusMaxHP;
                        hp += Mathf.RoundToInt((hpBonus / 100f) * hpMax);
                    }
                }
            }

            return hp;
        }
        public int GetHeroAttack(string idHero)
        {
            int attack = 0;
            HerosInfoData hero = GetInfoDataHero(idHero);
            attack = hero.Damage;
            if (HeroEquipmentData == null)
                return attack;
            if (!HeroEquipmentData.ContainsKey(idHero))
                return attack;
            EquipmentHeroData equipmentHeroDataCur = HeroEquipmentData[idHero];
            attack += GetTalentBonus(equipmentHeroDataCur.Talents.LevelAttack, TypeTalents.Attack.ToString());
            foreach (var item in equipmentHeroDataCur.Equipment)
            {
                if (item.Value == null || string.IsNullOrEmpty(item.Value.Id))
                {
                    Debug.LogWarning($"Equipment item is null or has an empty ID! Key: {item.Key}");
                    continue;
                }
                if (!string.IsNullOrEmpty(item.Value.Id))
                {
                    attack += item.Value.Damage;
                }
            }
            int attackMax = attack;
            foreach (var item in equipmentHeroDataCur.Equipment)
            {
                if (item.Value == null || string.IsNullOrEmpty(item.Value.Id))
                {
                    Debug.LogWarning($"Equipment item is null or has an empty ID! Key: {item.Key}");
                    continue;
                }
                if (!string.IsNullOrEmpty(item.Value.Id))
                {
                    EquipmentInfData equipmentInf = GetEquipmentInfo(item.Value.Id);
                    if (equipmentInf != null)
                    {
                        var activeList = GetActiveEquipmentInfData(equipmentInf);
                        float attackBonus = 0;
                        foreach (var bonus in activeList)
                            attackBonus += bonus.BonusDamage;
                        attack += Mathf.RoundToInt((attackBonus / 100f) * attackMax);
                    }
                }
            }
            return attack;
        }
        #region Data
        public int GetLevel()
        {
            LEVEL = PlayerPrefs.GetInt(LEVEL_KEY, 0);
            return 0;
        }
        public void SaveLevel(int value)
        {
            LEVEL = value;
            PlayerPrefs.SetInt(LEVEL_KEY, value);
        }
        public void LoadData()
        {
            //Debug.LogError("LoadData");
            //COINS = PlayerPrefsManager.GetCoins();
            //if (COINS < 10)
            //    PlayerPrefsManager.SaveCoins(10);
            //COINS = PlayerPrefsManager.GetCoins();
            int dataTest = PlayerPrefs.GetInt("FirstReward", 0);
            if (dataTest == 0)
            {
                PlayerPrefs.SetInt("FirstReward", 1);
                var reward1 = GetEquipmentInfo("100002");
                var reward2 = GetEquipmentInfo("130010");
                List<EquipmentInfData> rewards = new List<EquipmentInfData>();
                rewards.Add(reward1);
                rewards.Add(reward2);
                foreach (var item in rewards)
                {
                    EquipmentItemData data = new EquipmentItemData();
                    data.Id = item.ID;
                    data.HP = item.HP;
                    data.Damage = item.Damage;
                    bagEquipmentData.Add(data);
                }
                PlayerPrefs.SetInt("COINS", 100);
                SaveData();
            }
            COINS = GetCoins();
            LoadHeroEquipment();
            LoadBagEquipment();
            LoadHeroCollection();
            LoadBuildingCollection();
        }

        private void SaveHeroEquipment()
        {
            string json = JsonConvert.SerializeObject(heroEquipmentData);
            PlayerPrefs.SetString(HERO_EQUIPMENT_KEY, json);
        }

        private void LoadHeroEquipment()
        {
            string json = PlayerPrefs.GetString(HERO_EQUIPMENT_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                Dictionary<string, EquipmentHeroData> wrapper = JsonConvert.DeserializeObject<Dictionary<string, EquipmentHeroData>>(json);
                heroEquipmentData = wrapper;
            }
            else
            {
                EquipmentHeroData dataHero = new EquipmentHeroData();

                heroEquipmentData = new Dictionary<string, EquipmentHeroData>();
            }
        }

        private void SaveBagEquipment()
        {
            string json = JsonConvert.SerializeObject(BagEquipmentData);
            PlayerPrefs.SetString(BAG_EQUIPMENT_KEY, json);

            UpdateNoticeEquipment();
        }

        private void LoadBagEquipment()
        {
            string json = PlayerPrefs.GetString(BAG_EQUIPMENT_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                List<EquipmentItemData> data = JsonConvert.DeserializeObject<List<EquipmentItemData>>(json);
                bagEquipmentData = data;
            }
            else
            {
                bagEquipmentData = new List<EquipmentItemData>();
            }
            UpdateNoticeEquipment();
        }
        public void SaveHeroCollection(List<HeroCardData> heroCollection)
        {
            string json = JsonConvert.SerializeObject(heroCollection);
            PlayerPrefs.SetString(HERO_COLLECTION_KEY, json);
            PlayerPrefs.Save();
        }

        public List<HeroCardData> LoadHeroCollection()
        {
            string json = PlayerPrefs.GetString(HERO_COLLECTION_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                cardsCollectionData = JsonConvert.DeserializeObject<List<HeroCardData>>(json);
                return cardsCollectionData;
            }
            else
            {
                cardsCollectionData = new List<HeroCardData>();
                var allHeroes = HerosInfoData.Data;
                int countCard = 0;
                foreach (var hero in allHeroes)
                {
                    if (hero.Id == "0")
                        continue;
                    countCard++;
                    HeroeType heroType = (HeroeType)Enum.Parse(typeof(HeroeType), hero.Id);
                    string id = ((int)heroType).ToString("D2");
                    HeroCardData card = new HeroCardData
                    {
                        Id = id,
                        HeroType = heroType,
                        CardState = CardState.Build,
                        Level = 1
                    };
                    if (countCard < 5)
                    {
                        cardsCollectionData.Add(card);
                    }
                }

                ////Test
                HeroeType heroTypet = (HeroeType)Enum.Parse(typeof(HeroeType), allHeroes[5].Id);
                HeroCardData cardTest = new HeroCardData
                {
                    Id = ((int)heroTypet).ToString("D2"),
                    HeroType = heroTypet,
                    CardState = CardState.Available,
                    Level = 1,
                    Quantity = 1
                };
                cardsCollectionData.Add(cardTest);
                ///

                SaveHeroCollection(cardsCollectionData);
                return cardsCollectionData;
            }
        }

        public void Reward(int star)
        {
            List<HeroCardData> Reward = RewardHeroCards();
        }
        public List<HeroCardData> RewardHeroCards()
        {
            List<HeroCardData> rewardedCards = new List<HeroCardData>();

            List<HeroCardData> availableHeroes = cardsCollectionData
                .Where(card => card.CardState != CardState.Lock)
                .ToList();

            if (availableHeroes.Count == 0)
            {
                Debug.LogWarning("No available heroes to reward.");
                return rewardedCards;
            }

            int heroCount;
            float heroRandomValue = UnityEngine.Random.Range(0f, 1f);
            if (heroRandomValue <= 0.75f)
            {
                heroCount = 1; 
            }
            else if (heroRandomValue <= 0.95f)
            {
                heroCount = 2; 
            }
            else
            {
                heroCount = 3; 
            }


            List<HeroCardData> selectedHeroes = new List<HeroCardData>();
            for (int i = 0; i < heroCount; i++)
            {
                HeroCardData randomHero;
                do
                {
                    randomHero = availableHeroes[UnityEngine.Random.Range(0, availableHeroes.Count)];
                } while (selectedHeroes.Contains(randomHero)); 
                selectedHeroes.Add(randomHero);
            }


            foreach (var heroCard in selectedHeroes)
            {
                int cardCount;
                float cardRandomValue = UnityEngine.Random.Range(0f, 1f);
                if (cardRandomValue <= 0.75f)
                {
                    cardCount = 1;
                }
                else if (cardRandomValue <= 0.95f)
                {
                    cardCount = 2; 
                }
                else
                {
                    cardCount = 5; 
                }


                heroCard.Quantity += cardCount;

                rewardedCards.Add(new HeroCardData
                {
                    Id = heroCard.Id,
                    HeroType = heroCard.HeroType,
                    RarityType = heroCard.RarityType,
                    Level = heroCard.Level,
                    CardState = heroCard.CardState,
                    Quantity = cardCount
                });
            }

            SaveHeroCollection(cardsCollectionData);
            return rewardedCards;
        }

        public List<HeroCardData> UnlockHero()
        {
            List<HeroCardData> unlockedHeroes = new List<HeroCardData>();

            var allHeroes = herosInfoData.Data;

            foreach (var hero in allHeroes)
            {
                if (hero.Id == "0" || cardsCollectionData.Any(card => card.Id == (int.Parse(hero.Id).ToString("D2"))))
                    continue;
                if (hero.LevelUnlock <= LEVEL)
                {
                    HeroeType heroType = (HeroeType)Enum.Parse(typeof(HeroeType), hero.Id);
                    HeroCardData newHeroCard = new HeroCardData
                    {
                        Id = ((int)heroType).ToString("D2"),
                        HeroType = heroType,
                        RarityType = TypeRarityHero.Common,
                        Level = 1,
                        CardState = CardState.Available,
                        Quantity = 1, 
                        IsNew = true  
                    };

                    cardsCollectionData.Add(newHeroCard);
                    unlockedHeroes.Add(newHeroCard);
                }
            }
            if(unlockedHeroes.Count >0)
              SaveHeroCollection(cardsCollectionData);

            return unlockedHeroes;
        }
        #endregion


        private int GetTalentPrice(int level)
        {
            foreach (var data in talentsInfoData.Data)
            {
                if (data.Level == level)
                    return data.Gold;
            }
            return int.MaxValue;
        }


        public bool CanUpgradeTalent()
        {
            foreach (var equipmentHeroDataCur in heroEquipmentData)
            {
                var talents = equipmentHeroDataCur.Value.Talents;
                int attackPrice = GetTalentPrice(talents.LevelAttack + 1);
                int hpPrice = GetTalentPrice(talents.LevelHP + 1);
                if (COINS >= attackPrice || COINS >= hpPrice)
                    return true;
            }
            return false;
        }

        public bool CanMergeItems()
        {
            var groupedItems = bagEquipmentData
                .GroupBy(item => item.Id)
                .Where(group => group.Count() >= 3);

            return groupedItems.Any();
        }
        public bool CanMergeItems(string id)
        {
            var groupedItems = bagEquipmentData
                .GroupBy(item => item.Id)
                .Where(group => group.Key == id && group.Count() >= 3);
            return groupedItems.Any();
        }

        private void UpdateNoticeEquipment()
        {
            bool hasNotice = CanMergeItems();
            if (hasNotice)
                UINoticeManager<string>.Instance.UpdateInfo(NoticeKey.Equipment.ToString(), NoticeStatus.Red);
            else
                UINoticeManager<string>.Instance.UpdateInfo(NoticeKey.Equipment.ToString(), NoticeStatus.None);
        }

        public int GetUpgradeCardHeroCount(int level)
        {
            foreach (var data in heroCardUpgradeCostConfig.Data)
            {
                if (data.Level == level)
                    return data.Card;
            }
            return int.MaxValue;
        }

        public WaveData GetWaveData(int wave)
        {
            foreach (var data in WaveDataConfig.Data)
            {
                if (data.Wave == wave)
                    return data;
            }
            return WaveDataConfig.Data[WaveDataConfig.Data.Count-1];
        }

        public int GetMaxLevelUpgradeCard()
        {
            return heroCardUpgradeCostConfig.Data.Count;
        }

        public int GetUpgradeCardHeroPrice(int level)
        {
            foreach (var data in heroCardUpgradeCostConfig.Data)
            {
                if (data.Level == (level + 1))
                    return data.Coin;
            }
            return int.MaxValue;
        }

        // --------------------- PROGRESS REWARD ---------------------
        public void SaveProgressRewardState(int level, int milestoneIndex, bool isCollected)
        {
            if (!progressRewardData.collectedMilestones.ContainsKey(level))
            {
                progressRewardData.collectedMilestones[level] = new Dictionary<int, bool>();
            }
            
            progressRewardData.collectedMilestones[level][milestoneIndex] = isCollected;
            
            string key = string.Format(PROGRESS_REWARD_STATE_KEY, level, milestoneIndex);
            PlayerPrefs.SetInt(key, isCollected ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        public bool GetProgressRewardState(int level, int milestoneIndex)
        {
            if (progressRewardData.collectedMilestones.ContainsKey(level) && 
                progressRewardData.collectedMilestones[level].ContainsKey(milestoneIndex))
            {
                return progressRewardData.collectedMilestones[level][milestoneIndex];
            }
            
            string key = string.Format(PROGRESS_REWARD_STATE_KEY, level, milestoneIndex);
            return PlayerPrefs.GetInt(key, 0) == 1;
        }
        
        public void SaveProgressValue(int level, float progress)
        {
            progressRewardData.progressValues[level] = progress;
            
            string key = string.Format(PROGRESS_VALUE_KEY, level);
            PlayerPrefs.SetFloat(key, progress);
            PlayerPrefs.Save();
        }
        
        public float GetProgressValue(int level)
        {
            if (progressRewardData.progressValues.ContainsKey(level))
            {
                return progressRewardData.progressValues[level];
            }
            
            string key = string.Format(PROGRESS_VALUE_KEY, level);
            return PlayerPrefs.GetFloat(key, -1f);
        }
        
        public void SaveAllProgressRewardStates(int level, bool[] milestoneStates)
        {
            if (!progressRewardData.collectedMilestones.ContainsKey(level))
            {
                progressRewardData.collectedMilestones[level] = new Dictionary<int, bool>();
            }
            
            for (int i = 0; i < milestoneStates.Length; i++)
            {
                progressRewardData.collectedMilestones[level][i] = milestoneStates[i];
                
                string key = string.Format(PROGRESS_REWARD_STATE_KEY, level, i);
                PlayerPrefs.SetInt(key, milestoneStates[i] ? 1 : 0);
            }
            
            PlayerPrefs.Save();
        }
        
        public bool[] GetAllProgressRewardStates(int level, int milestoneCount)
        {
            bool[] states = new bool[milestoneCount];
            
            if (progressRewardData.collectedMilestones.ContainsKey(level))
            {
                for (int i = 0; i < milestoneCount; i++)
                {
                    if (progressRewardData.collectedMilestones[level].ContainsKey(i))
                    {
                        states[i] = progressRewardData.collectedMilestones[level][i];
                    }
                    else
                    {
                        string key = string.Format(PROGRESS_REWARD_STATE_KEY, level, i);
                        states[i] = PlayerPrefs.GetInt(key, 0) == 1;
                    }
                }
            }
            else
            {
                for (int i = 0; i < milestoneCount; i++)
                {
                    string key = string.Format(PROGRESS_REWARD_STATE_KEY, level, i);
                    states[i] = PlayerPrefs.GetInt(key, 0) == 1;
                }
            }
            
            return states;
        }
        public void SaveLongestTimeSurvival(int level, int totalTimeSur)
        {
            string key = string.Format(LONGEST_TIME_SURVIVED_VALUE_KEY, level);
            int longestTime = PlayerPrefs.GetInt(key, 0);
            if (totalTimeSur > longestTime)
            {
                PlayerPrefs.SetInt(key, totalTimeSur);
            }
        }
        public int GetLongestTimeSurvival(int level)
        {
            string key = string.Format(LONGEST_TIME_SURVIVED_VALUE_KEY, level);
            return PlayerPrefs.GetInt(key, 0);
        }
        
        public void CheatUnlockAllHeroes()
        {
            var allHeroes = herosInfoData.Data;

            foreach (var hero in allHeroes)
            {
                if (hero.Id == "0")
                    continue;
                    
                string heroId = (int.Parse(hero.Id)).ToString("D2");
                bool heroExists = cardsCollectionData.Any(card => card.Id == heroId);
                
                if (!heroExists)
                {
                    HeroeType heroType = (HeroeType)Enum.Parse(typeof(HeroeType), hero.Id);
                    HeroCardData newHeroCard = new HeroCardData
                    {
                        Id = heroId,
                        HeroType = heroType,
                        RarityType = TypeRarityHero.Common,
                        Level = 1,
                        CardState = CardState.Available,
                        Quantity = 10,
                        IsNew = true
                    };

                    cardsCollectionData.Add(newHeroCard);
                }
            }
            
            SaveHeroCollection(cardsCollectionData);
            Debug.Log("All heroes unlocked!");
        }

        public void SaveBuildingCollection()
        {
            try
            {
                string json = JsonConvert.SerializeObject(buildingCollectionData);
                PlayerPrefs.SetString(BUILDING_COLLECTION_KEY, json);
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save building collection: {ex.Message}");
                // Fallback: save empty list to prevent infinite loops
                PlayerPrefs.SetString(BUILDING_COLLECTION_KEY, "[]");
                PlayerPrefs.Save();
            }
        }

        private void LoadBuildingCollection()
        {
            string json = PlayerPrefs.GetString(BUILDING_COLLECTION_KEY, "");
            if (!string.IsNullOrEmpty(json) && json != "[]")
            {
                try
                {
                    buildingCollectionData = JsonConvert.DeserializeObject<List<BuildingCollectionItem>>(json);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to load building collection: {ex.Message}");
                    buildingCollectionData = new List<BuildingCollectionItem>();
                    AddBuildingsToCollection(buildingConfig.InitializeData);
                }
            }
            else
            {
                buildingCollectionData = new List<BuildingCollectionItem>();
                AddBuildingsToCollection(buildingConfig.InitializeData);
            }
        }

        public void AddBuildingToCollection(BuildingData buildingData)
        {
            if (buildingData != null && !string.IsNullOrEmpty(buildingData.ID))
            {
                buildingCollectionData.Add(new BuildingCollectionItem(buildingData.ID, 1, true));
                SaveBuildingCollection();
            }
        }

        public void AddBuildingsToCollection(List<BuildingData> buildings)
        {
            Debug.LogError(buildings.Count);
            if (buildings != null && buildings.Count > 0)
            {
                foreach (var building in buildings)
                {
                    if (!string.IsNullOrEmpty(building.ID))
                    {
                        if(building.ID == "0")
                          buildingCollectionData.Add(new BuildingCollectionItem(building.ID, 1, true, 2));
                        else
                          buildingCollectionData.Add(new BuildingCollectionItem(building.ID, 1, true, 0));
                    }
                }
                SaveBuildingCollection();
            }
        }

        // Get BuildingData by ID from config
        public BuildingData GetBuildingDataByID(string buildingID)
        {
            if (buildingConfig != null && buildingConfig.Data != null)
            {
                return buildingConfig.Data.FirstOrDefault(building => building.ID == buildingID);
            }
            return null;
        }

        // Get building collection item by ID
        public BuildingCollectionItem GetBuildingCollectionItem(string buildingID)
        {
            return buildingCollectionData.FirstOrDefault(item => item.buildingID == buildingID);
        }

        // Update building level
        public void UpdateBuildingLevel(string buildingID, int newLevel)
        {
            var item = GetBuildingCollectionItem(buildingID);
            if (item != null)
            {
                item.Level = newLevel;
                SaveBuildingCollection();
            }
        }

        // Check if building is unlocked
        public bool IsBuildingUnlocked(string buildingID)
        {
            var item = GetBuildingCollectionItem(buildingID);
            return item != null && item.isUnlocked;
        }

        // Get all unlocked buildings
        public List<BuildingData> GetUnlockedBuildings()
        {
            List<BuildingData> unlockedBuildings = new List<BuildingData>();
            foreach (var item in buildingCollectionData)
            {
                if (item.isUnlocked)
                {
                    var buildingData = GetBuildingDataByID(item.buildingID);
                    if (buildingData != null)
                    {
                        unlockedBuildings.Add(buildingData);
                    }
                }
            }
            return unlockedBuildings;
        }

        // Cheat function to unlock all buildings
        public void CheatUnlockAllBuildings()
        {
            // Clear existing collection
            buildingCollectionData.Clear();

            // Add all buildings from config with max level
            if (buildingConfig != null && buildingConfig.Data != null)
            {
                foreach (var building in buildingConfig.Data)
                {
                    if (!string.IsNullOrEmpty(building.ID))
                    {
                        buildingCollectionData.Add(new BuildingCollectionItem(building.ID, building.levelConfigs.Length, true));
                    }
                }
                SaveBuildingCollection();
            }
        }

        public List<BuildingCollectionItem> RewardBuildingCards()
        {
            List<BuildingCollectionItem> rewardedBuildings = new List<BuildingCollectionItem>();

            List<BuildingCollectionItem> availableBuildings = buildingCollectionData
                .Where(building => building.isUnlocked)
                .ToList();

            if (availableBuildings.Count == 0)
            {
                Debug.LogWarning("No available buildings to reward.");
                return rewardedBuildings;
            }

            int buildingCount;
            float buildingRandomValue = UnityEngine.Random.Range(0f, 1f);
            if (buildingRandomValue <= 0.75f)
            {
                buildingCount = 1;
            }
            else if (buildingRandomValue <= 0.95f)
            {
                buildingCount = 2;
            }
            else
            {
                buildingCount = 3;
            }

            List<BuildingCollectionItem> selectedBuildings = new List<BuildingCollectionItem>();
            for (int i = 0; i < buildingCount; i++)
            {
                BuildingCollectionItem randomBuilding;
                do
                {
                    randomBuilding = availableBuildings[UnityEngine.Random.Range(0, availableBuildings.Count)];
                } while (selectedBuildings.Contains(randomBuilding));
                selectedBuildings.Add(randomBuilding);
            }

            foreach (var buildingItem in selectedBuildings)
            {
                int cardCount;
                float cardRandomValue = UnityEngine.Random.Range(0f, 1f);
                if (cardRandomValue <= 0.75f)
                {
                    cardCount = 1;
                }
                else if (cardRandomValue <= 0.95f)
                {
                    cardCount = 2;
                }
                else
                {
                    cardCount = 5;
                }

                buildingItem.Quantity += cardCount;

                rewardedBuildings.Add(new BuildingCollectionItem(
                    buildingItem.buildingID,
                    buildingItem.Level,
                    buildingItem.isUnlocked,
                    cardCount
                ));
            }

            SaveBuildingCollection();
            return rewardedBuildings;
        }

        public List<BuildingCollectionItem> UnlockBuilding()
        {
            List<BuildingCollectionItem> unlockedBuildings = new List<BuildingCollectionItem>();

            var allBuildings = buildingConfig.Data;

            foreach (var building in allBuildings)
            {
                // Skip if building is already in collection
                if (buildingCollectionData.Any(item => item.buildingID == building.ID))
                    continue;

                // Check if building should be unlocked at current level
                if (building.LevelIndexUnlock <= LEVEL)
                {
                    BuildingCollectionItem newBuildingItem = new BuildingCollectionItem
                    {
                        buildingID = building.ID,
                        Level = 1,
                        isUnlocked = true,
                        Quantity = 1
                    };

                    buildingCollectionData.Add(newBuildingItem);
                    unlockedBuildings.Add(newBuildingItem);
                    Debug.Log($"Unlocked new building: {building.ID} at level {LEVEL}");
                }
            }

            if (unlockedBuildings.Count > 0)
            {
                SaveBuildingCollection();
                Debug.Log($"Unlocked {unlockedBuildings.Count} new buildings");
            }

            return unlockedBuildings;
        }
    }

    [System.Serializable]
    public class HeroEquipmentWrapper
    {
        public Dictionary<string, Dictionary<string, Dictionary<string, int>>> data;
    }
    [System.Serializable]
    public class EquipmentItemData
    {
        public string Id;
        public int Level;
        public int HP;
        public int Damage;
        public TypeRarity Rarity;
    }
    [System.Serializable]
    public class EquipmentInfData
    {
        public string ID;
        public string Name;
        public TypeEquipment Type;
        public TypeRarity Rarity;
        public int BasePrice;
        public int Damage;
        public int HP;
        public string Description;
        public float BonusDamage;
        public float BonusAtkRate;
        public float BonusAtkRange;
        public float BonusCritChance;
        public float BonusCritDamage;
        public float BonusResourceGenerate;
        public float BonusBulletSpd;
        public float BonusReduceDmgFromMob;
        public float BonusReduceCooldown;
        public float BonusGoldFromBattle;
        public float BonusMaxHP;
    }
    [System.Serializable]
    public class EquipmentHeroData
    {
        public Dictionary<string, EquipmentItemData> Equipment = new Dictionary<string, EquipmentItemData>();
        public TalentData Talents = new TalentData();
        public (int totalHP, int totalDamage) CalculateTotalStats(string heroId)
        {
            int totalHP = 0;
            int totalDamage = 0;

            var heroInfo = DataManager.Instance.GetInfoDataHero(heroId);
            if (heroInfo != null)
            {
                totalHP = heroInfo.HP;
                totalDamage = heroInfo.Damage;
            }

            foreach (var equipment in Equipment.Values)
            {
                totalHP += equipment.HP;
                totalDamage += equipment.Damage;
            }

            return (totalHP, totalDamage);
        }

        public int GetTotalHP(string heroId)
        {
            return CalculateTotalStats(heroId).totalHP;
        }

        public int GetTotalDamage(string heroId)
        {
            return CalculateTotalStats(heroId).totalDamage;
        }
    }
    [System.Serializable]
    public class HerosInfoData
    {
        public string Id;
        public string Name;
        public int HP;
        public int Damage;
        public int LevelUnlock;
    }
    [System.Serializable]
    public class UpgradeCostData
    {
        public int Level;
        public int Coin;
    }
    [System.Serializable]
    public class RankUpBonusInfoData
    {
        public TypeRarity Rarity;
        public int UpgradeLvMax;
        public int DamageLevelUp;
        public int HPLevelUp;
        public int DamageRankUp;
        public int HPRankUp;
    }
    [System.Serializable]
    public class TalentsInfoData
    {
        public int Level;
        public int Gold;
        public int HP;
        public int Attack;
        public int Defense;
    }
    public class TalentData
    {
        public int LevelHP;
        public int LevelAttack;
        public int LevelDefense;
    }
    [System.Serializable]
    public class BagEquipmentWrapper
    {
        public List<EquipmentItemData> data;
    }
    [System.Serializable]
    public class BonusItemData
    {
        public BonusItemType Type; 
        public string TextInfo;   
        public TypeRarity Rarity; 
    }
    [System.Serializable]
    public enum TypeCard
    {
        Hero,
        Skill
    }
    [System.Serializable]
    public class HeroCardData
    {
        public string Id;
        public HeroeType HeroType;
        public TypeRarityHero RarityType;
        public int Level;
        public CardState CardState;
        public int Quantity;
        public bool IsNew;
        public TypeCard CardType; 
        public SkillData SkillData;
    }
    [System.Serializable]
    public class ProgressRewardData
    {
        public Dictionary<int, Dictionary<int, bool>> collectedMilestones = new Dictionary<int, Dictionary<int, bool>>();
        public Dictionary<int, float> progressValues = new Dictionary<int, float>();
    }

    [System.Serializable]
    public class HeroCardUpgradeCostData
    {
        public int Level;
        public int Card;
        public int Coin;
    }
    [System.Serializable]
    public class WaveData
    {
        public int Wave;
        public int SilverReward;
    }
    
    [System.Serializable]
    public class BuildingCollectionItem
    {
        public string buildingID;
        public int Level = 1;
        public int Quantity = 0;
        public bool isUnlocked = true;
        
        public BuildingCollectionItem()
        {
        }
        
        public BuildingCollectionItem(string id, int level = 1, bool unlocked = true, int quantity = 0)
        {
            buildingID = id;
            Level = level;
            isUnlocked = unlocked;
            Quantity = quantity;
        }
    }
}
