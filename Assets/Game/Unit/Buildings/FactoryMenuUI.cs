using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Game.Unit.Builgings;
public class FactoryMenuUI : MonoBehaviour
{
    public static FactoryMenuUI Instance { get; private set; }

    [Header("Referencias UI Fábrica")]
    [SerializeField] private GameObject factoryPanel;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject unitButtonPrefab;

    private Factory currentFactory;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (factoryPanel != null) factoryPanel.SetActive(false);
    }

    public void ShowFactoryMenu(Factory factory)
    {
        currentFactory = factory;
        if (factoryPanel != null) factoryPanel.SetActive(true);
        RefreshList();
    }

    public void HideFactoryMenu()
    {
        if (factoryPanel != null) factoryPanel.SetActive(false);
        foreach (var btn in spawnedButtons) Destroy(btn);
        spawnedButtons.Clear();
    }

    private void RefreshList()
    {
        foreach (var btn in spawnedButtons) Destroy(btn);
        spawnedButtons.Clear();

        foreach (var unitData in currentFactory.availableUnitsToBuild)
        {
            if (unitButtonPrefab != null && buttonContainer != null)
            {
                GameObject btnObj = Instantiate(unitButtonPrefab, buttonContainer);
                Button btnComponent = btnObj.GetComponent<Button>();
                TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null)
                {
                    label.text = $"{unitData.unitName} - Coste: {unitData.cost} pts";
                }

                BuildableUnitData capturedData = unitData;
                btnComponent.onClick.AddListener(() => {
                    currentFactory.SpawnUnit(capturedData);
                    HideFactoryMenu();
                    MapCursor.Instance.CloseFactoryMenu();
                });

                spawnedButtons.Add(btnObj);
            }
        }
    }
}