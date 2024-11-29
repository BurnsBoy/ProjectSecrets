using UnityEngine;

public class Battery : MonoBehaviour
{
    public string color;
    bool destroy;
    bool didCheck;
    void Update()
    {
        if (!didCheck)
        {
            Player player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
            if (player != null)
            {
                didCheck = true;
                switch (color)
                {
                    case "Red": destroy = player.hasRedBattery; break;
                    case "Green": destroy = player.hasGreenBattery; break;
                    case "Blue": destroy = player.hasBlueBattery; break;
                }
                Debug.Log(destroy);
                if (destroy)
                    Destroy(gameObject);
            }
        }
    }
}
