using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI ui;
    public GameObject CurrentlySelectedScreen;
    public GameObject CharacterCreationUI;
    public Screen_GameUI MainGameplayUI;
    public Screen_ShipSelector ShipSelectionUI;
    public Screen_Inventory InventoryUI;
    public Screen_Map MapUI;
    public Screen_Talk TalkUI;
    public Image Crosshair;
    private GameObject PreviousScreen;
    public void SelectScreen(GameObject ob)
    {
        PreviousScreen = CurrentlySelectedScreen;
        if (CurrentlySelectedScreen) CurrentlySelectedScreen.SetActive(false);
        CurrentlySelectedScreen = ob;
        ob.SetActive(true);
    }

    public enum CrosshairModes
    {
        NONE,
        GRAPPLE,
        GRAPPLE_SUCCESS,
        RANGED,
        WEAPONS
    }
    public void SetCrosshair(CrosshairModes mod)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Crosshair.transform.position = new Vector3(pos.x, pos.y);
        switch (mod)
        {
            case CrosshairModes.NONE:
                Crosshair.color = new Color(1, 1, 1, 0);
                break;
            case CrosshairModes.GRAPPLE:
                Crosshair.color = new Color(1, 0, 0, 0.5f);
                break;
            case CrosshairModes.GRAPPLE_SUCCESS:
                Crosshair.color = new Color(0, 1, 0, 0.5f);
                break;
        }
    }
    public void GoBackToPreviousScreen()
    {
        SelectScreen(PreviousScreen);
    }

    private void Awake()
    {
        ui = this;
        SelectScreen(CurrentlySelectedScreen);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
