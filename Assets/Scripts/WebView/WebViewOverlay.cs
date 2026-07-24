using Gree.UnityWebView;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(JsMessageHandler))]
public class WebViewOverlay : MonoBehaviour
{
    private WebViewObject webViewObject;
    private JsMessageHandler messageHandler;

    private void Awake()
    {
        // Получаем ссылку на наш новый обработчик
        messageHandler = GetComponent<JsMessageHandler>();

        // Подписываемся на события из JS
        messageHandler.OnWin += HandleWin;
        messageHandler.OnClose += CloseWebView;
        messageHandler.OnShowInterstitial += ShowInterstitial;
        messageHandler.OnShowRewarded += ShowRewarded;
    }

    private void OnDestroy()
    {
        // Отписываемся во избежание утечек памяти
        if (messageHandler != null)
        {
            messageHandler.OnWin -= HandleWin;
            messageHandler.OnClose -= CloseWebView;
            messageHandler.OnShowInterstitial -= ShowInterstitial;
            messageHandler.OnShowRewarded -= ShowRewarded;
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            CloseWebView();
        }
    }

    public void OpenGame(string gameFolder)
    {
        string gameUrl = GetGameUrl(gameFolder);

        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(
            cb: (msg) => {
                Debug.Log($"Повідомлення від JS: {msg}");
                // Передаем сырую строку в обработчик
                messageHandler.ProcessMessage(msg);
            },
            err: (msg) => Debug.LogError($"Помилка WebView: {msg}"),
            started: (msg) => Debug.Log($"Завантаження стартувало: {msg}"),
            hooked: (msg) => Debug.Log($"Hooked: {msg}")
        );

        webViewObject.SetMargins(0, 0, 0, 0);
        webViewObject.SetVisibility(true);
        webViewObject.LoadURL(gameUrl);
    }

    private string GetGameUrl(string gameFolder)
    {
#if UNITY_EDITOR
        return Path.Combine(Application.streamingAssetsPath, "games", gameFolder, "index.html");
#elif UNITY_ANDROID
        return "file:///android_asset/games/" + gameFolder + "/index.html";
#elif UNITY_IOS
        return "file://" + Application.streamingAssetsPath + "/games/" + gameFolder + "/index.html";
#else
        return "";
#endif
    }

    public void CloseWebView()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
            Destroy(webViewObject.gameObject);
        }
    }

    // --- Блок бизнес-логики (реакции на события) ---

    private void HandleWin(int points)
    {
        int currentStars = PlayerPrefs.GetInt("StarsBalance", 0);
        PlayerPrefs.SetInt("StarsBalance", currentStars + points);
        PlayerPrefs.Save();

        Debug.Log($"Гравцю зараховано {points} зірочок!");
        CloseWebView();
    }

    private void ShowInterstitial()
    {
        AdManager.Instance.ShowInterstitialAd();
    }

    private void ShowRewarded()
    {
        AdManager.Instance.ShowRewardedAd((reward) =>
        {
            print("Rewarded callback");
        });
    }

    // Метод для отправки команд обратно в веб-часть
    public void SendDataToWebPage(string jsCommand)
    {
        if (webViewObject != null)
        {
            webViewObject.EvaluateJS(jsCommand);
        }
    }
}