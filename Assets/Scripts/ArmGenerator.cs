using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ArmGenerator : MonoBehaviour
{
    public GameObject armGroup;
    private GameObject armBase;
    private float armWidth = 0.2f;
    public int turnForce = 100;

    private ArmStructureData getObjFromFile(string fileName){
        string jsonString = new StreamReader(fileName).ReadToEnd();
        ArmStructureData obj = JsonUtility.FromJson<ArmStructureData>(jsonString);
        return obj;
    }

    private GameObject createAndOrientPart(ArmItemData item, ref Vector3 currentPosition){
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        part.transform.localScale = new Vector3(armWidth, item.length/10f, armWidth);
        part.transform.rotation = Quaternion.LookRotation(
            new Vector3(item.orientation[0], item.orientation[1], item.orientation[2]),
            Vector3.up
        );
        part.transform.parent = armGroup.transform;
        part.transform.Rotate(90, 0, 0); // Cylinders "look" from their sides, not their tops, rotate it, so they "look" with their tops

        part.transform.position = currentPosition +                 // Take old part top end position, add it's
            part.transform.rotation *                               //   rotated height to the position so it looks
            Vector3.Scale(part.transform.localScale, Vector3.up);   //   as if it's coming out from the old part

        currentPosition = part.transform.position +                 // Take current part top position, add it's
            part.transform.rotation *                               //   rotated height to the position so the currentPosition
            Vector3.Scale(part.transform.localScale, Vector3.up);   //   variable has the last part's top position


        // resize Arm so parts don't get stuck in each other    
        part.transform.localScale = new Vector3(armWidth, item.length/10f - armWidth/2, armWidth);

        addRigidBody(part);

        return part;
    }

    private void addRigidBody(GameObject part){
        Rigidbody rb = part.AddComponent<Rigidbody>();
        // rb.mass = 5;
        rb.useGravity = false;
    }

    private Joint addHingeJoint(GameObject partFrom, Vector3 jointPosition, GameObject partTo, List<int> rotationAxis){
        if (rotationAxis == null || rotationAxis.Count <= 0){
            FixedJoint fj = partFrom.AddComponent<FixedJoint>();
            fj.connectedBody = partTo.GetComponent<Rigidbody>();
            return fj;
        }
        HingeJoint hj = partFrom.AddComponent<HingeJoint>();
        
        hj.connectedBody = partTo.GetComponent<Rigidbody>();
        hj.anchor = Vector3.up;
        if (partFrom == armBase){
            hj.anchor /= 2;
        }
        Vector3 partAngles = Quaternion.Euler(90, 0, 0) * partFrom.transform.rotation.eulerAngles;
        hj.axis = partFrom.transform.rotation * Quaternion.Euler(-partAngles) * new Vector3(rotationAxis[0], rotationAxis[1], rotationAxis[2]);
        
        var motor = hj.motor;
        motor.force = turnForce;
        hj.motor = motor;

        hj.useMotor = true;

        return hj;
    }

    private void addSphereToPart(GameObject part, Vector3 currentPosition){
        // Add a sphere to make it more nice looking
        GameObject partSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        partSphere.transform.localScale = new Vector3(armWidth, armWidth, armWidth);
        partSphere.transform.position = currentPosition;
        partSphere.transform.parent = armGroup.transform; // part.transform;
        Destroy(partSphere.GetComponent<SphereCollider>());

        addRigidBody(partSphere);

        FixedJoint fj = partSphere.AddComponent<FixedJoint>();
        fj.connectedBody = part.GetComponent<Rigidbody>();
    }


    private ArmStructure generateHandFromObject(ArmStructureData obj){

        ArmStructure armStructure = new ArmStructure(armGroup, armBase);
        
        Dictionary<int, GameObject> parts = new Dictionary<int, GameObject>();
        Dictionary<int, Vector3> partEndPositions = new Dictionary<int, Vector3>();

        foreach (ArmItemData item in obj.items){

            Vector3 currentPosition = (item.parent <= -1 ? Vector3.zero : partEndPositions[(int)item.parent]);

            GameObject part = createAndOrientPart(item, ref currentPosition);
            addSphereToPart(part, currentPosition);
            
            parts[item.id] = part;
            partEndPositions[item.id] = currentPosition;


            GameObject oldPart = (item.parent <= -1 ? armBase : parts[item.parent]);
            Joint j = addHingeJoint(oldPart, currentPosition, part, item.rotationAxis);
            
            ArmItem a = new ArmItem(j);
            armStructure.items.Add(a);
        }

        return armStructure;
    }

    private ArmStructure arm;
    void Start()
    {
        armBase = armGroup.transform.Find("ArmBase").gameObject;

        string fileName = "Assets/Scripts/hand_gen2.json";

        ArmStructureData obj = getObjFromFile(fileName);

        arm = generateHandFromObject(obj);

        // TODO, DELETE THIS, MAKE CLASSES NON STATIC
        foreach(ArmItem j in arm.items)
        {
            if (j.IsTurnable())
            {
                CommandRunner.hingeItems.Add(j);
            }
        }
    }

    public ArmStructure GetArm(){
        return arm;
    }



}

