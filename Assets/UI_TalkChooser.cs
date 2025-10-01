using UnityEngine;

public class UI_TalkChooser : MonoBehaviour
{
    public int ChoiceIndex; //
    public void Press()
    {
        CO_STORY.co.SubmitClientChoiceRpc(LOCALCO.local.PlayerID.Value, ChoiceIndex);
    }

    public void PressMap()
    {
        CO.co.VoteForMapRpc(LOCALCO.local.PlayerID.Value, ChoiceIndex);
    }
}
