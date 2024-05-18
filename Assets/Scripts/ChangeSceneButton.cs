using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    public string sceneName; // Nome da cena para a qual queremos mudar

    private Button myButton;

    void Start()
    {
        // Obter a referência ao componente Button
        myButton = GetComponent<Button>();

        // Inicialmente, ocultar o botão
        // Ativa dentro do if
       // myButton.gameObject.SetActive(false);

        // Adicionar o listener ao botão para mudar de cena quando clicado
        myButton.onClick.AddListener(ChangeScene);
    }

    void ChangeScene()
    {
        // Carregar a cena especificada
        SceneManager.LoadScene(sceneName);
    }
}
