using UnityEngine;
using TMPro;

public class ClickManager : MonoBehaviour
{
    public int currentScore = 0;
    public TMP_Text scoreText;
    public GameObject floatingPrefab;
    public Canvas mainCanvas;
    public AuthManager authManager;
    public TMP_Text userText;

    void Start()
    {
        userText.text = authManager.Username;
    }

    public void OnClickButton()
    {
        currentScore++;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Puntaje: {currentScore}";
        }
    }

    public void OnSaveScoreButton()
    {
        authManager.PatchUsuario(currentScore);
    }

    public void OnCookieClicked()
{
    GameObject newFloating = Instantiate(floatingPrefab, mainCanvas.transform);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
        mainCanvas.GetComponent<RectTransform>(),
        Input.mousePosition,                     
        Camera.main,                             
        out Vector2 localPos                     
    );
    newFloating.GetComponent<RectTransform>().localPosition = localPos;
}
}
