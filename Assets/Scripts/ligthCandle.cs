using UnityEngine;

public class CandleController : MonoBehaviour
{
    public GameObject candleLight;


    private bool isLightOn;

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Alterna entre acender e apagar a luz
            if (isLightOn)
            {
                candleLight.SetActive(false);
                isLightOn = false;
            }
            else
            {
                candleLight.SetActive(true);
                isLightOn = true;
            }
        }
    }

}
