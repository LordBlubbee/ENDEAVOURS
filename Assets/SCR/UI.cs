using UnityEngine;

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
    private GameObject PreviousScreen;
    public void SelectScreen(GameObject ob)
    {
        PreviousScreen = CurrentlySelectedScreen;
        if (CurrentlySelectedScreen) CurrentlySelectedScreen.SetActive(false);
        CurrentlySelectedScreen = ob;
        ob.SetActive(true);
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
