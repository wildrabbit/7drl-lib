using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GameData", menuName = "7DRL_Lib/GameData")]
public class GameData: ScriptableObject
{
    public float InputDelay;
    public float DefaultTimescale;
    public bool BumpingWallsWillSpendTurn;

    public BaseMapData MapData;
    public BaseEntityCreationData EntityCreationData; // Prefabs, pool stuff, etc    
    public BaseInputData InputData;
}