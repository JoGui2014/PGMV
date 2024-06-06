using UnityEngine;

public class CandleController : MonoBehaviour
{

    // Reference to the candle light GameObject
    public GameObject candleLight;
    // Boolean that tracks the state of the candle light
    private bool isLightOn = true;

    // This method is called when the mouse button is released over the Box Collider attached to this GameObject
    void OnMouseUp()
    {
        // Toglle the candle light on an off then the candles it's clicked, if it's on it turn it off and vice versa
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
