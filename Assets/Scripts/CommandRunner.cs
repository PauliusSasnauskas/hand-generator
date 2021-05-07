using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class CommandRunner
{
    public static List<ArmItem> hingeItems;
    public static void runCommand(string command) {
        if (hingeItems == null){
            hingeItems = ArmGenerator.GetArm().GetHingeItems();
        }

        Debug.Log("We got command: " + command.ToString());
        if (command.StartsWith("target ")){
            var position = command.Substring(7).Split(' ').Select(n => Convert.ToDouble(n)).ToArray();
            ArmGenerator.GetArm().SetTarget(new Vector3((float)position[0], (float)position[1], -(float)position[2]));
            return;
        }

        var degrees = command.Split(' ').Select(n => Convert.ToDouble(n)).ToArray();
        for (int i=0; i < degrees.Length; i++) {

            if (i == 1 || i == 3){ degrees[i] *= -1; } // Hack to work with current configuration

            if (degrees[i] > 180f){ degrees[i] -= 360f; }
            if (degrees[i] < -180f){ degrees[i] += 360f; }

            hingeItems[i].targetDegree = -(float)degrees[i];
        }
    }
}
