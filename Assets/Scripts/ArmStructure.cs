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

    public float targetDegree = 0;

    public ArmItem(Joint j){
        this.j = j;
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
            
            Quaternion a = this.GetPartFrom().transform.rotation;
            Quaternion b = this.GetPartTo().transform.rotation;

            return Quaternion.Angle(a,b);

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