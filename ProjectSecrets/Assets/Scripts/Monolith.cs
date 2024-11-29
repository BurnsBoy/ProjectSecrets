using System.Linq;
using UnityEngine;

public class Monolith : MonoBehaviour
{
    public MeshRenderer[] puzzleLights;
    public Material puzzleLight;
    bool[,] matrix;

    public MeshRenderer[] redLights;
    public MeshRenderer[] greenLights;
    public MeshRenderer[] blueLights;

    public Material redLight;
    public Material greenLight;
    public Material blueLight;

    public GameObject redBattery;
    public GameObject greenBattery;
    public GameObject blueBattery;

    int answer;
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            matrix = player.portalPad.TranslateIntToMatrix(player.portalPad.portalDestinations.FirstOrDefault(x => x.Value == "Home").Key);
            if (player.hasRedBattery)
            {
                LightUp(redLights, redLight, redBattery);
                PuzzleLight(0);
            }
            if (player.hasGreenBattery)
            {
                LightUp(greenLights, greenLight, greenBattery);
                PuzzleLight(1);
            }
            if (player.hasBlueBattery)
            {
                LightUp(blueLights, blueLight, blueBattery);
                PuzzleLight(2);
            }
        }
    }
    void LightUp(MeshRenderer[] lights, Material lightMat, GameObject battery)
    {
        battery.SetActive(true);
        foreach (var light in lights)
        {
            light.material = lightMat;
        }
    }
    void PuzzleLight(int r)
    {
        for (int i = 0; i < 3; i++)
        {
            if (matrix[i, r])
                puzzleLights[r * 3 + i].material = puzzleLight;
        }
    }
}
