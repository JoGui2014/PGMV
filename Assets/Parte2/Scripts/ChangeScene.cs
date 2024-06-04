using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // scene to switch to
    public string sceneName;

    // Reference to the room GameObject
    public GameObject room;

    // Reference to the Change Scene Button component
    private Button myButton;

    void Start()
    {
        // Check if a GameObject named "Room" exists in the scene
        if (GameObject.Find("Room") == null) {
            // If room is not found, get the first root GameObject from the first scene loaded
            room = SceneManager.GetSceneAt(0).GetRootGameObjects()[0];
        } else {
            // If room is found, assign it to the room variable
            room = GameObject.Find("Room");
        }

        // Add the ChangeToScene method as a listener to the button's onClick event
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(ChangeToScene);
    }

    // Method to handle scene change
    void ChangeToScene()
    {
        // Check if there are not exactly 2 loaded scenes
        if (SceneManager.sceneCount != 2) {
            // Deactivate the room GameObject and load the specified scene additively (without unloading the current scene)
            room.SetActive(false);
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

        } else {
            // Get the second scene (index 1) which should be the additional loaded scene
            Scene thisScene = SceneManager.GetSceneAt(1);
            // Unload the additional scene
            SceneManager.UnloadSceneAsync(thisScene);
            // Reactivate the room GameObject
            room.SetActive(true);
        }
    }
}
