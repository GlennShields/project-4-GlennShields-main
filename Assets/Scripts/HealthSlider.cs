using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    public void SetHealth(int health)

    {
        float healthPercentage = Mathf.Clamp01(health / 100f);
        GetComponent<Slider>().value = healthPercentage;
        float tensionPercentage = Mathf.Pow(healthPercentage, 2f);
        GetComponent<Slider>().value = tensionPercentage;
    }
}
