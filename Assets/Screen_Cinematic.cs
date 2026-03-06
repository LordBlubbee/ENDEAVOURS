using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Screen_Cinematic : MonoBehaviour
{
    [Header("REFERENCES")]
    public CinematicPlayer Cinematic;
    public TextMeshProUGUI TalkTex;
    public Image TalkBack;
    public TextMeshProUGUI TitleTex;
    public TextMeshProUGUI SubtitleTex;
    public TextMeshProUGUI SkipTex;

    [Header("IMAGES")]
    public AudioClip Intro_OST;
    public AudioClip VCX_Narrator;
    public ScriptableScene[] Scenes;

    bool isRunningCinematic = false;
    bool canSkip = false;
    public void PlayIntroCinematic()
    {
        //UI.ui.SelectScreen(gameObject);
        gameObject.SetActive(true);
        if (!isRunningCinematic) StartCoroutine(PlayIntro());
    }
    public void OnDisable()
    {
        canSkip = false;
        isRunningCinematic = false;
        Cinematic.IsMuted = true;
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

        //SetScene(Scenes[1], false); //Cliffs
        // yield return new WaitForSeconds(10f);
        SetScene(Scenes[1], false); //Planet
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[2], false); //Capitolis
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[0], false); //Driftyards
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[3], true); //Driftyards 2
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[8], true); //8 = Frontal View Travel
        yield return new WaitForSeconds(12f);
        SetScene(Scenes[4], true); //4 = Underway
        yield return new WaitForSeconds(2f);
        SetTitle("STARLIGHT ENDEAVOUR EXPEDITION", "121 AD NEBULA - SOUTHERN REACHES");
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[5], true); //5 = Underway Night
        yield return new WaitForSeconds(4f);
        SetScene(Scenes[6], true); //6 = Underway Forests
        yield return new WaitForSeconds(4f);
        SetScene(Scenes[14], false); //14 = Map1
        //30 SECONDS IN TOTAL
        yield return new WaitForSeconds(2f);
        SetText("The <color=#FF00FF>Endeavour</color> has successfully gathered and launched from Capitolis.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("One year ago, the <color=#FF00FF>Fluctus Nebulae</color> destroyed at least ten percent of all known settlements on Unitas.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("The source has been triangulated to a location deep past the Nomaden Reaches, to a mountain in the far south-east.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("We are armed against Nebuloid and pirate threats, and have many supply bases built along parts of our route. We are to maintain level four secrecy.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetScene(Scenes[17], true); //17 = Scout Drifter goes on
        yield return new WaitForSeconds(2f);
        SetText("Twelve days into the <color=#FF00FF>Endeavour</color>, Stellae Frontier Scouting Blimp 07-10 maintains an eight-hour distance from the core fleet.", VCX_Narrator);
      
        yield return new WaitForSeconds(8f);
        SetText("Its Gubernator feels unusual distortions in the Flux. The effects are similar to recorded characteristics of the <color=#FF00FF>Fluctus Nebulae</color>.", VCX_Narrator);
        SetScene(Scenes[7], false); //7 = Gubernator
        yield return new WaitForSeconds(4f);
        SetScene(Scenes[16], true); //16 = Map with Signals
        yield return new WaitForSeconds(4f);
        SetText("We are to rendezvous with our scouting Drifter to investigate the possible source area, which lies inside a vast cave network.", VCX_Narrator);
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[10], true); //10 = Underway Cave
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[13], true); //13 = Drifter finds Wreck
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[9], true); //9 = Cave Wreck Captain's View
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[11], true);  //11 = Catali ground team finds Wreck
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[20], true); //20 = Enter the Wreck
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[19], true); //19 = Looting the Tubes
        yield return new WaitForSeconds(6f);
        SetScene(Scenes[12], true); //12 = The Fragmentum
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[15], true); //15 = Iridaceae Sketch
        yield return new WaitForSeconds(2f);
        SetText("The object of origin is a massive, highly advanced Drifter of unknown affiliation, design, or purpose.", VCX_Narrator);
        yield return new WaitForSeconds(8f);
        SetText("The cause of its rupture is unknown and there are no traces of a crew.", VCX_Narrator);
        yield return new WaitForSeconds(8f);
        SetScene(Scenes[27], true); //27 = Nexus Sketch
        yield return new WaitForSeconds(2f);
        SetText("The <color=#00AAFF>Catali</color> salvaging team believes the texts inside are an ancient form of their language.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("This map uses the mountain peaks of Unitas to describe a location known as <color=#00FFFF>NEXUS POINT</color>. The fleet must set course to learn more.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetScene(Scenes[26], false); //26 = Map2
        SetText("Our last courier from <color=yellow>Capitolis</color> has informed us that the situation in Unitas is deteriorating.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("The <color=#00AAFF>Catali</color> have been fractured in two, an internal war causing it to split into the <color=#0022FF>Democrats</color> and <color=#00AAFF>Royals</color>, the latter of which were banished from Valley.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("A group of Nomaden known as the <color=red>Bakuto Clan</color> have acquired a massive war fleet, and declared the <color=red>Insurrection</color>.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("All while the Nebula rises, and emergency services fail to cope with the rising humanitarian needs.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("Yet, there is hope. We believe the map must lead us to more answers about the true nature of the Nebula.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("We have plotted a new course, avoiding the Insurrectionist fleets. We will refuel at <color=#FF00FF>Starlight Point One</color> and make way for the <color=#00FFFF>NEXUS POINT</color>.", VCX_Narrator);
 
        yield return new WaitForSeconds(9f); //44 TOTAL

        SetScene(Scenes[18], true); //18 = Discussions + Explosion

        yield return new WaitForSeconds(10f);

        SetScene(Scenes[21], true); //21 = RisingUp
        yield return new WaitForSeconds(6f);

        SetScene(Scenes[23], true); //23 = EphemeralDying
        yield return new WaitForSeconds(4f);
        SetScene(Scenes[22], true); //22 = RED ALERT
        SetText("<color=red>ALERT LEVEL NINE. THE EXPEDITION IS UNDER ATTACK.</color>", VCX_Narrator);
        yield return new WaitForSeconds(4f);

        SetScene(Scenes[24], true); //24 = Engine Room
        yield return new WaitForSeconds(2f);
        SetText("<color=red>ALL PERSONNEL, PREPARE TO ENGAGE HOSTILE ID 01.</color>", VCX_Narrator);
        yield return new WaitForSeconds(4f);
        SetScene(Scenes[29], true); //28 = Logipedes Attack!!
        yield return new WaitForSeconds(5f);
        SetScene(Scenes[25], true); //25 = Attacking Seekers
        yield return new WaitForSeconds(6f);
        SetScene(Scenes[33], true); //33 = Cannonfire
        yield return new WaitForSeconds(4f);
        SetScene(Scenes[36], true); //36 = Attacking Seekers and we're dying
        yield return new WaitForSeconds(4f);
        SetScene(Scenes[30], true); //28 = Explosion
        yield return new WaitForSeconds(4f);
        SetScene(Scenes[28], true); //28 = Death1
        yield return new WaitForSeconds(2f);
        SetScene(Scenes[32], true); //32 = Lightning
        yield return new WaitForSeconds(4f);
        //SetScene(Scenes[28], true); //28 = Death1
        // yield return new WaitForSeconds(2f);
        SetScene(Scenes[34], true); //34 = Last Communication
        yield return new WaitForSeconds(2f);
        SetText("Vessel ID 14. Please acknowledge. You are the last remaining Endeavour vessel.", VCX_Narrator);
        yield return new WaitForSeconds(7f);
        SetText("Find the <color=#00FFFF>Nexus Point</color>. May All Be United ---", VCX_Narrator);
        yield return new WaitForSeconds(3f);
        SetScene(Scenes[35], true); //35 = Map of our escape
        yield return new WaitForSeconds(10f);
        SetScene(Scenes[31], true); //31 = End Shot
        yield return new WaitForSeconds(16f);
        SetScene(Scenes[1], true); //Planet
        yield return new WaitForSeconds(8f);

        UI.ui.MainMenuUI.EnableMainMenu(true);
        canSkip = false;
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
                if (ID != TextID) yield break;
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
                        if (!Cinematic.IsMuted) AUDCO.aud.PlaySFX(Speak);
                    }
                }
            }
        }
        yield return new WaitForSeconds(7f);
        if (ID != TextID) yield break;
        while (visibleCount > 0)
        {
            while (Timer > 0f)
            {
                Timer -= Time.deltaTime;
                yield return null;
                if (ID != TextID) yield break;
            }
            Timer += 0.005f;
            visibleCount--;
            TalkTex.maxVisibleCharacters = visibleCount;
        }

        /*while (TalkTex.color.a > 0)
        {
            TalkTex.color = new Color(1, 1, 1, Mathf.Clamp01(TalkTex.color.a - Time.deltaTime));
            yield return null;
            if (ID != TextID) yield break;
        }*/
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
        Cinematic.SetScene(ob, Instant ? 0f : 1.5f);
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
            if (canSkip)
            {
                UI.ui.MainMenuUI.EnableMainMenu(true);
                canSkip = false;
                Cinematic.IsMuted = true;
            }
            //AUDCO.aud.setOST(null);
        }
        if (TalkTex.text.Length > 0 && TalkTex.maxVisibleCharacters > 5)
        {
            TalkBack.color = new Color(0, 0, 0, Mathf.Clamp(TalkBack.color.a + Time.deltaTime, 0, 0.8f));
        } else
        {
            TalkBack.color = new Color(0, 0, 0, Mathf.Clamp(TalkBack.color.a - Time.deltaTime,0,0.8f));
        }
    }
}
