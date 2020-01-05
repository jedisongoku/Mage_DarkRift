using UnityEngine.SceneManagement;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{


    public void PlayGame()
    {
        SceneManager.LoadScene("PoisonShop");
    }
}
