using System.Collections.Generic;

[System.Serializable]
public class ArmStructure {
    public bool startTop;
    public List<ArmItem> items;
}

[System.Serializable]
public class ArmItem{
    public int id;
    public List<int> orientation;
    public List<int> rotationAxis;
    public float length;
    public int parent;
}