using System;
using UnityEngine;

public class MovingPlatformSystem : MonoBehaviour
{
    public Transform platform;
    public Rigidbody platformBody;
    public Transform[] locations;
    public AudioSource audioSource;
    public MeshRenderer meshRenderer;
    public Material onMaterial;
    public bool active;
    public float speed;
    public bool circularPath;
    public float pauseTime;
    float timer;
    int direction;
    int target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = 1;
        target = 1;
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.volume = platformBody.linearVelocity.magnitude / speed;
        if (active)
        {
            if (timer >= pauseTime)
            {
                platformBody.linearVelocity = (locations[target].position - platform.position).normalized * speed;
                if (Vector3.Distance(platform.position, locations[target].position) < 1)
                {
                    target += direction;
                    if (target == locations.Length)
                    {
                        if (circularPath)
                            target = 0;
                        else
                        {
                            platformBody.linearVelocity = Vector3.zero;
                            timer = 0;
                            target = locations.Length - 2;
                            direction = -1;
                        }
                    }
                    else if (target == -1)
                    {
                        platformBody.linearVelocity = Vector3.zero;
                        timer = 0;
                        target = 1;
                        direction = 1;
                    }
                }
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
        else
        {
            platform.position = locations[0].position;
        }
    }
}
