using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    private readonly HashSet<Player> playersSelecting = new HashSet<Player>();
    private readonly List<Player> subscribedPlayers = new List<Player>();

    private void Start()
    {
        foreach (Player player in Player.AllPlayers)
        {
            player.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
            subscribedPlayers.Add(player);
        }
    }

    private void OnDestroy()
    {
        foreach (Player player in subscribedPlayers)
        {
            if (player != null)
            {
                player.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
            }
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (sender is Player player)
        {
            if (e.selectedCounter == baseCounter)
            {
                playersSelecting.Add(player);
            }
            else
            {
                playersSelecting.Remove(player);
            }
        }

        if (playersSelecting.Count > 0)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(false);
        }
    }
}
