using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class CommandRunner
{
    public static List<ArmItem> hingeItems = new List<ArmItem>();

    public static void runCommand(string command)
    {
        Debug.Log("We got following command: " + command.ToString());

        int[] degrees = command.Split(' ').Select(n => Convert.ToInt32(n)).ToArray();
        for(int i=0;i<degrees.Length;i++)
        {
            float d = degrees[i];
            hingeItems[i].targetDegree = d;

        }


    }


}
