using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ArmGenerator : MonoBehaviour
{
    public GameObject armGroup;
    private GameObject armBase;
    public int turnForce = 100;

    private ArmStructureData getObjFromFile(string fileName){
        string jsonString = new StreamReader(fileName).ReadToEnd();
        ArmStructureData obj = JsonUtility.FromJson<ArmStructureData>(jsonString);
        return obj;
    }

    private GameObject createAndOrientPart(ArmItemData item, ref Vector3 currentPosition){
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        part.transform.localScale = new Vector3(item.width/10f, item.length/10f, item.width/10f);
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
        part.transform.localScale = new Vector3(item.width/10f, item.length/10f - item.width/20f, item.width/10f);

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

    private Joint addSlideJoint(GameObject partFrom, GameObject partTo){
        ConfigurableJoint cj = partFrom.AddComponent<ConfigurableJoint>();
        cj.connectedBody = partTo.GetComponent<Rigidbody>();

        cj.autoConfigureConnectedAnchor = false;

        cj.anchor = Vector3.up;
        cj.connectedAnchor = Vector3.zero;

        cj.xMotion = ConfigurableJointMotion.Locked;
        cj.yMotion = ConfigurableJointMotion.Limited; // Allow Y movement
        cj.zMotion = ConfigurableJointMotion.Locked;
        cj.angularXMotion = ConfigurableJointMotion.Locked;
        cj.angularYMotion = ConfigurableJointMotion.Locked;
        cj.angularZMotion = ConfigurableJointMotion.Locked;

        var limit = cj.linearLimit;
        limit.limit = partFrom.transform.localScale.y;
        cj.linearLimit = limit;

        var drive = cj.yDrive;
        drive.positionSpring = 1000;
        cj.yDrive = drive;

        cj.targetVelocity = new Vector3(0, 100, 0);
        cj.targetPosition = new Vector3(0, -partFrom.transform.localScale.y, 0);

        return cj;
    }

    private void addSphereToPart(GameObject part, Vector3 currentPosition, float width){
        // Add a sphere to make it more nice looking
        GameObject partSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        partSphere.transform.localScale = new Vector3(width, width, width);
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
            // Take last position to start from
            Vector3 currentPosition = (item.parent <= -1 ? Vector3.zero : partEndPositions[(int)item.parent]);

            // Create the part
            GameObject part = createAndOrientPart(item, ref currentPosition);
            
            parts[item.id] = part;
            partEndPositions[item.id] = currentPosition;

            // Find old part to connect to
            GameObject oldPart = (item.parent <= -1 ? armBase : parts[item.parent]);
            Joint j = addHingeJoint(oldPart, currentPosition, part, item.rotationAxis);
            
            ArmItem a = new ArmItem(j);
            armStructure.items.Add(a);

            if (item.telescope == null || item.telescope.width <= 0){
                addSphereToPart(part, currentPosition, item.width/10f); // Add sphere to end
            } else {
                // Add telescope if there is one
                GameObject telescopePart = GameObject.Instantiate(part);
                telescopePart.transform.localScale = new Vector3(item.telescope.width/10f, part.transform.localScale.y, item.telescope.width/10f);
                telescopePart.transform.parent = armGroup.transform;
                addSphereToPart(telescopePart, currentPosition, item.width/10f);

                Joint jt = addSlideJoint(part, telescopePart);
                ArmItem at = new ArmItem(jt);
                armStructure.items.Add(at);

                parts[item.telescope.id] = telescopePart;
                partEndPositions[item.telescope.id] = currentPosition;
            }
        }

        return armStructure;
    }

    private ArmStructure arm;

    void Start()
    {
        armBase = armGroup.transform.Find("ArmBase").gameObject;

        string fileName = "Assets/Scripts/hand_gen1.json";

        ArmStructureData obj = getObjFromFile(fileName);

        arm = generateHandFromObject(obj);

        // TODO, DELETE THIS, MAKE CLASSES NON STATIC
        JointMover jointMover = GetComponent<JointMover>();
        foreach (ArmItem j in arm.items)
        {
            if (j.IsTurnable())
            {
                CommandRunner.hingeItems.Add(j);
                jointMover.hingeItems.Add(j);
            }
        }
    }

    public ArmStructure GetArm(){
        return arm;
    }



}

