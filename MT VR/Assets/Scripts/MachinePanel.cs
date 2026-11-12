using TMPro;
using UnityEngine;

public class MachinePanel : MonoBehaviour
{
    public TMP_Text stateText;
    public TMP_Text axesText;
    public TMP_Text messageText;

    public void ShowState(float x, float y, float z, bool isRunning)
    {
        stateText.text = isRunning ? "<color=green>Работает</color>" : "<color=red>Остановлен</color>";
        axesText.text = $"X: {x:F1} мм\nY: {y:F1} мм\nZ: {z:F1} мм";
    }
    // Этот метод будет менять только текст состояния
    public void SetState(bool isRunning)
    {
        if (stateText != null)
        {
            stateText.text = isRunning ? "<color=green>Работает</color>" : "<color=red>Остановлен</color>";
        }
    }
    public void ShowMessage(string message)
    {
        messageText.text = message;
    }
}