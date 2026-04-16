using UnityEngine;

public class CatAnimationHelper : MonoBehaviour
{
    // Сюда в инспекторе нужно будет перетащить объект со скриптом SatietyUIController
    public SatietyUIController satietyController;

    // Эту функцию будет вызывать анимация
    public void CallDropTear(int side)
    {
        // А она уже будет вызывать нужную функцию в основном скрипте
        if (satietyController != null)
        {
            satietyController.DropTear(side);
        }
    }
}