using System;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;


/// <summary>
/// Статична система Options для data-driven роботи з параметрами через XML.
/// Використовує dot-notation для доступу до атрибутів: "section.subsection.attribute"
/// </summary>
public static class Options
{
    private static XmlDocument xmlDoc;
    private static string filePath;
    private static bool isDirty = false;
    private static bool isInitialized = false;

    private const string ROOT_NODE_NAME = "Options";
    private const string DEFAULT_FILENAME = "options.xml";

    // === НАСТРОЙКИ ШИФРОВАНИЯ ===
    public static bool UseEncryption = true; // Управляется из SaveManager

    // Ключи AES (Можешь изменить эти строки, главное чтобы Key был 32 символа, а IV - 16)
    private static readonly byte[] AesKey = Encoding.UTF8.GetBytes("Th1sIs4S3cr3tK3yF0r0pt1ons!!1234");
    private static readonly byte[] AesIV = Encoding.UTF8.GetBytes("1234567890123456");
    // ============================

    private static void Initialize()
    {
        if (isInitialized) return;

        filePath = Path.Combine(Application.persistentDataPath, DEFAULT_FILENAME); // [cite: 155]
        xmlDoc = new XmlDocument(); // [cite: 155]

        if (File.Exists(filePath)) // [cite: 156]
        {
            string fileContent = File.ReadAllText(filePath);
            bool isLoaded = false;
            bool wasEncrypted = false;

            // Спроба 1: Спочатку завжди пробуємо розшифрувати файл
            try
            {
                string decrypted = Decrypt(fileContent);
                xmlDoc.LoadXml(decrypted);
                isLoaded = true;
                wasEncrypted = true;
                Console.WriteLine($"[Options] Завантажено (Розшифровано) з {filePath}");
            }
            catch
            {
                // Помилка розшифрування — скоріш за все, файл просто збережений відкритим текстом
            }

            // Спроба 2: Якщо розшифрувати не вийшло, читаємо як звичайний XML
            if (!isLoaded)
            {
                try
                {
                    xmlDoc.LoadXml(fileContent);
                    isLoaded = true;
                    wasEncrypted = false;
                    Console.WriteLine($"[Options] Завантажено (Відкритий текст) з {filePath}");
                }
                catch (Exception e)
                {
                    // Файл дійсно пошкоджений або некоректний. Тільки тут створюємо новий.
                    Console.WriteLine($"[Options] Помилка завантаження XML: {e.Message}. Створюється новий файл."); // [cite: 157]
                    CreateNewDocument(); // [cite: 158]
                    isLoaded = true;
                }
            }

            // Якщо формат файлу не збігається з поточною настройкою галочки (наприклад, 
            // файл зашифрований, а галочку щойно зняли) — позначаємо його як "брудний", 
            // щоб при виході з гри він перезаписався у потрібному форматі.
            if (isLoaded && (wasEncrypted != UseEncryption))
            {
                isDirty = true;
            }
        }
        else
        {
            CreateNewDocument(); // [cite: 158]
        }

        isInitialized = true; // [cite: 159]
    }

    private static void CreateNewDocument()
    {
        xmlDoc = new XmlDocument();

        XmlDeclaration declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(declaration);

        XmlElement root = xmlDoc.CreateElement(ROOT_NODE_NAME);
        xmlDoc.AppendChild(root);

        isDirty = true;
    }

    private static XmlElement GetOrCreateNode(string path, bool createIfMissing)
    {
        if (!isInitialized) Initialize();

        string[] segments = path.Split('.');
        if (segments.Length < 2)
        {
            Console.WriteLine($"[Options] Некоректний шлях: {path}. Очікується формат 'node.attribute'");
            return null;
        }

        XmlElement current = xmlDoc.DocumentElement;

        for (int i = 0; i < segments.Length - 1; i++)
        {
            string segment = segments[i];
            XmlElement child = current[segment];

            if (child == null)
            {
                if (!createIfMissing) return null;

                child = xmlDoc.CreateElement(segment);
                current.AppendChild(child);
                isDirty = true;
            }

            current = child;
        }

        return current;
    }

    private static string GetAttributeName(string path)
    {
        string[] segments = path.Split('.');
        return segments[segments.Length - 1];
    }

    // ===== GET методи =====

    public static int GetInt(string key, int defaultValue = 0)
    {
        string value = GetString(key, null);
        return value != null && int.TryParse(value, out int result) ? result : defaultValue;
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        string value = GetString(key, null);
        return value != null && float.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        string value = GetString(key, null);
        if (value == null) return defaultValue;

        value = value.ToLower();
        if (value == "true" || value == "1") return true;
        if (value == "false" || value == "0") return false;

        return defaultValue;
    }

    public static string GetString(string key, string defaultValue = "")
    {
        XmlElement node = GetOrCreateNode(key, false);
        if (node == null) return defaultValue;

        string attrName = GetAttributeName(key);
        if (!node.HasAttribute(attrName)) return defaultValue;

        return node.GetAttribute(attrName);
    }

    // ===== SET методи =====

    public static void SetInt(string key, int value)
    {
        SetString(key, value.ToString());
    }

    public static void SetFloat(string key, float value)
    {
        SetString(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    public static void SetBool(string key, bool value)
    {
        SetString(key, value ? "true" : "false");
    }

    public static void SetString(string key, string value)
    {
        XmlElement node = GetOrCreateNode(key, true);
        if (node == null) return;

        string attrName = GetAttributeName(key);
        string currentValue = node.HasAttribute(attrName) ? node.GetAttribute(attrName) : null;

        if (currentValue != value)
        {
            node.SetAttribute(attrName, value);
            isDirty = true;
        }
    }

    // ===== Збереження та керування =====

    public static void Save()
    {
        if (!isInitialized) Initialize();
        if (!isDirty) return;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (UseEncryption)
            {
                // Шифруем и сохраняем как строку
                string encrypted = Encrypt(xmlDoc.OuterXml);
                File.WriteAllText(filePath, encrypted);
            }
            else
            {
                // Сохраняем как обычный XML
                xmlDoc.Save(filePath);
            }

            isDirty = false;
            Console.WriteLine($"[Options] Збережено у {filePath} (Шифрування: {UseEncryption})");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Options] Помилка збереження: {e.Message}");
        }
    }

    public static void Reload()
    {
        isInitialized = false;
        isDirty = false;
        Initialize();
    }

    public static bool HasUnsavedChanges()
    {
        return isDirty;
    }

    public static string GetFilePath()
    {
        if (!isInitialized) Initialize();
        return filePath;
    }

    public static void Delete(string key)
    {
        XmlElement node = GetOrCreateNode(key, false);
        if (node == null) return;

        string attrName = GetAttributeName(key);
        if (node.HasAttribute(attrName))
        {
            node.RemoveAttribute(attrName);
            isDirty = true;
        }
    }

    public static bool HasKey(string key)
    {
        XmlElement node = GetOrCreateNode(key, false);
        if (node == null) return false;

        string attrName = GetAttributeName(key);
        return node.HasAttribute(attrName);
    }

    // ===== АЛГОРИТМЫ ШИФРОВАНИЯ =====

    private static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = AesKey;
            aes.IV = AesIV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private static string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = AesKey;
            aes.IV = AesIV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
