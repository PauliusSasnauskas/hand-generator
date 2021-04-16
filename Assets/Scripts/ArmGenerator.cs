using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ArmGenerator : MonoBehaviour
{
    public bool needFixedJoints = true;

    public GameObject armGroup;
    private GameObject armBase;
    private float armWidth = 0.2f;

    private ArmStructure getObjFromFile(string fileName){
        string jsonString = new StreamReader(fileName).ReadToEnd();
        ArmStructure obj = JsonUtility.FromJson<ArmStructure>(jsonString);
        return obj;
    }

    private GameObject createAndOrientPart(ArmItem item, ref Vector3 currentPosition){
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

        return part;
    }

    private void addRigidBody(GameObject part){
        Rigidbody rb = part.AddComponent<Rigidbody>();
        // rb.mass = 5;
        rb.useGravity = false;
    }

    private List<HingeJoint> hingeJoints = new List<HingeJoint>();
    private List<FixedJoint> fixedJoints = new List<FixedJoint>();

    private void addHingeJoint(GameObject partFrom, Vector3 jointPosition, GameObject partTo, List<int> rotationAxis){
        if (rotationAxis == null || rotationAxis.Count <= 0){
            FixedJoint fj = partFrom.AddComponent<FixedJoint>();
            fj.connectedBody = partTo.GetComponent<Rigidbody>();
            return;
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

        hingeJoints.Add(hj);
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

    private void generateHandFromObject(ArmStructure obj){
        Vector3 currentPosition; // = Vector3.zero;
        GameObject oldPart; // = armBase;

        Dictionary<int, GameObject> parts = new Dictionary<int, GameObject>();
        Dictionary<int, GameObject> partSpheres = new Dictionary<int, GameObject>();
        Dictionary<int, Vector3> partEndPositions = new Dictionary<int, Vector3>();

        foreach (ArmItem item in obj.items){
            if (item.parent == -1){
                currentPosition = Vector3.zero;
            }else{
                currentPosition = partEndPositions[(int)item.parent];
            }

            GameObject part = createAndOrientPart(item, ref currentPosition);

            addRigidBody(part);
            addSphereToPart(part, currentPosition);
            
            parts[item.id] = part;
            partEndPositions[item.id] = currentPosition;


            if (item.parent == -1){
                oldPart = armBase;
            }else{
                oldPart = parts[(int)item.parent];
            }
            addHingeJoint(oldPart, currentPosition, part, item.rotationAxis);

            // oldPart = part;
        }
    }

    void Start()
    {
        armBase = armGroup.transform.Find("ArmBase").gameObject;

        string fileName = "Assets/Scripts/hand_gen2.json";

        ArmStructure obj = getObjFromFile(fileName);

        generateHandFromObject(obj);
    }

    public int turnVelocity = 30;
    public  int turnForce = 100;

    private int selectedPart = 0;

    private void updateSelectedJoint(int amount){
        var part = hingeJoints[selectedPart].gameObject;
        var part2 = hingeJoints[selectedPart].connectedBody.gameObject;

        part.GetComponent<Renderer>().material.color = Color.white;
        part2.GetComponent<Renderer>().material.color = Color.white;

        selectedPart += amount;
        if (selectedPart < 0){ selectedPart = hingeJoints.Count - 1; }
        selectedPart %= hingeJoints.Count;

        
        part = hingeJoints[selectedPart].gameObject;
        part2 = hingeJoints[selectedPart].connectedBody.gameObject;
        
        part.GetComponent<Renderer>().material.color = Color.blue;
        part2.GetComponent<Renderer>().material.color = Color.blue;
            
    }



    void Update() {
        var hj = hingeJoints[selectedPart];
        var motor = hj.motor;

        if (Input.GetKeyDown("up") || Input.GetKeyDown("down")){
            hj.useMotor = true;
            if (Input.GetKeyDown("up")){
                motor.targetVelocity = turnVelocity;
            }else{
                motor.targetVelocity = -turnVelocity;
            }
            motor.force = turnForce;
            hj.motor = motor;
        }

        if (Input.GetKeyUp("up") || Input.GetKeyUp("down")){
            motor.targetVelocity = 0;
            // motor.force = 0;
            hj.motor = motor;
            // hj.useMotor = false;
        }

        if (Input.GetKeyDown("right")){
            updateSelectedJoint(1);
        }
        if (Input.GetKeyDown("left")){            
            updateSelectedJoint(-1);
        }
    }


}

