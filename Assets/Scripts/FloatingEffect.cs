using UnityEngine;
using TMPro;

public class FloatingEffect : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float fadeSpeed = 2f;
    private float alpha = 1f;

    public TextMeshProUGUI tmpText;

    void Start()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        alpha -= fadeSpeed * Time.deltaTime;
        if (tmpText != null)
        {
            tmpText.color = new Color(tmpText.color.r, tmpText.color.g, tmpText.color.b, alpha);
        }
    }
}