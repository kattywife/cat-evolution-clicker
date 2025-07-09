using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // --- Переменные для геймплея ---
    public double score = 0;

    // --- Ссылки на объекты на сцене ---
    public TextMeshProUGUI scoreText; // Сюда перетаскиваем текст для счета
    public Transform catTransform;    // Сюда перетаскиваем котика

    // Этот метод вызывается при клике на котика
    public void OnCatClicked()
    {
        // Добавляем тестовое сообщение в консоль, чтобы убедиться, что клик работает
        Debug.Log("Клик по котику! Текущий счет: " + score);

        // Увеличиваем счет на 1
        score = score + 1;

        // Обновляем текст на экране
        UpdateScoreText();

        // Анимация клика: немного увеличиваем котика
        catTransform.localScale = new Vector3(1.1f, 1.1f, 1f); // Чуть больше исходного размера

        // Через 0.1 секунды вызываем метод, чтобы вернуть размер обратно
        Invoke("ResetCatScale", 0.1f);
    }

    // Этот метод возвращает котика к его обычному размеру
    private void ResetCatScale()
    {
        // Убедись, что здесь указан твой исходный размер котика!
        // Если ты его меняла, подставь свои значения. 5.3316 - это с твоего скриншота.
        catTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    // Этот метод обновляет текст счета на экране
    private void UpdateScoreText()
    {
        // Проверяем, что ссылка на текст не пустая, чтобы избежать ошибок
        if (scoreText != null)
        {
            scoreText.text = score.ToString("F0"); // "F0" означает показать число без запятых
        }
    }
}