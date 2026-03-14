using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_UnlockPopup : MonoBehaviour
{
    public Image Border;
    public Image Icon;
    public TextMeshProUGUI UnlockTex;
    public TextMeshProUGUI NameTex;
    private float AnimationTime = 0;
    private float Scale = 0;
    public void SetUnlockBackground(ScriptableBackground back)
    {
        gameObject.SetActive(true);
        NameTex.text = back.BackgroundName;
        NameTex.color = back.BackgroundColor;
        Border.color = back.BackgroundColor;
        UnlockTex.text = "NEW BACKGROUND";
        AnimationTime = 7f;
        Scale = 0;
    }
    public void SetUnlockDrifter(SpawnableShip ship)
    {
        gameObject.SetActive(true);
        NameTex.text = ship.Name;
        NameTex.color = ship.Color;
        Border.color = ship.Color;
        UnlockTex.text = "NEW DRIFTER";
        AnimationTime = 7f;
        Scale = 0;
    }
    private void Update()
    {
        AnimationTime -= Time.deltaTime;
        if (AnimationTime < 0)
        {
            Scale -= Time.deltaTime;
            if (Scale < 0) gameObject.SetActive(false);
        } else
        {
            if (Scale < 1f)
            {
                Scale += Time.deltaTime;
            }
        }
        transform.localScale = new Vector3(Scale, Scale, 1);
    }

    public void WhenPressed()
    {
        AnimationTime = 0f;
    }
}
