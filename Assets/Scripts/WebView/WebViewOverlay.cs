using Gree.UnityWebView;
using System.Collections;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(JsMessageHandler))]
public class WebViewOverlay : MonoBehaviour
{
    [SerializeField] private WebViewObject webViewObject;
    private JsMessageHandler messageHandler;
    private bool isInitialized = false;

    private void Awake()
    {
        messageHandler = GetComponent<JsMessageHandler>();
        messageHandler.OnWin += HandleWin;
        messageHandler.OnClose += CloseWebView;
        messageHandler.OnShowInterstitial += ShowInterstitial;
        messageHandler.OnShowRewarded += ShowRewarded;
    }

    private void OnDestroy()
    {
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

    private void EnsureWebViewCreated()
    {
        if (isInitialized) return;

        webViewObject.Init(
            cb: (msg) => {
                Debug.Log($"Повідомлення від JS: {msg}");
                messageHandler.ProcessMessage(msg);
            },
            err: (msg) => Debug.LogError($"Помилка WebView: {msg}"),
            started: (msg) => Debug.Log($"Завантаження стартувало: {msg}"),
            hooked: (msg) => Debug.Log($"Hooked: {msg}")
        );

        isInitialized = true;
    }

    public void OpenGame(string gameFolder)
    {
        EnsureWebViewCreated();

        string gameUrl = GetGameUrl(gameFolder);

        webViewObject.SetMargins(0, 0, 0, 0);
        webViewObject.SetVisibility(true);
        webViewObject.LoadURL(gameUrl);

        StartCoroutine(ForceRelayoutAfterDelay());
    }

    private IEnumerator ForceRelayoutAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // дать системным барам скрыться

        // "Дёргаем" margins, чтобы форсировать реальный layout pass у Android WebView
        webViewObject.SetMargins(0, 0, 0, 1);
        yield return null;
        webViewObject.SetMargins(0, 0, 0, 0);
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
            webViewObject.LoadURL("about:blank");
        }
    }

    private void HandleWin(int points)
    {
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

    public void SendDataToWebPage(string jsCommand)
    {
        if (webViewObject != null)
        {
            webViewObject.EvaluateJS(jsCommand);
        }
    }
}