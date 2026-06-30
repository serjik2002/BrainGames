using Gree.UnityWebView;
using System.IO;
using UnityEngine;

public class WebViewOverlay : MonoBehaviour
{
    private WebViewObject webViewObject;
    private string gameUrl;

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            CloseWebView();
        }
    }

    // Метод, який ти будеш викликати при натисканні на картку гри в Unity-меню
    public void OpenGame(string gameFolder)
    {
        // 1. Формуємо шлях до локального файлу index.html залежно від платформи
#if UNITY_EDITOR
        gameUrl = Path.Combine(Application.streamingAssetsPath, "games", gameFolder, "index.html");
#elif UNITY_ANDROID
        gameUrl = "file:///android_asset/games/" + gameFolder + "/index.html"; // [cite: 1255]
#elif UNITY_IOS
        gameUrl = "file://" + Application.streamingAssetsPath + "/games/" + gameFolder + "/index.html"; // [cite: 1255]
#endif

        // 2. Ініціалізуємо WebView
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(
            cb: (msg) => {
                Debug.Log($"Повідомлення від JS: {msg}");
                HandleJsMessage(msg);
            },
            err: (msg) => Debug.LogError($"Помилка WebView: {msg}"),
            started: (msg) => Debug.Log($"Завантаження стартувало: {msg}"),
            hooked: (msg) => Debug.Log($"Hooked: {msg}")
        );

        // 3. Налаштовуємо повноекранний режим із відступами (в пікселях)
        // Якщо у тебе є SafeArea або TopPanel, тут можна зробити відступи
        webViewObject.SetMargins(0, 0, 0, 0);

        // 4. Вмикаємо видимість та завантажуємо сторінку
        webViewObject.SetVisibility(true);
        webViewObject.LoadURL(gameUrl);
    }

    // Обробка даних, які прилітають з JavaScript (наш міст зв'язку)
    private void HandleJsMessage(string message)
    {
        if (message.StartsWith("win:")) // Наприклад, гра надіслала "win:150" [cite: 1282]
        {
            string scoreStr = message.Replace("win:", "");
            int starsEarned = int.Parse(scoreStr);

            // Нараховуємо зірочки в загальний баланс гравця
            int currentStars = PlayerPrefs.GetInt("StarsBalance", 0);
            PlayerPrefs.SetInt("StarsBalance", currentStars + starsEarned);
            PlayerPrefs.Save();

            Debug.Log($"Гравцю зараховано {starsEarned} зірочок!"); // [cite: 1282]
            CloseWebView();
        }
        else if (message == "close") // Якщо гравець натиснув кнопку "Х" в самій HTML-грі [cite: 1283]
        {
            CloseWebView();
        }
    }

    public void CloseWebView()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
            Destroy(webViewObject.gameObject);
        }
    }
}