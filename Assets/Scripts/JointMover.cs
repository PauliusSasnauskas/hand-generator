using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMover : MonoBehaviour
{

    public List<ArmItem> hingeItems = new List<ArmItem>();
    public float EPLSILON = 1.5f;

    public float SPEED = 5;

    private int cnt = 25;
    public ArmGenerator armGenerator;
    IEnumerator Start(){
        if (ArmGenerator.GetArm() == null){ yield return null; } // Waiting for arm to load

        hingeItems = ArmGenerator.GetArm().GetHingeItems();
    }

    void FixedUpdate(){
        string msg = "Current degrees: ";
        string target = "Target degrees: ";
        string velocities = "Velocities: ";
        
        foreach(ArmItem j in hingeItems) {
            msg += j.GetAngle().ToString() + " ";
            target += j.targetDegree.ToString() + " ";
            velocities += j.GetTurnVelocity() + " ";

            var angleDiff = j.GetAngle() - j.targetDegree;
            var angleDiffOther = 360f - j.GetAngle() + j.targetDegree;

            if (angleDiffOther < angleDiff){
                angleDiff = -angleDiffOther;
            }

            var sendSpeed = SPEED*Mathf.Abs(angleDiff/30);
            if (sendSpeed > SPEED){ sendSpeed = SPEED; }

            
            if(angleDiff > EPLSILON){
                j.SetColor(Color.Lerp(Color.white, Color.red, sendSpeed/SPEED));
                j.Move(-sendSpeed);
            } else if(angleDiff < -EPLSILON){
                j.SetColor(Color.Lerp(Color.white, Color.blue, sendSpeed/SPEED));
                j.Move(sendSpeed);
            } else {
                j.SetColor(Color.white);
                j.Move(0.0f);
            }

        }
        cnt--;
        if(cnt == 0){
            // Debug.Log(msg);
            // Debug.Log(target);
            // Debug.Log(velocities);
            Debug.Log("\n" + msg + "\n" + target + "\n" + velocities);
            cnt = 100;
        }
    }
}
