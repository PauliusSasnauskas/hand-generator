using System.Collections.Generic;

[System.Serializable]
public class ArmStructureData {
    public bool startTop;
    public List<ArmItemData> items;
}

[System.Serializable]
public class ArmItemData{
    public int id;
    public float width;
    public List<int> orientation;
    public string rotationAxis;
    public ArmItemTelescopeData telescope = null;
    public float length;
    public int parent;
    public float mass;
}

[System.Serializable]
public class ArmItemTelescopeData {
    public int id;
    public float width = -1;
    public float mass;
}