using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TurnUI : MonoBehaviour
{
    [Header("Panel Ficha Activa")]
    [SerializeField] private GameObject activeUnitPanel;
    [SerializeField] private Image activeUnitSprite;
    [SerializeField] private TextMeshProUGUI activeUnitNameText;
    [SerializeField] private TextMeshProUGUI activeUnitFatigueText;

    [Header("Cola de Siguientes Fichas")]
    [SerializeField] private Transform queueContainer; // Layout Group en el Canvas
    [SerializeField] private GameObject queueItemPrefab; // Prefab UI para cada ficha en la cola

    void Start()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnActiveUnitChanged += UpdateActiveUnitUI;
            TurnManager.Instance.OnQueueUpdated += UpdateQueueUI;
        }
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnActiveUnitChanged -= UpdateActiveUnitUI;
            TurnManager.Instance.OnQueueUpdated -= UpdateQueueUI;
        }
    }

    private void UpdateActiveUnitUI(Unit activeUnit)
    {
        if (activeUnit == null)
        {
            activeUnitPanel.SetActive(false);
            return;
        }

        activeUnitPanel.SetActive(true);
        activeUnitNameText.text = $"{activeUnit.id}";
        activeUnitFatigueText.text = $"{activeUnit.fatigue}";

        // Obtener el sprite visual de la ficha actual
        SpriteRenderer sr = activeUnit.GetComponent<SpriteRenderer>();
        if (sr != null && activeUnitSprite != null)
        {
            activeUnitSprite.sprite = sr.sprite;
            activeUnitSprite.color = sr.color;
        }
    }

    private void UpdateQueueUI(List<Unit> queueUnits)
    {
        // Limpiar cola anterior
        foreach (Transform child in queueContainer)
        {
            Destroy(child.gameObject);
        }

        // Rellenar con las unidades ordenadas por fatiga
        foreach (var unit in queueUnits)
        {
            if (queueItemPrefab != null)
            {
                GameObject item = Instantiate(queueItemPrefab, queueContainer);
                SpriteRenderer srU = unit.GetComponent<SpriteRenderer>();
                Image sr = item.GetComponentInChildren<Image>();
                TextMeshProUGUI label = item.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = $"{unit.id}    {unit.fatigue}";
                }
                if(sr != null)
                {
                    sr.sprite = srU.sprite;
                    sr.color = srU.color;
                }

            }
        }
    }
}