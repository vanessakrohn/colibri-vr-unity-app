using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace COLIBRIVR
{
    public class ImageClickHandler : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject renderer;
        private ColibriRenderer colibriRenderer;

        public void Start()
        {
            colibriRenderer = renderer.GetComponent<ColibriRenderer>();
        }

        public void OnImageClick(string renderingURL)
        {
            renderer.SetActive(true);
            colibriRenderer.LaunchRendering(renderingURL);
            canvas.SetActive(false);
        }
    }
}
