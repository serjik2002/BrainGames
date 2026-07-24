using UnityEngine;
using UnityEditor;
using System.IO;
using System; // Нужно для работы с датой

public class ScreenshotTool
{
    // Метод для получения или создания папки Screenshots
    private static string GetScreenshotFolder()
    {
        // Application.dataPath указывает на папку Assets. Поднимаемся на уровень выше.
        string folderPath = Path.Combine(Application.dataPath, "../Screenshots");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        return folderPath;
    }

    // Метод для генерации полного пути к файлу с датой и временем
    private static string GetFilePath(string prefix)
    {
        string folder = GetScreenshotFolder();
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return Path.Combine(folder, $"{prefix}_{timestamp}.png");
    }

    [MenuItem("Tools/Screenshot/Capture Game View")]
    public static void CaptureGameView()
    {
        string path = GetFilePath("Game");

        // Делаем скриншот
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Скриншот окна Game сохранен: " + path);

        // Открываем проводник и выделяем созданный файл
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Screenshot/Capture Scene View")]
    public static void CaptureSceneView()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;

        if (sceneView == null)
        {
            Debug.LogWarning("Окно Scene не найдено или не активно!");
            return;
        }

        string path = GetFilePath("Scene");

        // Подготовка к рендеру
        Camera cam = sceneView.camera;
        RenderTexture rt = new RenderTexture((int)sceneView.position.width, (int)sceneView.position.height, 24);

        // Рендерим сцену в текстуру
        cam.targetTexture = rt;
        cam.Render();

        // Считываем пиксели
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        // Очистка памяти
        cam.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(rt);

        // Сохранение в PNG
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        UnityEngine.Object.DestroyImmediate(tex);

        Debug.Log("Скриншот окна Scene сохранен: " + path);

        // Открываем проводник и выделяем созданный файл
        EditorUtility.RevealInFinder(path);
    }
}