using TMPro;
using UnityEngine;

public class MachinePanel : MonoBehaviour
{
    public TMP_Text stateText;
    public TMP_Text axesText;
    public TMP_Text messageText;

    // Показывает координаты и статус
    public void ShowState(float x, float y, float z, bool isRunning)
    {
        if (stateText != null)
            stateText.text = isRunning ? "<color=green>Работает</color>" : "<color=red>Остановлен</color>";
        
        if (axesText != null)
            axesText.text = $"X: {x:F1} мм\nY: {y:F1} мм\nZ: {z:F1} мм";
    }

    // Меняет только текст статуса (для кнопок)
    public void SetState(bool isRunning)
    {
        if (stateText != null)
            stateText.text = isRunning ? "<color=green>Работает</color>" : "<color=red>Остановлен</color>";
    }

    // Показывает сообщения об ошибках или отправке
    public void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}