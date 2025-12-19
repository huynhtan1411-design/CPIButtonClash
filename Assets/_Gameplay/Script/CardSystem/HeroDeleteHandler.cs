using CLHoma;
using CLHoma.Combat;
using System;
using TemplateSystems;
using UISystems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroDeleteHandler : MonoBehaviour
{
    [SerializeField] private GameObject deleteButtonPrefab;
    private GameObject deleteButtonInstance; 
    private Camera mainCamera;
    private CharacterBehaviour selectedHero;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Hero"))
                {
                    CharacterBehaviour hero = hit.collider.transform.parent.GetComponent<CharacterBehaviour>();
                    if (hero != null && hero.Data.Type != HeroeType.Main)
                    {
                        OnHeroClicked(hero);
                        return;
                    }
                    else
                    {
                        DeselectHero();
                    }
                }
            }
            DeselectHero();
        }

        //if (selectedHero != null && deleteButtonInstance != null)
        //{
        //    Vector3 screenPosition = mainCamera.WorldToScreenPoint(selectedHero.transform.position);
        //    deleteButtonInstance.transform.position = screenPosition;
        //}
    }

    public void OnHeroClicked(CharacterBehaviour hero)
    {
        if (selectedHero != null || hero == null)
        {
            DeselectHero();
        }

        selectedHero = hero;

        if (deleteButtonInstance == null)
        {
            deleteButtonInstance = Instantiate(deleteButtonPrefab, transform);
            deleteButtonInstance.GetComponent<Button>().onClick.AddListener(DeleteSelectedHero);
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(hero.transform.position);
        screenPosition.y += 0;
        screenPosition.x += 0;
        deleteButtonInstance.transform.position = screenPosition;
        deleteButtonInstance.SetActive(true);
    }

    private void DeleteSelectedHero()
    {
        if (selectedHero != null)
        {
            PlayerController.Instance.Characters.Remove(selectedHero);
            UIManager.instance.UIGameplayCtr.AddExperience(selectedHero.Data.RarityType);
            Destroy(selectedHero.gameObject);
            DeselectHero();
        }
    }

    private void DeselectHero()
    {
        if (deleteButtonInstance != null)
        {
            deleteButtonInstance.SetActive(false);
        }
        selectedHero = null;
    }

}