using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI ui;
    public GameObject CurrentlySelectedScreen;
    public GameObject CharacterCreationUI;
    public Screen_GameUI MainGameplayUI;
    public Screen_ShipSelector ShipSelectionUI;
    public Screen_Inventory InventoryUI;
    public Screen_Map MapUI;
    public Screen_Talk TalkUI;
    public Image Crosshair;
    private GameObject PreviousScreen;


    public TextMeshProUGUI CinematicTex;
    public Image BlackScreen;
    public Image WhiteScreen;

    public void SelectScreen(GameObject ob)
    {
        PreviousScreen = CurrentlySelectedScreen;
        if (CurrentlySelectedScreen) CurrentlySelectedScreen.SetActive(false);
        CurrentlySelectedScreen = ob;
        if (ob) ob.SetActive(true);
    }

    public enum CrosshairModes
    {
        NONE,
        GRAPPLE,
        GRAPPLE_SUCCESS,
        RANGED,
        WEAPONS
    }
    public void SetCrosshair(Vector3 pos, CrosshairModes mod)
    {
        Crosshair.transform.position = new Vector3(pos.x, pos.y);
        switch (mod)
        {
            case CrosshairModes.NONE:
                Crosshair.color = new Color(1, 1, 1, 0);
                break;
            case CrosshairModes.WEAPONS:
                Crosshair.color = new Color(1, 1, 1, 0);
                break;
            case CrosshairModes.GRAPPLE:
                Crosshair.color = new Color(1, 0, 0, 0.5f);
                break;
            case CrosshairModes.GRAPPLE_SUCCESS:
                Crosshair.color = new Color(0, 1, 0, 0.5f);
                break;
        }
    }
    [Header("CrosshairSprites")]
    public Sprite CrosshairSpriteGeneric;
    public Sprite CrosshairSpriteGrapple;
    public Sprite CrosshairSpriteBallista;
    public void SetCrosshairTexture(Sprite img)
    {
        Crosshair.sprite = img;
    }
    public void GoBackToPreviousScreen()
    {
        SelectScreen(PreviousScreen);
    }

    private void Awake()
    {
        ui = this;
        SelectScreen(CurrentlySelectedScreen);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private bool isFlashingWhite;
    private bool isFadingBlack;
    private bool shouldBlackBeActive;
    private float BlackFadeSpeed;
    public void SetCinematicTex(string tex, Color col, float dur, float delay)
    {
        StartCoroutine(RunCinematicTex(tex, col, dur, delay));
    }
    IEnumerator RunCinematicTex(string tex, Color col, float dur, float delay)
    {
        yield return new WaitForSeconds(delay);
        CinematicTex.gameObject.SetActive(true);
        CinematicTex.text = tex;
        CinematicTex.color = new Color(col.r, col.g, col.b, 0);
        while (CinematicTex.color.a < 1)
        {
            CinematicTex.color = new Color(CinematicTex.color.r, CinematicTex.color.g, CinematicTex.color.b, CinematicTex.color.a + Time.deltaTime * 1f);
            yield return null;
        }
        yield return new WaitForSeconds(dur);
        while (CinematicTex.color.a > 0)
        {
            CinematicTex.color = new Color(CinematicTex.color.r, CinematicTex.color.g, CinematicTex.color.b, CinematicTex.color.a - Time.deltaTime * 1f);
            yield return null;
        }
        CinematicTex.gameObject.SetActive(false);
    }
    public void FlashWhite(float dur)
    {
        if (isFlashingWhite) return;
        StartCoroutine(FlashWhiteNum(dur));
    }

    IEnumerator FlashWhiteNum(float dur)
    {
        isFlashingWhite = true;
        WhiteScreen.gameObject.SetActive(true);
        WhiteScreen.color = Color.white;
        while (WhiteScreen.color.a > 0)
        {
            WhiteScreen.color = new Color(1, 1, 1, WhiteScreen.color.a - (1f / dur) * Time.deltaTime);
            yield return null;
        }

        isFlashingWhite = false;
        WhiteScreen.gameObject.SetActive(false);
    }
    public void FadeToBlack(float dur, float delay)
    {

        shouldBlackBeActive = true;
        BlackFadeSpeed = dur;
        if (!isFadingBlack) StartCoroutine(ProcessFadeToBlack(delay));
    }
    public void FadeFromBlack(float dur)
    {
        shouldBlackBeActive = false;
        BlackFadeSpeed = dur;
        if (!isFadingBlack) StartCoroutine(ProcessFadeToBlack(0));
    }

    IEnumerator ProcessFadeToBlack(float delay)
    {
        isFadingBlack = true;
        yield return new WaitForSeconds(delay);
        BlackScreen.gameObject.SetActive(true);
        while (BlackScreen.color.a > 0 || shouldBlackBeActive)
        {
            if (shouldBlackBeActive)
                BlackScreen.color = new Color(0, 0, 0, BlackScreen.color.a + (1f / BlackFadeSpeed) * Time.deltaTime);
            else
                BlackScreen.color = new Color(0, 0, 0, BlackScreen.color.a - (1f / BlackFadeSpeed) * Time.deltaTime);
            yield return null;
        }

        isFadingBlack = false;
        BlackScreen.gameObject.SetActive(false);
    }
}
