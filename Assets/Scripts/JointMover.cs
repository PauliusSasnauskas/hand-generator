using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMover : MonoBehaviour
{

    public List<ArmItem> hingeItems = new List<ArmItem>();
    public float EPLSILON = 1.5f;

    public float SPEED = 5;

    private int cnt = 25;

    void FixedUpdate()
    {
        string msg = "Current degrees: ";
        string target = "Target degrees: ";
        string velocities = "Velocities :";
        
        foreach(ArmItem j in hingeItems)
        {

            msg += j.GetAngle().ToString() + " ";
            target += j.targetDegree.ToString() + " ";
            velocities += j.GetTurnVelocity() + " ";

            if(j.targetDegree - j.GetAngle() > EPLSILON)
            {
                j.Move(-SPEED);
            }
            else if(j.targetDegree - j.GetAngle() < -EPLSILON)
            {
                j.Move(SPEED);
            }
            else
            {
                j.Move(0.0f);
            }

        }
        cnt--;
        if(cnt == 0)
        {
            Debug.Log(msg);
            Debug.Log(target);
            Debug.Log(velocities);
            cnt = 100;
        }
        
    }
}
