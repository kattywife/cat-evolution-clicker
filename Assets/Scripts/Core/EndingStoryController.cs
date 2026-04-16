using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndingStoryController : MonoBehaviour
{
    [Header("Сюжет")]
    [TextArea(3, 10)]
    public string[] storySentences;

    [Header("Элементы UI")]
    public TextMeshProUGUI storyText;
    public GameObject nextButton;
    public GameObject finalButtonsGroup;

    private int currentIndex = 0;

    private void OnEnable()
    {
        StartStory();
    }

    // --- НОВОЕ: Слушаем нажатие клавиш ---
    void Update()
    {
        // Проверяем нажатие Пробела
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Сработает только если кнопка "Далее" активна (видна на экране)
            // Это нужно, чтобы пробел не работал, когда уже появились кнопки "Выход/Рестарт"
            if (nextButton != null && nextButton.activeSelf)
            {
                OnNextClicked();
            }
        }
    }
    // -------------------------------------

    private void StartStory()
    {
        currentIndex = 0;

        if (finalButtonsGroup != null)
            finalButtonsGroup.SetActive(false);

        if (nextButton != null)
            nextButton.SetActive(true);

        DisplaySentence();
    }

    public void OnNextClicked()
    {
        currentIndex++;

        if (currentIndex < storySentences.Length)
        {
            DisplaySentence();
        }
        else
        {
            FinishStory();
        }
    }

    private void DisplaySentence()
    {
        if (storyText != null && storySentences.Length > 0)
        {
            if (currentIndex < storySentences.Length)
            {
                storyText.text = storySentences[currentIndex];
            }
        }
    }

    private void FinishStory()
    {
        if (nextButton != null)
            nextButton.SetActive(false);

        if (finalButtonsGroup != null)
            finalButtonsGroup.SetActive(true);
    }
}