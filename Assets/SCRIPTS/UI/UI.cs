using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI ui;
    public GameObject CurrentlySelectedScreen;
    public Screen_CharacterCreator CharacterCreationUI;
    public GameObject LoadingStartGameUI;
    public Screen_GameUI MainGameplayUI;
    public Screen_ShipSelector ShipSelectionUI;
    public Screen_Inventory InventoryUI;
    public Screen_Map MapUI;
    public Screen_Talk TalkUI;
    public Screen_Reward RewardUI;
    public Screen_Settings SettingsUI;
    public Screen_Cinematic CinematicUI;
    public Screen_Host HostUI;
    public TutorialManager TutorialManager;
    public Image Crosshair;
    public ChatModule ChatUI;
    private GameObject PreviousScreen;

    public TextMeshProUGUI CinematicTex;
    public Image BlackScreen;
    public Image WhiteScreen;

    public Image LossScreenImage;
    public GameObject LossScreenContents;
    public TextMeshProUGUI LossScreenTex;
    public CanvasGroup LossScreen;

    public TextMeshProUGUI SaveTex;

    public void OpenLossScreen()
    {
        if (LossScreen.gameObject.activeSelf) return;
        StartCoroutine(OpenLossScreenNum());
    }

    IEnumerator OpenLossScreenNum()
    {
        LossScreen.gameObject.SetActive(true);
        LossScreenImage.color = Color.white;
        LossScreen.alpha = 0;
        while (LossScreen.alpha < 1)
        {
            LossScreen.alpha += Time.deltaTime * 0.35f;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        LossScreenImage.color = Color.black;
        LossScreenContents.gameObject.SetActive(true);
        LossScreenTex.alpha = 0;
        while (LossScreenTex.alpha < 1)
        {
            LossScreenTex.alpha += Time.deltaTime * 0.5f;
            yield return null;
        }

    }

    bool SaveTexShowing = false;
    public void DisplaySaveTex()
    {
        if (SaveTexShowing) return;
        StartCoroutine(SaveTexNum());
    }

    IEnumerator SaveTexNum()
    {
        SaveTexShowing = true;
        SaveTex.alpha = 0;
        while (SaveTex.alpha < 1)
        {
            SaveTex.alpha += Time.deltaTime * 1.4f;
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        while (SaveTex.alpha > 0)
        {
            SaveTex.alpha -= Time.deltaTime * 1.4f;
            yield return null;
        }
        SaveTexShowing = false;
    }
    private void Start()
    {
        SettingsUI.Init();

        SelectScreen(CurrentlySelectedScreen);
        CinematicUI.PlayIntroCinematic();
    }
    public void OpenTalkScreenFancy(GameObject ob)
    {
        ob.transform.localScale = Vector3.zero;
        SelectScreen(ob);
        StartCoroutine(OpeningTalkUIFancy(ob));
    }

    IEnumerator OpeningTalkUIFancy(GameObject ob)
    {
        float Scale = 0f;
        while (Scale < 1f)
        {
            Scale += Time.deltaTime * 4.5f * (0.1f + (1f-Scale));
            ob.transform.localScale = new Vector3(Scale, Scale, 1);
            yield return null;
        }
        ob.transform.localScale = new Vector3(1, 1, 1);
    }
    public void PressSelectScreen(GameObject ob)
    {
        AUDCO.aud.PlaySFX(AUDCO.aud.Press);
        SelectScreen(ob);
    }
    public void SelectScreen(GameObject ob)
    {
        PreviousScreen = CurrentlySelectedScreen;
        if (CurrentlySelectedScreen) CurrentlySelectedScreen.SetActive(false);
        CurrentlySelectedScreen = ob;
        if (ob) ob.SetActive(true);
    }

    public bool testturnoff;
    public void SetCrosshair(Vector3 pos)
    {
        if (testturnoff) return;
        Crosshair.transform.position = new Vector3(pos.x, pos.y);
    }
    public Sprite CrosshairSpriteGeneric;
    public void SetCrosshairTexture(Sprite img)
    {
        Crosshair.sprite = img == null ? CrosshairSpriteGeneric : img;
    }

    public void SetCrosshairColor(Color col)
    {
        Crosshair.color = col;
    }
    public void SetCrosshairRotateTowards(Vector3 towards, Vector3 from)
    {
        Crosshair.transform.Rotate(Vector3.forward, AngleBetweenPoints(from, towards));
    }
    public void SetCrosshairRotateReset()
    {
        Crosshair.transform.rotation = Quaternion.identity;
    }
    protected float AngleBetweenPoints(Vector3 from, Vector3 towards)
    {
        return Vector2.SignedAngle(getLookVector(), towards - from);
    }
    protected Vector3 getLookVector()
    {
        Quaternion rotref = Crosshair.transform.rotation;
        float rot = Mathf.Deg2Rad * rotref.eulerAngles.z;
        float dxf = Mathf.Cos(rot);
        float dyf = Mathf.Sin(rot);
        return new Vector3(dxf, dyf, 0);
    }

    public void GoBackToPreviousScreen()
    {
        PressSelectScreen(PreviousScreen);
    }
    public void GoBackToPreviousScreenNoPress()
    {
        SelectScreen(PreviousScreen);
    }
    private void Awake()
    {
        ui = this;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private bool isFlashingWhite;
    private bool isFadingBlack;
    private bool shouldBlackBeActive;
    private float BlackFadeSpeed;
    float CinematicTexScale = 1f;
    public void SetCinematicTex(string tex, Color col, float dur, float delay)
    {
        CinematicTexScale = 1.2f;
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
            CinematicTexScale = Mathf.Max((CinematicTexScale-0.8f) * Time.deltaTime * 3f,1);
            CinematicTex.transform.localScale = new Vector3(CinematicTexScale, CinematicTexScale, 1);
            CinematicTex.color = new Color(CinematicTex.color.r, CinematicTex.color.g, CinematicTex.color.b, CinematicTex.color.a + Time.deltaTime * 1f);
            yield return null;
        }
        yield return new WaitForSeconds(dur);
        while (CinematicTex.color.a > 0)
        {
            CinematicTexScale = Mathf.Max((CinematicTexScale - 0.8f) * Time.deltaTime * 4f, 1);
            CinematicTex.transform.localScale = new Vector3(CinematicTexScale, CinematicTexScale, 1);
            CinematicTex.color = new Color(CinematicTex.color.r, CinematicTex.color.g, CinematicTex.color.b, CinematicTex.color.a - Time.deltaTime * 1.2f);
            yield return null;
        }
        CinematicTex.color = Color.clear;
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
