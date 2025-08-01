using UnityEngine;

public class CatAnimationHelper : MonoBehaviour
{
    // ���� � ���������� ����� ����� ���������� ������ �� �������� SatietyUIController
    public SatietyUIController satietyController;

    // ��� ������� ����� �������� ��������
    public void CallDropTear(int side)
    {
        // � ��� ��� ����� �������� ������ ������� � �������� �������
        if (satietyController != null)
        {
            satietyController.DropTear(side);
        }
    }
}