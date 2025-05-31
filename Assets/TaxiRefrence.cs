using UnityEngine;

public class TaxiReference : MonoBehaviour
{
    public static TaxiReference Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
