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

    public ArmItem(Joint j){
        this.j = j;
    }

    public bool IsTurnable(){
        return j is HingeJoint;
    }

    public GameObject GetPartFrom(){
        return j.gameObject;
    }

    public GameObject GetPartTo(){
        return j.connectedBody.gameObject;
    }

    public float GetAngle(){
        if (j is HingeJoint hj){
            return hj.angle;
        }else{
            return -1000f;
        }
    }

    public void SetTurnVelocity(float turnVelocity){
        if (j is HingeJoint hj){
            var motor = hj.motor;
            motor.targetVelocity = turnVelocity;
            hj.motor = motor;
        }
    }

    public void SetColor(Color c){
        j.gameObject.GetComponent<Renderer>().material.color = c;
        j.connectedBody.gameObject.GetComponent<Renderer>().material.color = c;
    }
}