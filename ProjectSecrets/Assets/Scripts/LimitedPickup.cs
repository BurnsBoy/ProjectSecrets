using UnityEngine;

public class LimitedPickup : MonoBehaviour
{
    public string pickupName;
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
                switch (pickupName)
                {
                    case "Jetpack": destroy = player.hasJetpack; break;
                    case "Grapple": destroy = player.hasGrappleHook; break;
                    case "MagShoes": destroy = player.hasMagShoes; break;
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
