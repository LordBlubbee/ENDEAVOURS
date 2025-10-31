using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapPointButton : MonoBehaviour
{
    public Image Image;
    public TextMeshProUGUI Texto;
    Color DefaultColor = Color.white;
    private MapPoint AssociatedPoint;
    private int DestinationID = -1;
    public void Init(MapPoint pon, int dest, bool NameVisible)
    {
        AssociatedPoint = pon;
        DestinationID = dest;
        if (NameVisible) Texto.text = pon.GetData();
    }
    public void SetDefaultColor(Color col)
    {
         Image.color = col;
        Texto.color = col;
        DefaultColor = col;
    }
    public void HoverOver()
    {
        Image.color = Color.yellow;
        Texto.color = Color.yellow;
    }
    public void StopHoverOver()
    {
        Image.color = DefaultColor;
        Texto.color = DefaultColor;
    }
    public void Pressed()
    {
        CO.co.VoteForMapRpc(LOCALCO.local.PlayerID.Value, DestinationID);
    }

    public MapPoint GetAssociatedPoint()
    {
        return AssociatedPoint;
    }
}
