using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class Screen_Cinematic : MonoBehaviour
{
    [Header("REFERENCES")]
    public CinematicPlayer Cinematic;
    public TextMeshProUGUI TalkTex;
    public TextMeshProUGUI TitleTex;
    public TextMeshProUGUI SubtitleTex;
    public TextMeshProUGUI SkipTex;

    [Header("IMAGES")]
    public AudioClip Intro_OST;
    public AudioClip[] Voices;
    public ScriptableScene[] Scenes;

    bool isRunningCinematic = false;
    bool canSkip = false;
    public void PlayIntroCinematic()
    {
        UI.ui.SelectScreen(gameObject);
        if (!isRunningCinematic) StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        canSkip = false;
        isRunningCinematic = true;

        //SetSceneInstant
        SetScene(null, true);

        yield return new WaitForSeconds(1f);

        StartCoroutine(SkipTextRoutine());
        canSkip = true;
        if (Intro_OST) AUDCO.aud.setOST(Intro_OST);
        yield return new WaitForSeconds(2f);


        SetScene(Scenes[1], false); //Capitolis
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[2], false); //Cliffs
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[0], false); //Driftyards
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[3], false); //Driftyards 2
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[4], false); //Underway

        yield return new WaitForSeconds(4f);
        SetTitle("STARLIGHT ENDEAVOUR EXPEDITION", "121 AD NEBULA - SOUTHERN REACHES");
        yield return new WaitForSeconds(6f);

        /*
         FULL PLANNED DIALOGUE
        [30 seconds of Mistworld scenes]
        ROLL TITLE SCREEN - ENDEAVOUR FLEET.
        
        In response to the disastrous Fluctus Nebulae, the Republic has finally authorized the launch of a unified expedition force.
        The mission of the fleet is to trace the origin of the rising Nebula, and investigate it using new sub-nebulan diving technology. 
        Our Gubernators are reporting unusual Flux signatures below.
        Sub-nebulan drives engaged. Monitoring buoyancy...
        Engineering reports stable. Fleet advancing.

        Thirty-three days after the launch of the Endeavour, we found the wreckage of a massive Drifter.
        The metals that surround its hull are resistant to the Nebula, and of unknown composition.
        No known technology has been found. However, on the main bridge deck, writings in old Catali have been located.
        It's some sort of map, relaying coordinates by relativizing position to the largest known mountain peaks of Unitas.

        Whoever left this here for us, must know...

        The fleet is under attack. Contact with Rigid One lost. Contact with Ephemeral Command lost. Contact with the Coalition's flag-rigid lost.
        The Map must be protected at all costs.
        The coordinate lies to the far north.
        You must find it, captain.
        The Expedition must succeed.
        
         
         */
    }
    IEnumerator PlayIntroOld()
    {
        canSkip = false;
        isRunningCinematic = true;

        //SetSceneInstant
        SetScene(null, true);

        yield return new WaitForSeconds(1f);

        StartCoroutine(SkipTextRoutine());
        yield return new WaitForSeconds(1f);
        canSkip = true;
        if (Intro_OST) AUDCO.aud.setOST(Intro_OST);
        yield return new WaitForSeconds(2f);
        SetText("Long ago, the <color=#00AAFF>ORDER OF THE STORM</color> had united humanity.");
        yield return new WaitForSeconds(5f);
        SetScene(Scenes[0], false); //Peaceful Catali world
        yield return new WaitForSeconds(5f);
        SetText("In an era of peace, they invented the <color=#FF00FF>MIST</color>. A substance that would repair the planet's atmosphere...");
        yield return new WaitForSeconds(6f);
        SetText("But something was wrong.");
        yield return new WaitForSeconds(4f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetScene(Scenes[1], false);  //Mist incident (23)
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
        SetScene(Scenes[2], false);  //Purple planet (50)
        yield return new WaitForSeconds(3f);
        SetText("One hundred and eighteen years later.");
        yield return new WaitForSeconds(5f);
        SetText("Humanity had survived.");
        yield return new WaitForSeconds(4f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetScene(Scenes[3], false); //Logipedes
        yield return new WaitForSeconds(3f);
        SetText("They are gathered under the <color=#EEDD33>LOGIPEDES REPUBLIC</color>, obsessed bureaucrats who invented the Drifters...");
        yield return new WaitForSeconds(7f);
        SetText("...From their seats in CAPITOLIS.");
        yield return new WaitForSeconds(5f);
        SetText("Finally, remnants of the <color=#00AAFF>ORDER OF THE STORM</color> had convinced the <color=#EEDD33>THREE LEGISLATES</color> and the <color=#22DD22>NOMADEN COALITION</color>.");
        yield return new WaitForSeconds(7f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetScene(Scenes[4], false);  //Starlight Endeavour Fleet (86)
        yield return new WaitForSeconds(3f);
        SetText("They would launch the <color=#FF00FF>STARLIGHT ENDEAVOUR</color>. A top-secret mission...");
        yield return new WaitForSeconds(6f);
        SetText("...To delve under the <color=#FF00FF>MIST</color>, to find the source, and to destroy it.");
        yield return new WaitForSeconds(7f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetScene(Scenes[5], false);  //Fleet Underway
        yield return new WaitForSeconds(3f);
        SetText("The Expedition travelled for two months before they found a strange signal in the far south.");
        yield return new WaitForSeconds(7f);
        SetText("The fleet moved in, tracking it deep into mountain caverns...");
        yield return new WaitForSeconds(6f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetScene(Scenes[6], false);  //Seeker Ambush (119)
        yield return new WaitForSeconds(2f);
        SetText("But then, an ambush, blasts from the deep piercing through the armour of our proud Drifters!");
        yield return new WaitForSeconds(6f);
        SetText("The fleet fought back. With all they could... But they were no match for the mysterious force that had assailed them.");
        yield return new WaitForSeconds(8f);

        SetText("All they had found was a single coordinate. Far from here, with an unknown purpose...");
        yield return new WaitForSeconds(7f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetScene(Scenes[7], false);  //Nexus Points (144)
        yield return new WaitForSeconds(3f);
        SetText("The <color=#00FFAA>NEXUS POINT</color>.");
        yield return new WaitForSeconds(4f);
        SetText("One coordinate left behind on rocks, deep within the darkness, far to the north...");
        yield return new WaitForSeconds(6f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetScene(Scenes[8], false);  //Governments Failing
        yield return new WaitForSeconds(3f);
        SetText("A revolution has broken out. Evidence that the <color=#FF00FF>MIST</color> is rising has been confirmed.");
        yield return new WaitForSeconds(7f);
        SetText("The governments can no longer support a mission for the good of all humanity when their internal struggles overtake them.");
        yield return new WaitForSeconds(7f);
        SetText("");
        yield return new WaitForSeconds(1f);
        SetScene(Scenes[9], false);  //Map to Nexus Point (177)
        yield return new WaitForSeconds(3f);
        SetText("YOU are the only known survivor of the Expedition. Lead all that remains of us to the <color=#00FFAA>NEXUS POINT</color>...");
        yield return new WaitForSeconds(7f);
        SetText("Uncover the truth. Free Humanity from the <color=#FF00FF>MIST</color>. Before we are consumed, before we destroy ourselves.");
        yield return new WaitForSeconds(7f);
        SetText("Save us from this <color=#FF00FF>MISTWORLD</color>.");
        yield return new WaitForSeconds(3f);
        SetScene(null, false);
        yield return new WaitForSeconds(3f);
        SetText("");
        yield return new WaitForSeconds(2f);
        //202

        //SetImage(Intro_Images[10]); //Title Screen

        //Ending
        isRunningCinematic = false;
        UI.ui.GoBackToPreviousScreen();
    }
    private void SetText(string tex, AudioClip Speak = null)
    {
        StartCoroutine(SwitchTextRoutine(tex, Speak));
    }
    bool isSwitchingText = false;
    int TextID = 0;
    IEnumerator SwitchTextRoutine(string tex, AudioClip Speak)
    {
        // while (isSwitchingText) yield return null;
        TextID++;
        int ID = TextID;
        
        isSwitchingText = true;
        while (TalkTex.color.a > 0)
        {
            TalkTex.color = new Color(1, 1, 1, Mathf.Clamp01(TalkTex.color.a - Time.deltaTime));
            yield return null;
            if (ID != TextID) yield break;
        }
        TalkTex.text = tex;
        TalkTex.maxVisibleCharacters = 0;

        int totalChars = TalkTex.text.Length;
        int visibleCount = 0;
        int speakLetter = 0;

        float Timer = 0f;

        while (visibleCount < totalChars)
        {
            while (Timer > 0f)
            {
                Timer -= Time.deltaTime;
                yield return null;
            }
            Timer += 0.015f;
            visibleCount++;
            TalkTex.maxVisibleCharacters = visibleCount;

            if (Speak)
            {
                char c = TalkTex.text[Mathf.Clamp(visibleCount - 1, 0, totalChars - 1)];
                if (char.IsLetterOrDigit(c))
                {
                    speakLetter--;
                    if (speakLetter < 0)
                    {
                        speakLetter = Random.Range(2, 4);
                        AUDCO.aud.PlaySFX(Speak);
                    }
                }
            }
        }
        isSwitchingText = false;
    }

    private void SetTitle(string title, string subtitle)
    {
        TitleTex.text = title;
        SubtitleTex.text = subtitle;
        StartCoroutine(SwitchTitleRoutine());
    }

    IEnumerator SwitchTitleRoutine()
    {
        TitleTex.color = new Color(0, 1, 1, 1);
        SubtitleTex.color = new Color(0f, 0.8f, 0.8f, 1);
        /*while (TitleTex.color.a < 1)
        {
            TitleTex.color = new Color(0, 1, 1, TitleTex.color.a + Time.deltaTime * 0.5f);
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        while (SubtitleTex.color.a < 1)
        {
            SubtitleTex.color = new Color(0, 0.8f, 0.8f, SubtitleTex.color.a + Time.deltaTime * 0.5f);
            yield return null;
        }*/
        SubtitleTex.maxVisibleCharacters = 0;
        TitleTex.maxVisibleCharacters = 0;
        while (TitleTex.maxVisibleCharacters < TitleTex.text.Length)
        {
            TitleTex.maxVisibleCharacters++;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(1f);
        while (SubtitleTex.maxVisibleCharacters < SubtitleTex.text.Length)
        {
            SubtitleTex.maxVisibleCharacters++;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(7f);
        SubtitleTex.maxVisibleCharacters = SubtitleTex.text.Length;
        while (SubtitleTex.maxVisibleCharacters > 0)
        {
            SubtitleTex.maxVisibleCharacters--;
            yield return new WaitForSeconds(0.02f);
        }
        TitleTex.maxVisibleCharacters = TitleTex.text.Length;
        while (TitleTex.maxVisibleCharacters > 0)
        {
            TitleTex.maxVisibleCharacters--;
            yield return new WaitForSeconds(0.02f);
        }
    }
    IEnumerator SkipTextRoutine()
    {
        while (SkipTex.color.a < 1)
        {
            SkipTex.color = new Color(1, 0, 0, Mathf.Clamp01(SkipTex.color.a + Time.deltaTime));
            yield return null;
        }
        yield return new WaitForSeconds(4f);
        while (SkipTex.color.a > 0)
        {
            SkipTex.color = new Color(1, 0, 0, Mathf.Clamp01(SkipTex.color.a - Time.deltaTime));
            yield return null;
        }
    }

    private void SetScene(ScriptableScene ob, bool Instant = false)
    {
        Cinematic.SetScene(ob, Instant ? -1 : 1.5f);
    }
    /*private void SetImage(Sprite spr)
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
    }*/
    private void Update()
    {
        //float curFade = CinematicImage1.color.a;
        //CinematicImage1.color = new Color(CinematicImage1.color.r, CinematicImage1.color.g, CinematicImage1.color.b, Mathf.Clamp01(curFade + Time.deltaTime * 0.5f));
        //CinematicImage2.color = new Color(CinematicImage2.color.r, CinematicImage2.color.g, CinematicImage2.color.b, Mathf.Clamp01(1f-curFade));

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            if (canSkip) UI.ui.GoBackToPreviousScreenNoPress();
            //AUDCO.aud.setOST(null);
        }
    }
}
