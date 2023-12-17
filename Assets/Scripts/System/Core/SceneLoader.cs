using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class SceneLoader : Singleton<SceneLoader>
{
    public GameObject loadingScreen;
    public Image loadingPanel;
    public Slider loadingSlider;
    public Text loadingText;
    public GameObject CornerLoadingObject;

    public void StartCornerLoading() {
        loadingScreen.SetActive(true);
        Color c = loadingPanel.color;
        loadingPanel.color = new Color(c.r, c.g, c.b, 0);
        loadingSlider.gameObject.SetActive(false);
        CornerLoadingObject.SetActive(true);
    }    

    public void StopCornerLoading() {
        loadingScreen.SetActive(false);
        Color c = loadingPanel.color;
        loadingPanel.color = new Color(c.r, c.g, c.b, 1);
        loadingSlider.gameObject.SetActive(true);
        CornerLoadingObject.SetActive(false);
    }

    public void ChangeScene(SceneId index, bool network = false) {
        loadingScreen.SetActive(true);
        AudioSystem.instance.StopMusic();
        PhotonNetwork.AutomaticallySyncScene = network;
        
        if (!network) {
            PhotonNetwork.AutomaticallySyncScene = false;
            StartCoroutine(ChangeSceneAsync(index));
        }
        else {
            if (!PhotonNetwork.IsConnected)
                return;

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel((int)index);
        
            StartCoroutine(ChangeSceneAsyncPhoton(index));
        }
    }
    
    private IEnumerator ChangeSceneAsync(SceneId index) {
        AsyncOperation operation = SceneManager.LoadSceneAsync(((int)index));

        while(!operation.isDone) {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            loadingSlider.value = progress;
            loadingText.text = (Mathf.CeilToInt(progress * 10000) / 100).ToString() + "%";

            yield return null;
        }

        loadingScreen.SetActive(false);
    }

    private IEnumerator ChangeSceneAsyncPhoton(SceneId index) {
        while (PhotonNetwork.LevelLoadingProgress < 1) {
            float progress = PhotonNetwork.LevelLoadingProgress;
            
            loadingSlider.value = progress;
            loadingText.text = (Mathf.CeilToInt(progress * 10000) / 100).ToString() + "%";
            yield return null;
        }

        loadingScreen.SetActive(false);
    }
}

public enum SceneId {
    Title = 0,
    Main = 1,
    DeckBuilder = 2,
    Portal = 3,
    Battle = 4,
    Room = 5,
}
