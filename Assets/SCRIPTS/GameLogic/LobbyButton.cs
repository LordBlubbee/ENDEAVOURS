using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyButton : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Players;
    protected string lobbyID;
    public void SetLobbyButton(Lobby lobby)
    {
        lobbyID = lobby.Id;
        Title.text = $"{lobby.Name}";
        Players.text = $"({lobby.Players.Count}/{lobby.MaxPlayers})";
        if (lobby.Players.Count == lobby.MaxPlayers) Players.color = Color.red;
        else Players.color = Color.cyan;
    }
    public void WhenPressed()
    {
        LOBBY.lobby.PressJoinLobby(lobbyID);
    }
}
