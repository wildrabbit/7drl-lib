using UnityEngine;

[System.Serializable]
public class HPTraitData
{
    public int MaxHP;
    public int StartHP;
    public bool Regen;
    public float RegenRate;
    public int RegenAmount; // amount or percent??
}


public class BaseEntityData: ScriptableObject
{
    public Transform DefaultViewPrefab;
    public string DisplayName;
}
