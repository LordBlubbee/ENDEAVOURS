using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_Cinematic : MonoBehaviour
{
    [Header("REFERENCES")]
    public Image CinematicImage1;
    public Image CinematicImage2;
    public TextMeshProUGUI CinematicTex;
    public TextMeshProUGUI SkipTex;

    [Header("IMAGES")]
    public AudioClip Intro_OST;
    public Sprite[] Intro_Images;

    bool isRunningCinematic = false;
    public void PlayIntroCinematic()
    {
        UI.ui.SelectScreen(gameObject);
        if (!isRunningCinematic) StartCoroutine(PlayIntro());
    }
    IEnumerator PlayIntro()
    {
        isRunningCinematic = true;
        SetImage(null);
        SetImage(null);
        yield return new WaitForSeconds(1f);

        StartCoroutine(SkipTextRoutine());
        yield return new WaitForSeconds(1f);
        if (Intro_OST) AUDCO.aud.setOST(Intro_OST);
        yield return new WaitForSeconds(2f);
        SetText("Long ago, the <color=#00AAFF>ORDER OF THE STORM</color> had united humanity.");
        yield return new WaitForSeconds(5f);
        SetImage(Intro_Images[0]); //Peaceful Catali world
        yield return new WaitForSeconds(5f);
        SetText("In an era of peace, they invented the <color=#FF00FF>MIST</color>. A substance that would repair the planet's atmosphere...");
        yield return new WaitForSeconds(6f);
        SetText("But something was wrong.");
        yield return new WaitForSeconds(4f);
        SetText("");
        yield return new WaitForSeconds(1f); 
        SetImage(Intro_Images[1]); //Mist incident (23)
        yield return new WaitForSeconds(3f);
        SetText("The Mist consumed all life. It took every man, every plant, every animal...");
        yield return new WaitForSeconds(6f);
        SetText("It consumed the world.");
        yield return new WaitForSeconds(4f);
        SetText("99.5% of the world population died, and the purplish void sat at a height of three kilometers.");
        yield return new WaitForSeconds(7f);
        SetText("Destiny was taken from humanity. Only the mountains remained safe.");
        yield return new WaitForSeconds(6f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetImage(Intro_Images[2]); //Purple planet (50)
        yield return new WaitForSeconds(3f);
        SetText("One hundred and eighteen years later.");
        yield return new WaitForSeconds(5f);
        SetText("Humanity had survived.");
        yield return new WaitForSeconds(4f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetImage(Intro_Images[3]); //Logipedes
        yield return new WaitForSeconds(3f);
        SetText("They are gathered under the <color=#EEDD33>LOGIPEDES REPUBLIC</color>, obsessed bureaucrats who invented the Drifters...");
        yield return new WaitForSeconds(7f);
        SetText("...From their seats in CAPITOLIS.");
        yield return new WaitForSeconds(5f);
        SetText("Finally, remnants of the <color=#00AAFF>ORDER OF THE STORM</color> had convinced the <color=#EEDD33>THREE LEGISLATES</color> and the <color=#22DD22>NOMADEN COALITION</color>.");
        yield return new WaitForSeconds(7f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetImage(Intro_Images[4]); //Starlight Endeavour Fleet (86)
        yield return new WaitForSeconds(3f);
        SetText("They would launch the <color=#FF00FF>STARLIGHT ENDEAVOUR</color>. A top-secret mission...");
        yield return new WaitForSeconds(6f);
        SetText("...To delve under the <color=#FF00FF>MIST</color>, to find the source, and to destroy it.");
        yield return new WaitForSeconds(7f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetImage(Intro_Images[5]); //Fleet Underway
        yield return new WaitForSeconds(3f);
        SetText("The Expedition travelled for two months before they found a strange signal in the far south.");
        yield return new WaitForSeconds(7f);
        SetText("The fleet moved in, tracking it deep into mountain caverns...");
        yield return new WaitForSeconds(6f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetImage(Intro_Images[6]); //Seeker Ambush (119)
        yield return new WaitForSeconds(2f);
        SetText("But then, an ambush, blasts from the deep piercing through the armour of our proud Drifters!");
        yield return new WaitForSeconds(6f);
        SetText("The fleet fought back. With all they could... But they were no match for the mysterious force that had assailed them.");
        yield return new WaitForSeconds(8f);

        SetText("All they had found was a single coordinate. Far from here, with an unknown purpose...");
        yield return new WaitForSeconds(7f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetImage(Intro_Images[7]); //Nexus Points (144)
        yield return new WaitForSeconds(3f);
        SetText("The <color=#00FFAA>NEXUS POINT</color>.");
        yield return new WaitForSeconds(4f);
        SetText("One coordinate left behind on rocks, deep within the darkness, far to the north...");
        yield return new WaitForSeconds(6f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetImage(Intro_Images[8]); //Governments Failing
        yield return new WaitForSeconds(3f);
        SetText("A revolution has broken out. Evidence that the <color=#FF00FF>MIST</color> is rising has been confirmed.");
        yield return new WaitForSeconds(7f);
        SetText("The governments can no longer support a mission for the good of all humanity when their internal struggles overtake them.");
        yield return new WaitForSeconds(7f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetImage(Intro_Images[9]); //Map to Nexus Point (177)
        yield return new WaitForSeconds(3f);
        SetText("YOU are the only known survivor of the Expedition. Lead all that remains of us to the <color=#00FFAA>NEXUS POINT</color>...");
        yield return new WaitForSeconds(7f);
        SetText("Uncover the truth. Free Humanity from the <color=#FF00FF>MIST</color>. Before we are consumed, before we destroy ourselves.");
        yield return new WaitForSeconds(7f);
        SetText("Save us from this <color=#FF00FF>MISTWORLD</color>.");
        yield return new WaitForSeconds(3f); 
        SetImage(null);
        yield return new WaitForSeconds(3f);
        SetText("");
        yield return new WaitForSeconds(2f);
        //202

        //SetImage(Intro_Images[10]); //Title Screen

        //Ending
        isRunningCinematic = false;
        UI.ui.GoBackToPreviousScreen();
    }

    private void SetText(string tex)
    {
        StartCoroutine(SwitchTextRoutine(tex));
    }
    bool isSwitchingText = false;
    IEnumerator SwitchTextRoutine(string tex)
    {
       // while (isSwitchingText) yield return null;
        isSwitchingText = true;
        while (CinematicTex.color.a > 0)
        {
            CinematicTex.color = new Color(1, 1, 1, Mathf.Clamp01(CinematicTex.color.a - Time.deltaTime));
            yield return null;
        }
        CinematicTex.text = tex;
        if (tex.Length > 0)
        {
            while (CinematicTex.color.a < 1)
            {
                CinematicTex.color = new Color(1, 1, 1, Mathf.Clamp01(CinematicTex.color.a + Time.deltaTime * 1.4f));
                yield return null;
            }
        }
        isSwitchingText = false;
    }
    IEnumerator SkipTextRoutine()
    {
        while (SkipTex.color.a < 1)
        {
            SkipTex.color = new Color(1, 0, 0, Mathf.Clamp01(SkipTex.color.a - Time.deltaTime));
            yield return null;
        }
        yield return new WaitForSeconds(7f);
        while (SkipTex.color.a > 0)
        {
            SkipTex.color = new Color(1, 0, 0, Mathf.Clamp01(SkipTex.color.a + Time.deltaTime));
            yield return null;
        }
    }
    private void SetImage(Sprite spr)
    {
        CinematicImage2.sprite = CinematicImage1.sprite;
        CinematicImage2.color = CinematicImage1.color;
        if (spr == null)
        {
            CinematicImage1.sprite = null;
            CinematicImage1.color = new Color(0,0,0,0);
        } else
        {
            CinematicImage1.sprite = spr;
            CinematicImage1.color = new Color(1, 1, 1, 0);
        }
    }
    private void Update()
    {
        float curFade = CinematicImage1.color.a;
        CinematicImage1.color = new Color(CinematicImage1.color.r, CinematicImage1.color.g, CinematicImage1.color.b, Mathf.Clamp01(curFade + Time.deltaTime * 0.5f));
        CinematicImage2.color = new Color(CinematicImage2.color.r, CinematicImage2.color.g, CinematicImage2.color.b, Mathf.Clamp01(1f-curFade));

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            //AUDCO.aud.setOST(null);
            UI.ui.GoBackToPreviousScreen();
        }
    }
}
