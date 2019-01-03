using UnityEngine;

public class Exciter : MonoBehaviour 
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        rectTransform.localScale = Vector3.zero;
    }

    public void Stop()
    {
        gameObject.SetActive(false);
    }

}
