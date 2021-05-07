using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMover : MonoBehaviour
{

    public List<ArmItem> hingeItems = new List<ArmItem>();
    public float EPLSILON = 1.5f;

    public const float SPEED = 10;

    private int cnt = 25;
    public ArmGenerator armGenerator;
    IEnumerator Start(){
        if (ArmGenerator.GetArm() == null){ yield return null; } // Waiting for arm to load

        hingeItems = ArmGenerator.GetArm().GetHingeItems();
    }

    private static float PickValid180(params float[] values){
        float best = values[0];
        foreach (float val in values){
            if (val > 180f || val < -180f){ continue; }
            best = val;
        }
        return best;
    }

    void FixedUpdate(){
        string msg = "Current degrees: ";
        string target = "Target degrees: ";
        string velocities = "Velocities: ";
        
        int i = 0;

        foreach(ArmItem j in hingeItems) {
            msg += j.GetAngle().ToString() + " ";
            target += j.targetDegree.ToString() + " ";
            velocities += j.GetTurnVelocity() + " ";

            var angleDiff0 = j.GetAngle() - j.targetDegree;
            var angleDiff1 = angleDiff0 - 360f;
            var angleDiff2 = angleDiff0 + 360f;

            var angleDiff = PickValid180(angleDiff0, angleDiff1, angleDiff2);

            var maxSpeed = SPEED;

            var sendSpeed = maxSpeed*Mathf.Abs(angleDiff/10);
            if (sendSpeed > maxSpeed){ sendSpeed = maxSpeed; }
            
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
            i++;
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
