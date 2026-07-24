using System;
using UnityEngine;

public class JsMessageHandler : MonoBehaviour
{
    // События, на которые могут подписаться другие системы
    public Action<int> OnWin;
    public Action OnClose;
    public Action OnShowInterstitial;
    public Action OnShowRewarded;

    public void ProcessMessage(string jsonMessage)
    {
        try
        {
            // Превращаем JSON-строку в C# объект
            JsPayload payload = JsonUtility.FromJson<JsPayload>(jsonMessage);

            switch (payload.action)
            {
                case "win":
                    OnWin?.Invoke(payload.points);
                    break;
                case "close":
                    OnClose?.Invoke();
                    break;
                case "showInterstitial":
                    OnShowInterstitial?.Invoke();
                    break;
                case "showRewarded":
                    OnShowRewarded?.Invoke();
                    break;
                default:
                    Debug.LogWarning($"JS Handler: Неизвестное действие -> {payload.action}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"JS Handler: Ошибка парсинга JSON: {e.Message}. Исходное сообщение: {jsonMessage}");
        }
    }
}