using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizeText : MonoBehaviour
{
    [Header("Ключ локализации из JSON")]
    public string localizationKey;

    private void OnEnable()
    {
        Localize();
    }

    private void Start()
    {
        Localize(); // Двойной контроль: сработает при самом старте компонента
    }

    public void Localize()
    {
        if (string.IsNullOrEmpty(localizationKey)) return;

        TextMeshProUGUI textComponent = GetComponent<TextMeshProUGUI>();
        
        if (textComponent != null && LocalizationManager.Instance != null)
        {
            textComponent.text = LocalizationManager.Instance.GetTranslation(localizationKey);
        }
    }
}