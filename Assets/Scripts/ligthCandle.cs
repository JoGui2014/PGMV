using UnityEngine;

public class CandleController : MonoBehaviour
{
    public GameObject candleLight;
    private bool isLightOn = true;

    void OnMouseUp()
    {
        // Alterna entre acender e apagar a luz quando o objeto é clicado
        if (isLightOn)
        {
            candleLight.SetActive(false);
            isLightOn = false;
        } else
        {
            candleLight.SetActive(true);
            isLightOn = true;
        }
    }
}
