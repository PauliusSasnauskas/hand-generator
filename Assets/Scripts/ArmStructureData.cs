using System.Collections.Generic;

[System.Serializable]
public class ArmStructureData {
    public bool startTop;
    public List<ArmItemData> items;
}

[System.Serializable]
public class ArmItemData{
    public int id;
    public List<int> orientation;
    public List<int> rotationAxis;
    public float length;
    public int parent;
}