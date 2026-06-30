using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Handles downloading of card images from URLs and caching them locally to disk and memory.
/// </summary>
public class ImageCacheManager : MonoBehaviour
{
    private static ImageCacheManager instance;
    public static ImageCacheManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ImageCacheManager");
                instance = go.AddComponent<ImageCacheManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private Dictionary<string, Sprite> memoryCache = new Dictionary<string, Sprite>();
    private string cacheDirectory;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        cacheDirectory = Path.Combine(Application.persistentDataPath, "ImageCache");
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }
    }

    /// <summary>
    /// Loads an image from the URL. Checks memory and disk cache first before downloading.
    /// </summary>
    public void LoadImage(string url, System.Action<Sprite> callback)
    {
        if (string.IsNullOrEmpty(url))
        {
            callback?.Invoke(null);
            return;
        }

        // 1. Check memory cache
        if (memoryCache.TryGetValue(url, out Sprite cachedSprite))
        {
            callback?.Invoke(cachedSprite);
            return;
        }

        // 2. Check disk cache
        string hash = GetMD5Hash(url);
        string extension = Path.GetExtension(url);
        if (string.IsNullOrEmpty(extension) || extension.Contains("?"))
        {
            extension = ".png"; // Default to png
        }
        else if (extension.Contains("&"))
        {
            // Strip any query params
            int idx = extension.IndexOfAny(new char[] { '?', '&' });
            if (idx != -1) extension = extension.Substring(0, idx);
        }
        string localPath = Path.Combine(cacheDirectory, hash + extension);

        if (File.Exists(localPath))
        {
            StartCoroutine(LoadFromDiskCoroutine(localPath, url, callback));
        }
        else
        {
            StartCoroutine(DownloadImageCoroutine(url, localPath, callback));
        }
    }

    private IEnumerator LoadFromDiskCoroutine(string localPath, string url, System.Action<Sprite> callback)
    {
        string uri = "file://" + localPath;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    if (!memoryCache.ContainsKey(url))
                    {
                        memoryCache[url] = sprite;
                    }
                    callback?.Invoke(sprite);
                    yield break;
                }
            }
        }

        // Fallback: If loading from disk failed, attempt redownloading
        StartCoroutine(DownloadImageCoroutine(url, localPath, callback));
    }

    private IEnumerator DownloadImageCoroutine(string url, string localPath, System.Action<Sprite> callback)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    
                    if (!memoryCache.ContainsKey(url))
                    {
                        memoryCache[url] = sprite;
                    }

                    try
                    {
                        byte[] bytes = uwr.downloadHandler.data;
                        File.WriteAllBytes(localPath, bytes);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[ImageCacheManager] Error writing disk cache: {ex.Message}");
                    }

                    callback?.Invoke(sprite);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogWarning($"[ImageCacheManager] Download failed for {url}: {uwr.error}");
                callback?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// Preloads a list of card image URLs concurrently and reports progress.
    /// </summary>
    public void PreloadImages(List<PokemonCardData> cards, System.Action<float> onProgress, System.Action onComplete)
    {
        StartCoroutine(PreloadCoroutine(cards, onProgress, onComplete));
    }

    private IEnumerator PreloadCoroutine(List<PokemonCardData> cards, System.Action<float> onProgress, System.Action onComplete)
    {
        List<string> urlsToLoad = new List<string>();
        foreach (var card in cards)
        {
            if (card == null) continue;
            if (!string.IsNullOrEmpty(card.imageUrl) && !urlsToLoad.Contains(card.imageUrl))
            {
                urlsToLoad.Add(card.imageUrl);
            }
            if (!string.IsNullOrEmpty(card.customBackgroundUrl) && !urlsToLoad.Contains(card.customBackgroundUrl))
            {
                urlsToLoad.Add(card.customBackgroundUrl);
            }
        }

        if (urlsToLoad.Count == 0)
        {
            onProgress?.Invoke(1.0f);
            onComplete?.Invoke();
            yield break;
        }

        int completedCount = 0;
        int total = urlsToLoad.Count;

        foreach (var url in urlsToLoad)
        {
            LoadImage(url, (sprite) =>
            {
                completedCount++;
                float progress = (float)completedCount / total;
                onProgress?.Invoke(progress);
            });
        }

        // Wait until all downloads complete
        while (completedCount < total)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }

    private string GetMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
