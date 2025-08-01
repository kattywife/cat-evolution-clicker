using UnityEngine;
using TMPro; // Обязательно добавьте эту строку для работы с TextMeshPro

public class FloatingText : MonoBehaviour
{
    [Tooltip("Как быстро текст будет падать (или подниматься, если значение отрицательное)")]
    public float moveSpeed = 150f;

    [Tooltip("Как быстро текст будет исчезать")]
    public float fadeSpeed = 1f;

    [Tooltip("Сколько секунд текст проживет перед уничтожением")]
    public float lifeTime = 1f;

    // Ссылка на компонент текста, чтобы менять его содержимое и цвет
    private TextMeshProUGUI textMesh;
    private Color startColor;

    void Awake()
    {
        // Находим компонент текста и запоминаем его исходный цвет
        textMesh = GetComponent<TextMeshProUGUI>();
        startColor = textMesh.color;
    }

    void Start()
    {
        // Сразу после создания запускаем таймер на самоуничтожение
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. Движение текста вниз
        // Двигаем объект вниз со скоростью moveSpeed пикселей в секунду
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        // 2. Плавное исчезновение
        // Уменьшаем альфа-канал (прозрачность) цвета со временем
        startColor.a -= fadeSpeed * Time.deltaTime;

        // Применяем новый, более прозрачный цвет к тексту
        textMesh.color = startColor;
    }
}