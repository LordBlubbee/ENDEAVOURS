using UnityEngine;

public class ConvoHandler : MonoBehaviour
{
    private float Timer;
    private int Progress;
    CREW UnitOne;
    CREW UnitTwo;
    Conversation Conversation;
    CREW GetUnit(ConversationPart part)
    {
        return part.IsPersonA ? UnitOne : UnitTwo;
    }
    public void InitConvo(CREW One, CREW Two, Conversation Convo)
    {
        UnitOne = One;
        UnitTwo = Two;
        Conversation = Convo;
    }
    void Update()
    {
        Timer -= CO.co.GetWorldSpeedDelta();
        if (!CO.co.IsSafe() || UnitOne == null || UnitTwo == null)
        {
            EndConvo();
            return;
        }
        if (Timer < 0)
        {
            if (Progress >= Conversation.Parts.Count)
            {
                EndConvo();
                return;
            }
            if ((UnitOne.transform.position-UnitTwo.transform.position).magnitude > 20f)
            {
                EndConvo();
                return;
            }
            ConversationPart part = Conversation.Parts[Progress];
            Progress++;
            GetUnit(part).SpawnVoiceRpc(part.Voice.VoiceTex, part.Voice.Style);
            Timer = 2.7f + part.Voice.VoiceTex.Length*0.03f;
            GetUnit(part).GetVoiceHandler().SetCooldown(Timer + 5f);
        }
    }

    void EndConvo()
    {
        if (UnitOne != null) UnitOne.GetAI().SwitchTacticsCrewOutOfCombat();
        if (UnitTwo != null) UnitTwo.GetAI().SwitchTacticsCrewOutOfCombat();
        Destroy(gameObject);
    }
}
