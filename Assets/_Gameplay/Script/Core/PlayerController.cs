using Cinemachine;
using CLHoma.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplateSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLHoma
{
    public class PlayerController : ManualSingletonMono<PlayerController>
    {
        #region Serialized Fields
        [Header("Player Healthbar")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Character Settings")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private CharactersDatabase database;
        [SerializeField] private PlayerStatsData baseStatsData;
        [SerializeField] private Transform target;
        [SerializeField] private SlotBehavior[] slots;
        [SerializeField] private Transform slotMain;
        [SerializeField] private CinemachineVirtualCamera cameraPlayer;
        #endregion

        #region Properties
        public Transform Target => target;
        private static PlayerStatsManager statsManager;
        public static PlayerStatsManager StatsManager => statsManager;
        public static Character MainCharacter => Instance.GetCharacter(HeroeType.Main);
        private List<CharacterBehaviour> characters = new List<CharacterBehaviour>();
        public List<CharacterBehaviour> Characters => characters;
        public bool IsFullHeroes => (characters.Count -1) == slots.Length;
        private int countSpawnHero;
        public int CountSpawnHero { get => countSpawnHero; set => countSpawnHero = value; }
        #endregion

        #region Events
        public event Action<CharacterBehaviour> OnCharacterSpawned;
        public event Action<CharacterBehaviour> OnCharacterUpgraded;
        public event Action OnStatsChanged;
        #endregion

        #region Initialization
        public void Initialise()
        {
            InitializeStatsManager();
            StartHealthAndResourceRegeneration();
        }

        private void InitializeStatsManager()
        {
            statsManager = new PlayerStatsManager(baseStatsData);
            statsManager.OnChangeStats += HandleStatsChanged;
        }

        private void HandleStatsChanged()
        {
            UpdateHealthUI();
            UpdateResourceUI();
            OnStatsChanged?.Invoke();
        }

        private void UpdateHealthUI()
        {
            if (healthBar != null && healthText != null)
            {
                healthBar.value = statsManager.CurrentHealth / statsManager.maxHealth;
                healthText.text = $"{(int)statsManager.CurrentHealth}/{(int)statsManager.maxHealth}";
            }
        }

        private void UpdateResourceUI()
        {
            if (UISystems.UIManager.instance?.UIGameplayCtr != null)
            {
                UISystems.UIManager.instance.UIGameplayCtr.UpdateResourcePerSecUI(
                    statsManager.resourcePerSecond * statsManager.resourceMultiplier);
            }
        }

        private void StartHealthAndResourceRegeneration()
        {
            StartCoroutine(statsManager.RegenerateHealthOverTime());
            //StartCoroutine(statsManager.RegenerateResourcePerSecond());
        }

        public static void ToggleHealthBar(bool value)
        {
            if (Instance.healthBar != null)
            {
                Instance.healthBar.gameObject.SetActive(value);
            }
        }
        #endregion

        #region Character Management
        public void Reload()
        {
            //statsManager.InitializeStats();
            //ClearAllCharacters();
            SpawnMainCharacter();
            //countSpawnHero = 0;
        }

        public void ClearAllCharacters()
        {
            foreach (var character in characters)
            {
                if (character != null && character.gameObject != null)
                {
                    Destroy(character.gameObject);
                }
            }
            characters.Clear();
        }

        public Character GetCharacter(HeroeType type)
        {
            return database?.GetCharacter(type);
        }

        private void SpawnMainCharacter()
        {
            Vector3 point = slotMain.position;
            CharacterBehaviour character = SummonHero(HeroeType.Main, point);
            cameraPlayer.Follow = character.transform;
            //if (characters.Count < slots.Length)
            //{
            //    Vector3 point = slotMain.position;
            //    SummonHero(HeroeType.Main, point);
            //}
            //else
            //{
            //    Debug.LogError("Cannot spawn main character: no available slots");
            //}
        }

        public void SpawnCharacter(SlotBehavior slot, HeroeType type, TypeRarityHero typeRarityHero = TypeRarityHero.Common, HeroCardData cardData = null)
        {
            if (IsFullHeroes)
            {
                Debug.LogError("Cannot spawn more characters: all slots are full");
                return;
            }

            //if (HasCharacter(type))
            //{
            //    Debug.LogWarning($"Character of type {type} already exists");
            //    return;
            //}
            SpawnHeroAtSlot(slot, type, typeRarityHero, cardData);
        }

        public void UpgradeCharacter(HeroeType type)
        {
            if (!HasCharacter(type))
            {
                Debug.LogWarning($"Cannot upgrade character of type {type}: not found");
                return;
            }

            CharacterBehaviour character = characters.FirstOrDefault(c => c.CharacterType == type);
            if (character != null)
            {
                character.UpgradeWeapon();
                OnCharacterUpgraded?.Invoke(character);
            }
        }
        public bool UpgradeCharacter(HeroeType type, TypeRarityHero typeRarity)
        {
            if (!HasCharacter(type))
            {
                Debug.LogWarning($"Cannot upgrade character of type {type}: not found");
                return false;
            }

            var eligibleCharacters = characters.Where(c => c.CharacterType == type && 
                                                        c.Data.RarityType == typeRarity &&
                                                        typeRarity != TypeRarityHero.Godlike).ToList();
            Debug.LogError($"UpgradeCharacter {type} {typeRarity} {eligibleCharacters.Count}");
            if (eligibleCharacters.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, eligibleCharacters.Count);
                CharacterBehaviour character = eligibleCharacters[randomIndex];
                
                character.UpgradeWeapon();
                OnCharacterUpgraded?.Invoke(character);
                return true;
            }

            return false;
        }
        public void SetLevelCharacter(CharacterBehaviour character, TypeRarityHero rarityHero)
        {

            if (character != null)
            {
                int level = rarityHero.GetHashCode();
                character.UpgradeWeapon(level);
                OnCharacterUpgraded?.Invoke(character);

            }
        }
        private CharacterBehaviour SummonMain(HeroeType type, Vector3 pos, TypeRarityHero typeRarityHero = TypeRarityHero.Common)
        {
            Character character = GetCharacter(type);
            character.RarityType = typeRarityHero;
            if (character == null)
            {
                Debug.LogError($"Failed to get character data for type {type}");
                return null;
            }

            GameObject playerObject = Instantiate(playerPrefab, pos, Quaternion.identity);
            playerObject.name = $"[CHARACTER_{type}]";

            CharacterBehaviour characterBehaviour = playerObject.GetComponent<CharacterBehaviour>();
            if (characterBehaviour == null)
            {
                Debug.LogError("PlayerPrefab does not have CharacterBehaviour component");
                Destroy(playerObject);
                return null;
            }
            characters.Add(characterBehaviour);

            characterBehaviour.Initialise(character, pos);
            SetLevelCharacter(characterBehaviour, typeRarityHero);
            ApplyEquipmentStats(characterBehaviour, character);
            OnCharacterSpawned?.Invoke(characterBehaviour);

            return characterBehaviour;
        }
        private CharacterBehaviour SummonHero(HeroeType type, Vector3 pos, TypeRarityHero typeRarityHero = TypeRarityHero.Common)
        {
            Character character = GetCharacter(type);
            character.RarityType = typeRarityHero;
            if (character == null)
            {
                Debug.LogError($"Failed to get character data for type {type}");
                return null;
            }

            GameObject playerObject = Instantiate(playerPrefab, pos, Quaternion.identity);
            playerObject.name = $"[CHARACTER_{type}]";
            
            CharacterBehaviour characterBehaviour = playerObject.GetComponent<CharacterBehaviour>();
            if (characterBehaviour == null)
            {
                Debug.LogError("PlayerPrefab does not have CharacterBehaviour component");
                Destroy(playerObject);
                return null;
            }
            characters.Add(characterBehaviour);

            characterBehaviour.Initialise(character, pos);
            SetLevelCharacter(characterBehaviour, typeRarityHero);
            ApplyEquipmentStats(characterBehaviour, character);
            OnCharacterSpawned?.Invoke(characterBehaviour);

            return characterBehaviour;
        }
        public void SpawnHeroInEmptySlot(HeroeType type, TypeRarityHero rarity = TypeRarityHero.Common, HeroCardData cardData = null)
        {
            foreach (SlotBehavior slot in slots)
            {
                if (slot.transform.GetChild(0).childCount == 0)
                {
                    Character character = GetCharacter(type);
                    character.RarityType = rarity;
                    if(cardData != null)
                    character.Level = cardData.Level;
                    if (character == null)
                    {
                        Debug.LogError($"Failed to get character data for type {type}");
                        return;
                    }

                    GameObject hero = Instantiate(playerPrefab, slot.transform.position, Quaternion.identity);
                    hero.transform.SetParent(slot.transform.GetChild(0));

                    CharacterBehaviour characterBehaviour = hero.GetComponent<CharacterBehaviour>();
                    if (characterBehaviour != null)
                    {
                        character.RarityType = rarity;
                        characterBehaviour.Initialise(character, slot.transform.position);
                        SetLevelCharacter(characterBehaviour, rarity);
                        characters.Add(characterBehaviour);
                        ApplyEquipmentStats(characterBehaviour, character);
                        OnCharacterSpawned?.Invoke(characterBehaviour);
                        //slot.SetCharacterBehaviour(characterBehaviour);
                    }
                    else
                    {
                        Debug.LogError("PlayerPrefab does not have CharacterBehaviour component");
                        Destroy(hero);
                    }

                    return;
                }
            }

            Debug.LogWarning("No empty slots available to spawn hero.");
        }

        public void SpawnHeroAtSlot(SlotBehavior slot, HeroeType type, TypeRarityHero rarity = TypeRarityHero.Common, HeroCardData cardData = null)
        {
            if (slot == null)
            {
                SpawnHeroInEmptySlot(type, rarity, cardData);
                countSpawnHero++;
                return;
            }

            if (slot.transform.GetChild(0).childCount > 0)
            {
                Debug.LogError($"Slot {slot.name} is not empty. Cannot spawn hero.");
                return;
            }

            Character character = GetCharacter(type);
            character.RarityType = rarity;
            if (cardData != null)
                character.Level = cardData.Level;

            if (character == null)
            {
                Debug.LogError($"Failed to get character data for type {type}");
                return;
            }

            GameObject hero = Instantiate(playerPrefab, slot.transform.position, Quaternion.identity);
            hero.transform.SetParent(slot.transform.GetChild(0));
            countSpawnHero++;
            CharacterBehaviour characterBehaviour = hero.GetComponent<CharacterBehaviour>();
            if (characterBehaviour != null)
            {
                character.RarityType = rarity;
                characterBehaviour.Initialise(character, slot.transform.position);
                SetLevelCharacter(characterBehaviour, rarity);
                characters.Add(characterBehaviour);
                ApplyEquipmentStats(characterBehaviour, character);
                OnCharacterSpawned?.Invoke(characterBehaviour);
                //slot.SetCharacterBehaviour(characterBehaviour);
            }
            else
            {
                Debug.LogError("PlayerPrefab does not have CharacterBehaviour component");
                Destroy(hero);
            }
        }

        private void ApplyEquipmentStats(CharacterBehaviour characterBehaviour, Character character)
        {
            try
            {
                ApplyStats(characterBehaviour, character.Id, character.Level);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error applying stats for character {character.Id}: {e.Message}");
                ApplyDefaultStats(characterBehaviour, character);
            }
        }
        private void ApplyDefaultStats(CharacterBehaviour characterBehaviour, Character character)
        {
            var dataManager = TemplateSystems.DataManager.Instance;
            if (dataManager != null)
            {
                var heroInfo = dataManager.GetInfoDataHero(character.Id);
                if (heroInfo != null)
                {
                    //statsManager.AddMaxHealth(heroInfo.HP);
                    characterBehaviour.SetDamage(heroInfo.Damage);

                    Debug.LogError(string.Format("HP {0} : ATK: {1}", heroInfo.HP, heroInfo.Damage));
                }
                else
                {
                    Debug.LogWarning($"No hero info found for id {character.Id}");
                }
            }
        }
        private void ApplyStats(CharacterBehaviour characterBehaviour, string id, int level = 0)
        {
            HerosInfoData hero = DataManager.Instance.GetInfoDataHero(id);
            //int hpAdd = DataManager.Instance.GetHeroHP(id) + hero.HP;
            int levelCheck = level -1;
            if (levelCheck < 0)
                levelCheck = 0;
            float addDamage = hero.Damage * levelCheck * 10f / 100f;
            int roundeaddDamage = Mathf.CeilToInt(addDamage);
            int damageAdd = DataManager.Instance.GetHeroAttack(id) + roundeaddDamage;
            //statsManager.AddMaxHealth(hpAdd);
            characterBehaviour.SetDamage(damageAdd);
        }
        public bool HasCharacter(HeroeType type)
        {
            return characters.Any(c => c != null && c.CharacterType == type);
        }


        public void UnhighlightAllSlot()
        {
            foreach (var slot in slots)
            {
                //slot.SetHighlight(SlotBehavior.SlotState.None);
            }
        }
        public void ToggleRecommendSlot(HeroCardData cardData, bool value)
        {
            //foreach(var slot in slots)
            //{
            //    if (!value)
            //    {
            //        slot.ToggleRecommend(false, Color.white);
            //        continue;
            //    }
            //    if (!slot.IsEmpty())
            //    {
            //        if (cardData.CardType == TypeCard.Hero)
            //        {
            //            if (slot.CharacterBehaviour.CanUpgradeWithCard(cardData))
            //            {
            //                slot.ToggleRecommend(true, Color.white);
            //            }
            //        }
            //        else if (cardData.CardType == TypeCard.Skill)
            //        {
            //            slot.ToggleRecommend(true, Color.yellow);
            //        }

            //    }
            //}
        }
        #endregion
    }
}