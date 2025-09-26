using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public class CO_STORY : NetworkBehaviour
{
    [NonSerialized] public NetworkVariable<FixedString512Bytes> CurrentChoices0 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> CurrentChoices1 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> CurrentChoices2 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> CurrentChoices3 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> CurrentChoices4 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> CurrentChoices5 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> CurrentChoices6 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText0 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText1 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText2 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText3 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText4 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText5 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText6 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText7 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText8 = new();
    [NonSerialized] public NetworkVariable<FixedString512Bytes> MainStoryText9 = new();
    //The visual is provided by a scripted element, as is character voice and background music
    public static CO_STORY co;

    private void Start()
    {
        co = this;
    }
    public void SetCurrentChoices(List<string> ChoiceList)
    {
        CurrentChoices0.Value = ChoiceList.Count > 0 ? ChoiceList[0] : default;
        CurrentChoices1.Value = ChoiceList.Count > 1 ? ChoiceList[1] : default;
        CurrentChoices2.Value = ChoiceList.Count > 2 ? ChoiceList[2] : default;
        CurrentChoices3.Value = ChoiceList.Count > 3 ? ChoiceList[3] : default;
        CurrentChoices4.Value = ChoiceList.Count > 4 ? ChoiceList[4] : default;
        CurrentChoices5.Value = ChoiceList.Count > 5 ? ChoiceList[5] : default;
        CurrentChoices6.Value = ChoiceList.Count > 6 ? ChoiceList[6] : default;

        foreach (LOCALCO local in CO.co.GetLOCALCO())
        {
            local.CurrentDialogVote.Value = -1;
        }
    }
    private int HasVoteResult()
    {
        int num = -1;
        foreach (LOCALCO local in CO.co.GetLOCALCO())
        {
            if (local.CurrentDialogVote.Value != -1)
            {
                if (num == -1) num = local.CurrentDialogVote.Value;
                else if (num != local.CurrentDialogVote.Value) return -1; //No consensus
            }
            else return -1; //Not everyone has voted
        }
        return -1;
    }
    public void SetMainStoryText(List<string> StoryList)
    {
        MainStoryText0.Value = StoryList.Count > 0 ? StoryList[0] : default;
        MainStoryText1.Value = StoryList.Count > 1 ? StoryList[1] : default;
        MainStoryText2.Value = StoryList.Count > 2 ? StoryList[2] : default;
        MainStoryText3.Value = StoryList.Count > 3 ? StoryList[3] : default;
        MainStoryText4.Value = StoryList.Count > 4 ? StoryList[4] : default;
        MainStoryText5.Value = StoryList.Count > 5 ? StoryList[5] : default;
        MainStoryText6.Value = StoryList.Count > 6 ? StoryList[6] : default;
        MainStoryText7.Value = StoryList.Count > 7 ? StoryList[7] : default;
        MainStoryText8.Value = StoryList.Count > 8 ? StoryList[8] : default;
        MainStoryText9.Value = StoryList.Count > 9 ? StoryList[9] : default;
    }
    public void ResolveStory()
    {
        SetCurrentChoices(new());
        SetMainStoryText(new());
    }
    public void SubmitClientChoice(int choiceIndex)
    {
        //Client only
        LOCALCO.local.CurrentDialogVote.Value = choiceIndex;
    }
}
