using UnityEngine;

public class UI_TalkChooser : MonoBehaviour
{
    public int ChoiceIndex; //
    public void Press()
    {
        CO_STORY.co.SubmitClientChoice(ChoiceIndex);
    }
}
