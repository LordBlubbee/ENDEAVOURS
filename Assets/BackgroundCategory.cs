using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BackgroundCategory : MonoBehaviour
{
    public List<ScriptableBackground> BackgroundsInside = new();
    public GameObject OpenSubscreen;
    public Image Border;
    public Image Sprite;
    private bool isEnabled = false;
    private void OnEnable()
    {
        if (BackgroundsInside.Count == 0)
        {
            isEnabled = true;
            Border.color = Color.white;
            Sprite.color = Color.white;
            return;
        }
        foreach (ScriptableBackground str in BackgroundsInside)
        {
            if (GO.g.UnlockedBackgrounds.Contains(str.name))
            {
                isEnabled = true;
                Border.color = Color.white;
                Sprite.color = Color.white;
                return;
            }
        }
        isEnabled = false;
        Border.color = Color.black;
        Sprite.color = new Color(0.2f,0.2f,0.2f);
    }
    public void WhenPressed()
    {
        if (!isEnabled)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        UI.ui.CharacterCreationUI.OpenSubscreen(OpenSubscreen);
    }
}
