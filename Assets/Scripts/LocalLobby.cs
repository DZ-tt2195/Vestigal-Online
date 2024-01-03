using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LocalLobby : MonoBehaviour
{
    [SerializeField] TMP_InputField player1;
    [SerializeField] TMP_InputField player2;
    [SerializeField] TMP_Text error;
    [SerializeField] Toggle fullScreen;
    [SerializeField] Button playGame;

    private void Start()
    {
        error.gameObject.SetActive(false);
        if (PlayerPrefs.HasKey("P1"))
            player1.text = PlayerPrefs.GetString("P1");
        if (PlayerPrefs.HasKey("P2"))
            player2.text = PlayerPrefs.GetString("P2");

        fullScreen.isOn = Screen.fullScreen;
        fullScreen.onValueChanged.AddListener(delegate { WindowMode(); });
        playGame.onClick.AddListener(LoadGame);
    }

    public void WindowMode()
    {
        Screen.fullScreenMode = (fullScreen.isOn) ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
    }

    IEnumerator ErrorMessage(string x)
    {
        error.gameObject.SetActive(true);
        error.text = x;
        yield return new WaitForSeconds(3f);
        error.gameObject.SetActive(false);
    }

    void LoadGame()
    {
        if (player1.text == "" || player2.text == "")
        {
            StartCoroutine(ErrorMessage("One player doesn't have a name."));
        }
        else
        {
            PlayerPrefs.SetString("P1", player1.text);
            PlayerPrefs.SetString("P2", player2.text);
            SceneManager.LoadScene("2. Game");
        }
    }
}
