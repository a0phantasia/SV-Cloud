using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : Manager<MainManager>
{
    private void Start() {
        AudioSystem.instance.PlayMusic("Main");
    }

    public void BackToTitleScene() {
        SceneLoader.instance.ChangeScene(SceneId.Title);
    }

    public void GoToPortalScene() {
        SceneLoader.instance.ChangeScene(SceneId.Portal);
    }
}
