using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AI : MonoBehaviour
{
    public string pathToPyCharmFolder = @"C:\Users\user2\PyCharmMiscProject\";
    public Camera targetCamera; 
    public void CaptureFrame()
    {
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame(); // Ждём конца кадра, чтобы всё было нарисовано

        // Настраиваем рендер в текстуру
        RenderTexture rt = new RenderTexture(256, 256, 24); // Размер можно любой
        targetCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        targetCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        screenShot.Apply();

        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Ужимаем картинку
        Texture2D resized = ResizeTexture(screenShot, 244, 244); // Ужимаем до 244x244

        // Сохраняем
        byte[] bytes = resized.EncodeToJPG(75); // 75% качество
        string filename = @"C:\Users\user2\PyCharmMiscProject\img.jpg";
        File.WriteAllBytes(filename, bytes);

        UnityEngine.Debug.Log($"Скриншот сохранён: {filename}");
        StartCoroutine(RunPythonCoroutine());
    }
    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    private IEnumerator RunPythonCoroutine()
    {     
        // Путь до интерпретатора python.exe
        string pythonPath =@"C:/Users/user2/PyCharmMiscProject/.venv/Scripts/python.exe";

        // Путь до скрипта .py
        string scriptPath =@"C:\Users\user2\PyCharmMiscProject\show.py";

        // Готовим процесс запуска
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonPath;
        start.Arguments = $"\"{scriptPath}\""; // Скрипт в кавычках, чтобы работало даже с пробелами в пути
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true; // Без открытия черного окна консоли

        // Запускаем процесс
        using (Process process = Process.Start(start))
        {
            while (!process.HasExited)
            {
                yield return null; // Просто пропускаем кадр и проверяем снова на следующем кадре
            }

            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            UnityEngine.Debug.Log("Python Output: " + output);
            if (!string.IsNullOrEmpty(errors))
            {
                UnityEngine.Debug.LogError("Python Errors: " + errors);
            }

            LoadProcessedImage();
        }
    }

    private void LoadProcessedImage()
    {
        string processedImagePath = Path.Combine(pathToPyCharmFolder, "result.jpg"); // допустим скрипт сохраняет результат сюда
        byte[] fileData = File.ReadAllBytes(processedImagePath);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);

        // Теперь ты можешь отобразить эту текстуру в UI, например
        // Передай её в компонент Image, если нужно

        // Пример:
        GameObject.Find("ResultImage").GetComponent<UnityEngine.UI.RawImage>().texture = tex;
    }
}