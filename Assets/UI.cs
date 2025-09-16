using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI ui;
    public GameObject CurrentlySelectedScreen;
    public GameObject CharacterCreationUI;
    public GameObject MainGameplayUI;
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
