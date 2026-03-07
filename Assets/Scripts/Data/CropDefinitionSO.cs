using UnityEngine;

[CreateAssetMenu(menuName = "Stardewlike/Crop Definition")]
public class CropDefinitionSO : ScriptableObject
{
    public string cropId = "Carrot Seed";

    // stages 0..4 (Day0..Day4)
    public Sprite[] stageSprites = new Sprite[5];
}