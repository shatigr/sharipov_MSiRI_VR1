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
    public string postUrl = "https://cnc.kovalev.team/post/3"; 
    public int channel = 1;
    public float interval = 0.5f;

    [Header("Узлы станка")]
    public Transform nodeX;
    public Transform nodeY;
    public Transform nodeZ;

    [Header("Панель интерфейса")]
    public MachinePanel panel; 

    private Vector3 startX, startY, startZ;
    private bool isMachineRunning = false; // Новая переменная для хранения статуса

    private IEnumerator Start()
    {
        if (nodeX == null || nodeY == null || nodeZ == null)
        {
            Debug.LogError("Назначьте три узла станка в Inspector.");
            yield break;
        }

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
                        if (panel != null) panel.ShowMessage("Ошибка JSON");
                    }
                }
                else
                {
                    Debug.LogWarning($"Ошибка запроса: {request.responseCode} {request.error}");
                    if (panel != null) panel.ShowMessage("Нет связи");
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

        nodeX.localPosition = startX + Vector3.right * (xMm * 0.001f);
        nodeY.localPosition = startY + Vector3.forward * (yMm * 0.001f);
        nodeZ.localPosition = startZ + Vector3.up * (zMm * 0.001f);

        if (panel != null)
        {
            // ТЕПЕРЬ ПЕРЕДАЕМ ПЕРЕМЕННУЮ isMachineRunning, А НЕ ЖЕСТКОЕ TRUE
            panel.ShowState(xMm, yMm, zMm, isMachineRunning); 
            panel.ShowMessage("Связь ОК");
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
                float value = float.Parse(text, CultureInfo.InvariantCulture);

                if (float.IsNaN(value) || float.IsInfinity(value))
                    throw new FormatException("Некорректная координата " + axis);

                return value;
            }
        }
        throw new FormatException("Не найдена координата " + axis);
    }

    // --- МЕТОДЫ ДЛЯ КНОПОК ---
    public void StartMachine()
    {
        isMachineRunning = true; // Запоминаем, что нажали старт
        if (panel != null) panel.SetState(true); 
        StartCoroutine(SendCommand("start"));
    }

    public void StopMachine()
    {
        isMachineRunning = false; // Запоминаем, что нажали стоп
        if (panel != null) panel.SetState(false); 
        StartCoroutine(SendCommand("stop"));
    }

    // ТОТ САМЫЙ ПОТЕРЯВШИЙСЯ МЕТОД ОТПРАВКИ POST-ЗАПРОСА
    private IEnumerator SendCommand(string command)
    {
        if (panel != null) panel.ShowMessage("Отправка команды...");
        
        string jsonPayload = "{\"command\":\"" + command + "\"}";
        
        using (UnityWebRequest request = new UnityWebRequest(postUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (panel != null) panel.ShowMessage("Ошибка: " + request.error);
                Debug.LogError("Ошибка POST: " + request.error);
            }
            else
            {
                if (panel != null) panel.ShowMessage("Команда принята");
                Debug.Log("Команда " + command + " успешно отправлена!");
            }
        }
    }
}