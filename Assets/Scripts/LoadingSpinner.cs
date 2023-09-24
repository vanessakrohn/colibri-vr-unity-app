using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    private GameObject _spinner;

    private void Start()
    {
        _spinner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _spinner.transform.parent = gameObject.transform;
        _spinner.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    private void Update()
    {
        _spinner.transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * 50);
    }

    private void OnDestroy()
    {
        Destroy(_spinner);
    }
}