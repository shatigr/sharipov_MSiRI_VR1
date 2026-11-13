using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CncJsonController : MonoBehaviour
{
    [Header("Настройки API")]
    public string url = "https://cnc.kovalev.team/get/3";
    public int channel = 1;
    public float interval = 0.5f;

    [Header("Узлы станка")]
    public Transform nodeX;
    public Transform nodeY;
    public Transform nodeZ;

    [Header("Панель интерфейса")]
    public MachinePanel panel; 

    private Vector3 startX, startY, startZ;
    
    // ВОТ НАШ ВЫКЛЮЧАТЕЛЬ: пока false - станок не двигается
    private bool isMachineRunning = false; 

    private IEnumerator Start()
    {
        if (nodeX == null || nodeY == null || nodeZ == null)
        {
            Debug.LogError("Назначьте три узла станка в Inspector.");
            yield break;
        }

        // Запоминаем, где станок стоял изначально
        startX = nodeX.localPosition;
        startY = nodeY.localPosition;
        startZ = nodeZ.localPosition;

        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        MoveNodes(request.downloadHandler.text);
                    }
                    catch (Exception error)
                    {
                        Debug.LogWarning("Ошибка чтения JSON: " + error.Message);
                    }
                }
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private void MoveNodes(string json)
    {
        JObject data = JObject.Parse(json);

        float xMm = GetCoordinate(data, "X");
        float yMm = GetCoordinate(data, "Y");
        float zMm = GetCoordinate(data, "Z");

        // 1. ВСЕГДА обновляем текст на панели (координаты и статус)
        if (panel != null)
        {
            panel.ShowState(xMm, yMm, zMm, isMachineRunning); 
        }

        // 2. А ДВИГАЕМ СТАНОК ТОЛЬКО ЕСЛИ НАЖАТА КНОПКА ПУСК
        if (isMachineRunning)
        {
            nodeX.localPosition = startX + Vector3.right * (xMm * 0.001f);
            nodeY.localPosition = startY + Vector3.forward * (yMm * 0.001f);
            nodeZ.localPosition = startZ + Vector3.up * (zMm * 0.001f);
        }
    }

    private float GetCoordinate(JObject data, string axis)
    {
        JToken sections = data["data"];
        if (sections == null) throw new FormatException("В JSON нет раздела data");

        string channelName = "Оси Канала " + channel;
        string axisName = "Перемещение по оси " + axis;

        foreach (JToken section in sections)
        {
            if (section == null || section.Type != JTokenType.Array) continue;
            if (!string.Equals((string)section[0], channelName, StringComparison.OrdinalIgnoreCase)) continue;

            foreach (JToken row in section[1])
            {
                if (row == null || row.Type != JTokenType.Array) continue;
                if (!string.Equals((string)row[0], axisName, StringComparison.OrdinalIgnoreCase)) continue;

                string text = ((string)row[1]).Replace("мм", "").Trim().Replace(',', '.');
                return float.Parse(text, CultureInfo.InvariantCulture);
            }
        }
        throw new FormatException("Не найдена координата " + axis);
    }

    // --- МЕТОДЫ ДЛЯ КНОПОК ---
    public void StartMachine()
    {
        isMachineRunning = true; // Включаем движение
        if (panel != null) panel.SetState(true); // Пишем на панели "Работает"
    }

    public void StopMachine()
    {
        isMachineRunning = false; // Выключаем движение
        if (panel != null) panel.SetState(false); // Пишем на панели "Остановлен"
    }
}