using UnityEngine;
using UnityEngine.UI;

public class PulsingUI : MonoBehaviour
{
    [Header("Настройки Пульсации")]
    [Tooltip("Минимальный размер (обычно 1)")]
    public float minScale = 0.95f;

    [Tooltip("Максимальный размер (насколько увеличится)")]
    public float maxScale = 1.1f;

    [Tooltip("Скорость пульсации")]
    public float speed = 3.0f;

    private Button btn;
    private Vector3 baseScale;

    void Start()
    {
        btn = GetComponent<Button>();
        baseScale = transform.localScale; // Запоминаем исходный размер
    }

    void Update()
    {
        // Если кнопки нет или она выключена (идет таймер перезарядки) -> не пульсируем
        if (btn != null && !btn.interactable)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 10f);
            return;
        }

        // Математика пульсации (синусоида)
        // Mathf.Sin выдает значения от -1 до 1.
        // (Sin + 1) / 2 переводит это в диапазон от 0 до 1.
        float t = (Mathf.Sin(Time.unscaledTime * speed) + 1.0f) / 2.0f;

        // Плавный переход от min к max
        float scale = Mathf.Lerp(minScale, maxScale, t);

        // Применяем размер
        transform.localScale = baseScale * scale;
    }
}