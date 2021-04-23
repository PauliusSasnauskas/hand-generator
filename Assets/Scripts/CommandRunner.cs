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

        int[] velocities = command.Split(' ').Select(n => Convert.ToInt32(n)).ToArray();
        for(int i=0;i<velocities.Length;i++)
        {
            Debug.Log("i = " + i.ToString());

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                hingeItems[i].JustMove();
            });
            
        }


    }


}
