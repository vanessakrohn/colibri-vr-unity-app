using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace COLIBRIVR
{
    [System.Serializable]
    public class ImageFolder
    {
        public string[] imageUrls;
    }

    [System.Serializable]
    public class RenderingFolders
    {
        public string[] renderings;
    }

    public class GridController : MonoBehaviour
    {
        public string renderingFoldersJsonUrl;
        public GridLayoutGroup gridLayoutGroup;
        public GameObject imagePrefab;
        public ImageClickHandler imageClickHandler;
        public string renderingURL;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(LoadRenderingFolders());
        }

        private IEnumerator LoadRenderingFolders()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(renderingFoldersJsonUrl))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download rendering folder JSON: " + www.error);
                    yield break;
                }

                RenderingFolders renderingFolders = JsonUtility.FromJson<RenderingFolders>(www.downloadHandler.text);

                foreach (string renderingFolder in renderingFolders.renderings)
                {
                    yield return StartCoroutine(LoadImageFolder(renderingFolder));
                }
            }
        }

        private IEnumerator LoadImageFolder(string folderUrl)
        {
            yield return StartCoroutine(LoadImage("thumbnail.jpg", folderUrl));
        }

        private IEnumerator LoadImage(string imageUrl, string folderUrl)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(folderUrl + imageUrl))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download image: " + www.error);
                    yield break;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));

                GameObject newImage = Instantiate(imagePrefab, gridLayoutGroup.transform);
                newImage.GetComponent<Image>().sprite = sprite;

                // Set the OnClick event for the image's button component
                Button imageButton = newImage.GetComponent<Button>();
                if (imageButton != null)
                {
                    imageButton.onClick.AddListener(() =>
                    {
                        renderingURL = folderUrl;
                        imageClickHandler.OnImageClick(renderingURL);
                        
                    });
                }
            }
        }

    }
}