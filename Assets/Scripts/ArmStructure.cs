using System.Collections.Generic;
using UnityEngine;
public class ArmStructure {
    public GameObject armGroup;
    public GameObject armBase;
    public List<ArmItem> items = new List<ArmItem>();

    public ArmStructure(GameObject armGroup, GameObject armBase){
        this.armGroup = armGroup;
        this.armBase = armBase;
    }
}

public class ArmItem {
    private Joint j;
    private GameObject angleCompare;
    private GameObject jointSphere;

    public float targetDegree = 0;

    public ArmItem(Joint j, GameObject angleCompare = null){
        this.j = j;
        this.angleCompare = angleCompare;
    }

    public void SetSphere(GameObject jointSphere){
        this.jointSphere = jointSphere;
    }

    public bool IsMovable(){
        return j is HingeJoint || j is ConfigurableJoint;
    }

    public GameObject GetPartFrom(){
        return j.gameObject;
    }

    public GameObject GetPartTo(){
        return j.connectedBody.gameObject;
    }

    public float GetAngle(){
        if (j is HingeJoint hj){
            var plane1 = this.angleCompare.transform.position;
            var plane2 = this.GetPartFrom().transform.position;
            var plane3 = plane2 + this.GetPartFrom().transform.up;

            var planeX = this.GetPartTo().transform.position;

            plane1 = plane1 - plane3;
            plane2 = plane2 - plane3;
            planeX = planeX - plane3;

            var a = plane1.x; var b = plane1.y; var c = plane1.z;
            var d = plane2.x; var e = plane2.y; var f = plane2.z;
            var g = planeX.x; var h = planeX.y; var i = planeX.z;
            
            var side = a*(e*i-f*h) - b*(d*i-f*g) + c*(d*h-e*g);


            Quaternion rot1 = this.angleCompare.transform.rotation;
            Quaternion rot2 = this.GetPartTo().transform.rotation;

            if (side > 0){ return -Quaternion.Angle(rot1,rot2); }
            return Quaternion.Angle(rot1,rot2);
        }else{
            return -1000f;
        }
    }

    public void Move(float amount){
        if (j is HingeJoint hj){
            var motor = hj.motor;
            motor.targetVelocity = amount;
            hj.motor = motor;
            return;
        }
        if (j is ConfigurableJoint cj){
            var targetPos = cj.targetPosition;
            targetPos.y += amount / 10000f;
            if (targetPos.y > cj.linearLimit.limit){ targetPos.y = cj.linearLimit.limit; }
            if (targetPos.y < -cj.linearLimit.limit){ targetPos.y = -cj.linearLimit.limit; }
            cj.targetPosition = targetPos;
        }
    }

    public bool IsTurnable(){
        return j is HingeJoint;
    }
    public float GetTurnVelocity(){
        if (j is HingeJoint hj){
            var motor = hj.motor;
            return motor.targetVelocity;
        }
        return 0f;
    }
    public void SetColor(Color c){
        j.gameObject.GetComponent<Renderer>().material.color = c;
        j.connectedBody.gameObject.GetComponent<Renderer>().material.color = c;
    }

}