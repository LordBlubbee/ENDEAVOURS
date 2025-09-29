using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Netcode;

public class CO_STORY : NetworkBehaviour
{
    [NonSerialized] public NetworkVariable<bool> CommsActive = new();
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

    public string GetCurrentChoice(int index)
    {
        switch (index)
        {
            case 0: return CurrentChoices0.Value.ToString();
            case 1: return CurrentChoices1.Value.ToString();
            case 2: return CurrentChoices2.Value.ToString();
            case 3: return CurrentChoices3.Value.ToString();
            case 4: return CurrentChoices4.Value.ToString();
            case 5: return CurrentChoices5.Value.ToString();
            case 6: return CurrentChoices6.Value.ToString();
            default: return string.Empty;
        }
    }
    public string GetMainStoryText(int index)
    {
        switch (index)
        {
            case 0: return MainStoryText0.Value.ToString();
            case 1: return MainStoryText1.Value.ToString();
            case 2: return MainStoryText2.Value.ToString();
            case 3: return MainStoryText3.Value.ToString();
            case 4: return MainStoryText4.Value.ToString();
            case 5: return MainStoryText5.Value.ToString();
            case 6: return MainStoryText6.Value.ToString();
            case 7: return MainStoryText7.Value.ToString();
            case 8: return MainStoryText8.Value.ToString();
            case 9: return MainStoryText9.Value.ToString();
            default: return string.Empty;
        }
    }
    public bool IsCommsActive()
    {
        return CommsActive.Value;
    }
    public bool IsLastMainStoryText(int index)
    {
        // There are 10 MainStoryText fields, indexed 0-9
        if (index < 0 || index > 9)
            return false; // Out of range

        // If next index is out of range, this is the last
        if (index == 9)
            return true;

        // Check if the next MainStoryText is empty
        string nextText = GetMainStoryText(index + 1);
        return string.IsNullOrEmpty(nextText);
    }
    //The visual is provided by a scripted element, as is character voice and background music
    public static CO_STORY co;

    [NonSerialized] public bool ShouldUpdate = true;

    private void Start()
    {
        co = this;
    }
    public void SetCurrentChoices(List<string> ChoiceList)
    {
        CurrentChoices0.Value = ChoiceList.Count > 0 ? ChoiceList[0] : "";
        CurrentChoices1.Value = ChoiceList.Count > 1 ? ChoiceList[1] : "";
        CurrentChoices2.Value = ChoiceList.Count > 2 ? ChoiceList[2] : "";
        CurrentChoices3.Value = ChoiceList.Count > 3 ? ChoiceList[3] : "";
        CurrentChoices4.Value = ChoiceList.Count > 4 ? ChoiceList[4] : "";
        CurrentChoices5.Value = ChoiceList.Count > 5 ? ChoiceList[5] : "";
        CurrentChoices6.Value = ChoiceList.Count > 6 ? ChoiceList[6] : "";

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
        return num;
    }
    public int VoteResultAmount(int num)
    {
        int count = 0;
        foreach (LOCALCO local in CO.co.GetLOCALCO())
        {
            if (local.CurrentDialogVote.Value == num) count++;
        }
        return count;
    }
    public void SetMainStoryText(List<string> StoryList)
    {
        Debug.Write(StoryList.Count);
        MainStoryText0.Value = StoryList.Count > 0 ? StoryList[0] : "";
        MainStoryText1.Value = StoryList.Count > 1 ? StoryList[1] : "";
        MainStoryText2.Value = StoryList.Count > 2 ? StoryList[2] : "";
        MainStoryText3.Value = StoryList.Count > 3 ? StoryList[3] : "";
        MainStoryText4.Value = StoryList.Count > 4 ? StoryList[4] : "";
        MainStoryText5.Value = StoryList.Count > 5 ? StoryList[5] : "";
        MainStoryText6.Value = StoryList.Count > 6 ? StoryList[6] : "";
        MainStoryText7.Value = StoryList.Count > 7 ? StoryList[7] : "";
        MainStoryText8.Value = StoryList.Count > 8 ? StoryList[8] : "";
        MainStoryText9.Value = StoryList.Count > 9 ? StoryList[9] : "";
    }
    public void SetStoryEnd()
    {
        CommsActive.Value = false;
        SetCurrentChoices(new());
        SetMainStoryText(new());
        ForceUpdateRpc();
    }

    private ScriptableDialog CurrentDialog;
    public void SetStory(ScriptableDialog Dialog)
    {
        CommsActive.Value = true;
        CurrentDialog = Dialog;
        List<string> StoryList = new();
        foreach (DialogPart line in Dialog.StoryTexts)
        {
            StoryList.Add(ReturnDialogPart(line));
        }
        SetMainStoryText(StoryList);
        StoryList = new();
        foreach (PossibleNextDialog line in Dialog.ChoicePathDialogs)
        {
            DialogPart showChoice = line.ChoiceText;
            StoryList.Add(ReturnDialogPart(showChoice));
        }
        SetCurrentChoices(StoryList);
        ForceUpdateRpc();
    }
    string ReturnDialogPart(DialogPart showChoice)
    {
        foreach (AlternativeDialogPart alternatives in showChoice.Alternatives)
        {
            if (alternatives.ArePrerequisitesMet())
            {
                return alternatives.AlternativeText;
            }
        }
        return showChoice.BaseText;
    }

    [Rpc(SendTo.Server)]
    public void SubmitClientChoiceRpc(int choiceIndex)
    {
        //Client only
        if (LOCALCO.local.CurrentDialogVote.Value == choiceIndex) LOCALCO.local.CurrentDialogVote.Value = -1;
        else LOCALCO.local.CurrentDialogVote.Value = choiceIndex;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ForceUpdateRpc()
    {
        ShouldUpdate = true;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (HasVoteResult() != -1)
        {
            int result = HasVoteResult();
            foreach (LOCALCO local in CO.co.GetLOCALCO())
            {
                local.CurrentDialogVote.Value = -1;
            }
            SetNextStory(result);
            PerformEvent(CurrentDialog.TriggerEvent);
            SetStoryEnd();
        }
    }

    private void SetNextStory(int result)
    {
        foreach (AlternativeDialog alternatives in CurrentDialog.ChoicePathDialogs[result].AlternativeResults)
        {
            if (alternatives.ArePrerequisitesMet())
            {
                SetStory(alternatives.ReplaceDialog);
                return;
            }
        }
        SetStory(CurrentDialog.ChoicePathDialogs[result].DialogResult);
    }

    private void PerformEvent(string str)
    {
        //LOOOOOOOOOOOOOOOOOOOOOOOOONG LIST
        switch (str)
        {
        }
    }
}
