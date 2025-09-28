using UnityEngine;

public class UI_TalkChooser : MonoBehaviour
{
    public int ChoiceIndex; //
    public void Press()
    {
        CO_STORY.co.SubmitClientChoiceRpc(ChoiceIndex);
    }

    public void PressMap()
    {
        CO.co.VoteForMapRpc(ChoiceIndex);
    }
}
