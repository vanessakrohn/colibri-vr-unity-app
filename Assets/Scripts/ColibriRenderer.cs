using System.Collections;
using System.Collections.Generic;
using COLIBRIVR.Rendering;
using UnityEngine;

public class ColibriRenderer : MonoBehaviour
{
    private GameObject template;
    private GameObject current;
    public GameObject spinner;
    
    void Start()
    {
        template = GameObject.FindWithTag("RendererTemplate");
    }
    
    public void LaunchRendering(string url)
    {
        StartCoroutine(LaunchRenderingCoroutine(url));
    }

    private IEnumerator LaunchRenderingCoroutine(string url)
    {
        spinner.SetActive(true);

        // Deactivate previous rendering
        if (current != null)
            current.SetActive(false);
        current = Instantiate(template, gameObject.transform);
        var rendering = current.GetComponent<AsyncRendering>();
        yield return rendering.LaunchRenderingWithURL(url);
        
        spinner.SetActive(false);
    }
}
