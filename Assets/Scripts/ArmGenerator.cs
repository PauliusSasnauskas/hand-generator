using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class ArmGenerator : MonoBehaviour
{
    public bool HELPERS_VISIBLE = false;
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
        part.name = "Solid Limb";
        part.transform.localScale = new Vector3(item.width, (item.length - item.width)/2f, item.width);
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


        // resize part so parts don't get stuck in each other
        part.transform.localScale = new Vector3(item.width, (item.length - item.width)/2f, item.width);


        addRigidBody(part);

        return part;
    }

    private void addRigidBody(GameObject part, float mass = 1){
        Rigidbody rb = part.GetComponent<Rigidbody>();
        if (rb == null){
            rb = part.AddComponent<Rigidbody>();
        }
        rb.mass = mass;
        rb.useGravity = false;
    }

    private Joint addHingeJoint(GameObject partFrom, Vector3 jointPosition, GameObject partTo, string rotationAxis, List<int> orientation){
        if (rotationAxis == null || rotationAxis.Length <= 0){
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
        Vector3 rotation = rotationAxis switch {
            "x" => Vector3.forward,
            "y" => Vector3.down,
            "z" => Vector3.forward,
             _  => Vector3.zero
        };
        hj.axis = partFrom.transform.rotation * rotation;
        
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

    private GameObject addSphereToPart(GameObject part, Vector3 currentPosition, float width){
        // Add a sphere to make it more nice looking
        GameObject partSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        partSphere.name = "Joint Sphere";
        partSphere.transform.localScale = new Vector3(width, width, width);
        partSphere.transform.position = currentPosition;
        partSphere.transform.parent = armGroup.transform; // part.transform;
        Destroy(partSphere.GetComponent<SphereCollider>());

        addRigidBody(partSphere);

        FixedJoint fj = partSphere.AddComponent<FixedJoint>();
        fj.connectedBody = part.GetComponent<Rigidbody>();

        return partSphere;
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

            // Create part for visuals
            GameObject partVisual = GameObject.Instantiate(part);
            partVisual.name = "Visual Limb";
            partVisual.transform.parent = armGroup.transform;
            Destroy(partVisual.GetComponent<CapsuleCollider>());
            addRigidBody(partVisual, 0);
            var fjVis = partVisual.AddComponent<FixedJoint>();
            fjVis.connectedBody = part.GetComponent<Rigidbody>();
            
            
            parts[item.id] = part;
            partEndPositions[item.id] = currentPosition;

            // Find old part to connect to
            GameObject oldPart = (item.parent <= -1 ? armBase : parts[item.parent]);

            // TODO: fix
            // Create relative part for angle checking
            GameObject partRest = GameObject.Instantiate(part);
            partRest.name = "Invisible Helper Limb";
            partRest.transform.parent = armGroup.transform;
            addRigidBody(partRest, 0);
            var fj = oldPart.AddComponent<FixedJoint>();
            fj.connectedBody = partRest.GetComponent<Rigidbody>();
            if (HELPERS_VISIBLE){
                var newcolor = new Color(0.5f, 0, 0, 0.5f);
                partRest.GetComponent<Renderer>().material.color = newcolor;
            }else{
                Destroy(partRest.GetComponent<Renderer>());
            }
            Destroy(partRest.GetComponent<CapsuleCollider>());


            // Connect to old part
            Joint j = addHingeJoint(oldPart, currentPosition, part, item.rotationAxis, item.orientation);

            // Modify scale so parts don't get stuck in each other
            var scaleMod = part.transform.localScale;
            scaleMod.y -= item.width / 2f;
            if (item.parent == -1){
                scaleMod.y += item.width / 4f;
            }
            part.transform.localScale = scaleMod;

            ArmItem a = new ArmItem(j, partVisual, partRest);
            armStructure.items.Add(a);

            if (item.telescope == null || item.telescope.width <= 0){
                GameObject sphere = addSphereToPart(part, currentPosition, item.width); // Add sphere to end
            } else {
                // Add telescope if there is one
                GameObject telescopePart = GameObject.Instantiate(part);
                telescopePart.name = "Solid Limb Telescope";
                telescopePart.transform.localScale = new Vector3(item.telescope.width, part.transform.localScale.y, item.telescope.width);
                telescopePart.transform.parent = armGroup.transform;
                GameObject sphere = addSphereToPart(telescopePart, currentPosition, item.width);

                Joint jt = addSlideJoint(part, telescopePart);
                ArmItem at = new ArmItem(jt, telescopePart);
                armStructure.items.Add(at);

                parts[item.telescope.id] = telescopePart;
                partEndPositions[item.telescope.id] = currentPosition;
            }
            Destroy(part.GetComponent<Renderer>());
        }

        return armStructure;
    }

    private static ArmStructure arm = null;

    void Start()
    {
        armBase = armGroup.transform.Find("ArmBase").gameObject;

        string fileName = "Assets/Scripts/hand_gen2.json";

        ArmStructureData obj = getObjFromFile(fileName);

        arm = generateHandFromObject(obj);

        // TODO: fix
        arm.GetHingeItems()[0].targetDegree = -43f;
        arm.GetHingeItems()[1].targetDegree = 25f;
        arm.GetHingeItems()[2].targetDegree = -20f;
        arm.GetHingeItems()[3].targetDegree = 80f;
        arm.GetHingeItems()[4].targetDegree = 50f;
    }

    public static ArmStructure GetArm(){
        return arm;
    }
}