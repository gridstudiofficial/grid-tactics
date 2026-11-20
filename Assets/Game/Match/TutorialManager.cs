using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI stepCounterText;
    [SerializeField] private Button nextButton;

    [Header("Secuencias por Momento")]
    [SerializeField] private List<TutorialSequence> startTutorials = new List<TutorialSequence>();
    [SerializeField] private List<TutorialSequence> midBattleTutorials = new List<TutorialSequence>();
    [SerializeField] private List<TutorialSequence> endBattleTutorials = new List<TutorialSequence>();

    public bool IsTutorialActive { get; private set; }

    private Queue<TutorialStep> currentSteps = new Queue<TutorialStep>();
    private int totalStepsInCurrentSequence = 0;
    private bool pausedByTutorial = false;
    private float previousTimeScale = 1f;
    private Action onTutorialClosedCallback;
    private HashSet<string> shownTutorialIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(AdvanceOrClose);
        }
    }

    private void Start()
    {
        TriggerMoment(TutorialTriggerMoment.StartOfBattle);
    }

    private void Update()
    {
        if (!IsTutorialActive) return;

        // Usar unscaledDeltaTime / GetKeyDown funciona aunque Time.timeScale sea 0
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            AdvanceOrClose();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTutorial();
        }
    }

    public void TriggerMoment(TutorialTriggerMoment moment, Action onComplete = null)
    {
        List<TutorialSequence> candidates = moment switch
        {
            TutorialTriggerMoment.StartOfBattle => startTutorials,
            TutorialTriggerMoment.MidBattle => midBattleTutorials,
            TutorialTriggerMoment.EndOfBattle => endBattleTutorials,
            _ => null
        };

        if (candidates == null || candidates.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        foreach (var seq in candidates)
        {
            if (seq != null && (!seq.showOnlyOnce || !shownTutorialIds.Contains(seq.tutorialId)))
            {
                PlaySequence(seq, onComplete);
                return;
            }
        }

        onComplete?.Invoke();
    }

    public void PlayById(string tutorialId, Action onComplete = null)
    {
        TutorialSequence found = FindSequenceById(tutorialId);
        if (found != null)
        {
            PlaySequence(found, onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private TutorialSequence FindSequenceById(string id)
    {
        foreach (var s in startTutorials) if (s != null && s.tutorialId == id) return s;
        foreach (var s in midBattleTutorials) if (s != null && s.tutorialId == id) return s;
        foreach (var s in endBattleTutorials) if (s != null && s.tutorialId == id) return s;
        return null;
    }

    public void PlaySequence(TutorialSequence sequence, Action onComplete = null)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (sequence.showOnlyOnce)
        {
            shownTutorialIds.Add(sequence.tutorialId);
        }

        currentSteps.Clear();
        foreach (var step in sequence.steps)
        {
            currentSteps.Enqueue(step);
        }

        totalStepsInCurrentSequence = currentSteps.Count;
        onTutorialClosedCallback = onComplete;
        IsTutorialActive = true;

        if (sequence.pauseGameWhileOpen && Time.timeScale > 0f)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            pausedByTutorial = true;
        }

        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        ShowNextStep();
    }

    private void ShowNextStep()
    {
        if (currentSteps.Count == 0)
        {
            CloseTutorial();
            return;
        }

        TutorialStep step = currentSteps.Dequeue();
        int currentIndex = totalStepsInCurrentSequence - currentSteps.Count;

        if (titleText != null) titleText.text = step.speakerOrTitle;
        if (bodyText != null) bodyText.text = step.message;
        if (stepCounterText != null) stepCounterText.text = $"{currentIndex} / {totalStepsInCurrentSequence}";
    }

    public void AdvanceOrClose()
    {
        if (!IsTutorialActive) return;

        if (currentSteps.Count > 0)
        {
            ShowNextStep();
        }
        else
        {
            CloseTutorial();
        }
    }

    public void CloseTutorial()
    {
        if (!IsTutorialActive) return;

        IsTutorialActive = false;
        currentSteps.Clear();

        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        if (pausedByTutorial)
        {
            Time.timeScale = previousTimeScale;
            pausedByTutorial = false;
        }

        Action callback = onTutorialClosedCallback;
        onTutorialClosedCallback = null;
        callback?.Invoke();
    }
}