using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Mastery : MonoBehaviour
{
    public TextMeshProUGUI Tex;
    public Image Sprite;
    public TooltipObject Tool;
    public int RequireUnlock = -1;
    public Image UnlockLine;
    Color PrimaryColor;
    ScriptableMasteryItem Item;
    int GlobalID;
    MasteryStates State;
    public void SetItem(ScriptableMasteryItem item, int globalid, Color primary)
    {
        Item = item;
        GlobalID = globalid;
        PrimaryColor = primary;

        Tex.text = item.ItemName;
        Tool.Tooltip = item.ItemDesc;
    }

    public void SetState(MasteryStates state)
    {
        State = state;
        switch (State)
        {
            case MasteryStates.LOCKED:
                Sprite.color = new Color(0.3f,0.2f,0.2f);
                Tex.color = new Color(0.3f, 0.2f, 0.2f);
                if (UnlockLine) UnlockLine.color = Color.gray;
                break;
            case MasteryStates.CONNECTED:
                Sprite.color = Color.white;
                Tex.color = Color.white;
                if (UnlockLine) UnlockLine.color = Color.gray;
                break;
            case MasteryStates.RESEARCHABLE:
                Sprite.color = new Color(0.6f + (Mathf.Sin(Time.time * 8f) + 1) * 0.2f, 0.6f + (Mathf.Sin(Time.time * 8f) + 1) * 0.2f, 0.6f + (Mathf.Sin(Time.time * 8f) + 1) * 0.2f);
                Tex.color = Color.white;
                if (UnlockLine) UnlockLine.color = Color.gray;
                break;
            case MasteryStates.UNLOCKED:
                Sprite.color = PrimaryColor;
                Tex.color = PrimaryColor;
                if (UnlockLine) UnlockLine.color = PrimaryColor;
                break;
        }
    }

    private void Update()
    {
        if (State == MasteryStates.RESEARCHABLE)
        {
            Sprite.color = new Color(0.6f + (Mathf.Sin(Time.time * 8f) + 1) * 0.2f, 0.6f + (Mathf.Sin(Time.time * 8f) + 1) * 0.2f, 0.6f + (Mathf.Sin(Time.time * 8f) + 1) * 0.2f);
        }
    }

    public enum MasteryStates
    {
        LOCKED,
        CONNECTED,
        RESEARCHABLE,
        UNLOCKED
    }
    public void WhenPressed()
    {
        CREW Play = LOCALCO.local.GetPlayer();
        if (Play == null)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        if (State != MasteryStates.RESEARCHABLE)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        if (Play.MasteryPoints.Value < 1)
        {
            AUDCO.aud.PlaySFX(AUDCO.aud.Fail);
            return;
        }
        AUDCO.aud.PlaySFX(AUDCO.aud.Upgrade);
        Play.TryUnlockMasteryRpc(GlobalID);
    }
}
