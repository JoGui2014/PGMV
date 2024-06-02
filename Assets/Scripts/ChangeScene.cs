using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string sceneName;
    public GameObject room;

    private Button myButton;

    void Start()
    {
        if (GameObject.Find("Room") == null){
            room = SceneManager.GetSceneAt(0).GetRootGameObjects()[0];
        }else{
            room = GameObject.Find("Room");
        }

        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(ChangeToScene);
    }

    void ChangeToScene()
    {
        if (SceneManager.sceneCount != 2){
            room.SetActive(false);
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        }else{
            Scene thisScene = SceneManager.GetSceneAt(1);
            SceneManager.UnloadSceneAsync(thisScene);
            room.SetActive(true);

        }


    }
}
