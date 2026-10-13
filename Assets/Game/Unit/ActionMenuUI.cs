using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ActionMenuUI : MonoBehaviour
{
    public static ActionMenuUI Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Transform buttonContainer;      // Vertical Layout Group en el Canvas
    [SerializeField] private GameObject actionButtonPrefab;  // Prefab de Botón con Texto

    private Unit currentUnit;
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private List<UnitAction> validActionsForUnit = new List<UnitAction>();
    private int selectedButtonIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void ShowMenu(Unit unit)
    {
        currentUnit = unit;
        if (menuPanel != null) menuPanel.SetActive(true);
        RefreshButtons();
        selectedButtonIndex = 0;
        HighlightSelectedButton();
    }

    public void HideMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        foreach (var btn in spawnedButtons)
        {
            Destroy(btn);
        }
        spawnedButtons.Clear();
    }

    private void RefreshButtons()
    {
        foreach (var btn in spawnedButtons) Destroy(btn);
        spawnedButtons.Clear();
        validActionsForUnit.Clear();

        // Filtrar solo las acciones que se pueden ejecutar en este estado
        foreach (var action in currentUnit.availableActions)
        {
            if (action.CanExecute(currentUnit))
            {
                validActionsForUnit.Add(action);
            }
        }

        // Crear los botones en la UI
        foreach (var action in validActionsForUnit)
        {
            if (actionButtonPrefab != null && buttonContainer != null)
            {
                GameObject btnObj = Instantiate(actionButtonPrefab, buttonContainer);
                Button btnComponent = btnObj.GetComponent<Button>();
                TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();

                if (label != null) label.text = action.ActionName;

                UnitAction capturedAction = action;
                btnComponent.onClick.AddListener(() => {
                    HideMenu();
                    capturedAction.Execute(currentUnit);
                });

                spawnedButtons.Add(btnObj);
            }
        }
    }

    private void Update()
    {
        if (menuPanel == null || !menuPanel.activeSelf || validActionsForUnit.Count == 0) return;

        // Navegación por teclado (W / S o Flechas Arriba / Abajo)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedButtonIndex = Mathf.Max(0, selectedButtonIndex - 1);
            HighlightSelectedButton();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedButtonIndex = Mathf.Min(validActionsForUnit.Count - 1, selectedButtonIndex + 1);
            HighlightSelectedButton();
        }

        // Confirmar selección con Enter o Espacio
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteSelectedAction();
        }
    }

    private void HighlightSelectedButton()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            Image img = spawnedButtons[i].GetComponent<Image>();
            if (img != null)
            {
                // Cambia de color el fondo del botón seleccionado por teclado
                img.color = (i == selectedButtonIndex) ? Color.yellow : Color.white;
            }
        }
    }

    private void ExecuteSelectedAction()
    {
        if (selectedButtonIndex >= 0 && selectedButtonIndex < validActionsForUnit.Count)
        {
            UnitAction action = validActionsForUnit[selectedButtonIndex];
            HideMenu();
            action.Execute(currentUnit);
        }
    }
}