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
            ArmGenerator.GetArm().SetTarget(new Vector3((float)position[0]/2f, (float)position[1]/2f, -(float)position[2]/2f));
            return;
        }

        var degrees = command.Split(' ').Select(n => Convert.ToDouble(n)).ToArray();
        for (int i=0; i < degrees.Length; i++) {
            if (i == 1 || i == 3){
                degrees[i] *= -1;
            }
            hingeItems[i].targetDegree = -(float)degrees[i];
        }
    }
}
