#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Инструмент для открытия папки с файлами опций из редактора Unity
/// </summary>
public class OptionsEditorTool : EditorWindow
{
    [MenuItem("Tools/Options/Open Options Folder")]
    private static void OpenOptionsFolder()
    {
        string optionsPath = Path.Combine(Application.persistentDataPath, "options.xml");
        string folderPath = Path.GetDirectoryName(optionsPath);

        if (Directory.Exists(folderPath))
        {
            // Открываем папку в проводнике (на Windows) или Finder (на Mac)
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", folderPath.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", folderPath);
#endif
        }
        else
        {
            EditorUtility.DisplayDialog("Ошибка", $"Папка не найдена: {folderPath}", "OK");
        }
    }

    [MenuItem("Tools/Options/Show Options File Path")]
    private static void ShowOptionsPath()
    {
        string optionsPath = Path.Combine(Application.persistentDataPath, "options.xml");
        EditorUtility.DisplayDialog("Путь к файлу опций",
            $"Файл опций: {optionsPath}", "OK");
    }

    [MenuItem("Tools/Options/Open Options File")]
    private static void OpenOptionsFile()
    {
        string optionsPath = Path.Combine(Application.persistentDataPath, "options.xml");

        if (File.Exists(optionsPath))
        {
            // Открываем файл в текстовом редакторе
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("notepad.exe", optionsPath);
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", "-a TextEdit " + optionsPath);
#endif
        }
        else
        {
            EditorUtility.DisplayDialog("Ошибка", $"Файл не найден: {optionsPath}", "OK");
        }
    }

    [MenuItem("Tools/Options/Clear Options File")]
    private static void ClearOptionsFile()
    {
        string optionsPath = Path.Combine(Application.persistentDataPath, "options.xml");

        if (File.Exists(optionsPath))
        {
            if (EditorUtility.DisplayDialog("Подтверждение",
                "Вы уверены, что хотите удалить файл опций?", "Удалить", "Отмена"))
            {
                File.Delete(optionsPath);
                EditorUtility.DisplayDialog("Успешно", "Файл опций удален. При следующем запуске будет создан новый.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Информация", "Файл опций не найден.", "OK");
        }
    }

    // Дополнительный пункт меню для просмотра содержимого файла
    [MenuItem("Tools/Options/View Options Content")]
    private static void ViewOptionsContent()
    {
        string optionsPath = Path.Combine(Application.persistentDataPath, "options.xml");

        if (File.Exists(optionsPath))
        {
            try
            {
                string content = File.ReadAllText(optionsPath);

                // Показываем окно с содержимым
                OptionsViewerWindow window = GetWindow<OptionsViewerWindow>("Options Viewer");
                window.SetContent(content);
                window.Show();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Ошибка", $"Не удалось прочитать файл: {e.Message}", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Информация", "Файл опций не найден.", "OK");
        }
    }
}

/// <summary>
/// Окно для просмотра содержимого файла опций
/// </summary>
public class OptionsViewerWindow : EditorWindow
{
    private string content = "";
    private Vector2 scrollPosition;

    public void SetContent(string content)
    {
        this.content = content;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Содержимое файла опций:", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        if (string.IsNullOrEmpty(content))
        {
            EditorGUILayout.HelpBox("Файл пуст или не найден", MessageType.Info);
        }
        else
        {
            // Отображаем содержимое в текстовом поле с возможностью редактирования
            content = EditorGUILayout.TextArea(content, GUILayout.ExpandHeight(true));
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Сохранить изменения"))
        {
            string path = Path.Combine(Application.persistentDataPath, "options.xml");
            try
            {
                File.WriteAllText(path, content);
                EditorUtility.DisplayDialog("Успешно", "Файл успешно сохранен.", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Ошибка", $"Не удалось сохранить файл: {e.Message}", "OK");
            }
        }

        if (GUILayout.Button("Закрыть"))
        {
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }
}

#endif