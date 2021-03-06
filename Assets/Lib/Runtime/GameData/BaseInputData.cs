using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class DirectionDataMapping
{
    public MoveDirection MoveDir;
    public KeyCode KeyCode;
}

[System.Serializable]
public class MappingLayout
{
    [FormerlySerializedAs("_mappings")] public List<DirectionDataMapping> Mappings;
}

[CreateAssetMenu(fileName = "New move input mapping", menuName = "7DRL_Lib/Data/New menu mapping")]
public class BaseInputData : ScriptableObject
{
    public int NumberKeys;
    public LayoutType DefaultLayout;
    [FormerlySerializedAs("_layouts")]public List<MappingLayout> Layouts;
}
