using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class ArmKeyboardControl : MonoBehaviour
{
    public ArmGenerator armGenerator;
    private int selectedPart;
    private List<ArmItem> turnableItems = new List<ArmItem>();
    IEnumerator Start(){
        if (armGenerator.GetArm() == null){ yield return null; } // Waiting for arm to load

        ArmStructure arm = armGenerator.GetArm();

        for (int i = 0; i < arm.items.Count; i++){
            ArmItem item = arm.items[i];
            if (!item.IsTurnable()){ continue; }
            turnableItems.Add(item);
        }

        updateSelectedJoint();
    }

    public int turnVelocity = 30;

    private void updateSelectedJoint(int amount = 0){
        ArmItem turnItem = turnableItems[selectedPart];
        turnItem.SetColor(Color.white);

        selectedPart += amount;
        if (selectedPart < 0){ selectedPart = turnableItems.Count - 1; }
        selectedPart %= turnableItems.Count;

        turnItem = turnableItems[selectedPart];
        turnItem.SetColor(Color.blue);
    }

    void Update() {
        if (turnableItems == null || turnableItems.Count <= 0) { return; }

        ArmItem turnItem = turnableItems[selectedPart];

        if (Input.GetKeyDown("up") || Input.GetKeyDown("down")){
            if (Input.GetKeyDown("up")){
                turnItem.SetTurnVelocity(turnVelocity);
            }else{
                turnItem.SetTurnVelocity(-turnVelocity);
            }

            print(turnItem.GetAngle());
        }

        if (Input.GetKeyUp("up") || Input.GetKeyUp("down")){
            turnItem.SetTurnVelocity(0);
        }

        if (Input.GetKeyDown("right")){
            updateSelectedJoint(1);
        }
        if (Input.GetKeyDown("left")){            
            updateSelectedJoint(-1);
        }
    }
}