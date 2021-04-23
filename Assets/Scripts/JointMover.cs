using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMover : MonoBehaviour
{

    public List<ArmItem> hingeItems = new List<ArmItem>();

    public float EPLSILON = 10f;

    void Update()
    {
        foreach(ArmItem j in hingeItems)
        {
            if(j.targetDegree - j.GetAngle() > EPLSILON)
            {
                j.SetTurnVelocity(10);
            }
            else if(j.targetDegree - j.GetAngle() < -EPLSILON)
            {
                j.SetTurnVelocity(-10);
            }
            else
            {
                j.SetTurnVelocity(0);
            }

        }
    }
}
