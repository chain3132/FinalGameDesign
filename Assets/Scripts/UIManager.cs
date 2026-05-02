using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void StartGame()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1f;
        }
        SceneManager.LoadScene("30Per");
    }
}
