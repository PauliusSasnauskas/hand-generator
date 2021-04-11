using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ArmGenerator : MonoBehaviour
{
    public GameObject armGroup;

    private ArmStructure getObjFromFile(string fileName){
        string jsonString = new StreamReader(fileName).ReadToEnd();
        ArmStructure obj = JsonUtility.FromJson<ArmStructure>(jsonString);
        return obj;
    }

    private void generateHandFromObject(ArmStructure obj){
        Vector3 oldPosition = Vector3.zero;

        foreach (ArmItem item in obj.items){
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.transform.localScale = new Vector3(0.2f, item.length/10f, 0.2f);
            part.transform.rotation = Quaternion.LookRotation(
                new Vector3(item.orientation[0], item.orientation[1], item.orientation[2]),
                Vector3.up
            );
            part.transform.parent = armGroup.transform;
            part.transform.Rotate(90, 0, 0); // Cylinders "look" from their sides, not their tops, rotate it, so they "look" with their tops

            part.transform.position = oldPosition +                     // Take old part top end position, add it's
                part.transform.rotation *                               //   rotated height to the position so it looks
                Vector3.Scale(part.transform.localScale, Vector3.up);   //   as if it's coming out from the old part

            oldPosition = part.transform.position +                     // Take current part top position, add it's
                part.transform.rotation *                               //   rotated height to the position so the oldPosition
                Vector3.Scale(part.transform.localScale, Vector3.up);   //   variable has the last part's top position
            
            GameObject partEnd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            partEnd.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            partEnd.transform.position = oldPosition;
            partEnd.transform.parent = part.transform;
        }
    }

    void Start()
    {
        string fileName = "Assets/Scripts/hand_gen1.json";

        ArmStructure obj = getObjFromFile(fileName);

        generateHandFromObject(obj);
    }
}

