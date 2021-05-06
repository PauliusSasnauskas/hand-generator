using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class ArmKeyboardControl : MonoBehaviour
{
    public ArmGenerator armGenerator;
    private int selectedPart;
    private List<ArmItem> movableItems = new List<ArmItem>();
    IEnumerator Start(){
        if (ArmGenerator.GetArm() == null){ yield return null; } // Waiting for arm to load

        // movableItems = ArmGenerator.GetArm().GetHingeItems();
        movableItems = ArmGenerator.GetArm().GetMovableItems();

        updateSelectedJoint();
    }

    public int moveVelocity = 30;

    private void updateSelectedJoint(int amount = 0){
        ArmItem moveItem = movableItems[selectedPart];
        moveItem.SetColor(Color.white);

        selectedPart += amount;
        if (selectedPart < 0){ selectedPart = movableItems.Count - 1; }
        selectedPart %= movableItems.Count;

        moveItem = movableItems[selectedPart];
        moveItem.SetColor(Color.blue);
    }

    void Update() {
        if (movableItems == null || movableItems.Count <= 0) { return; }

        ArmItem moveItem = movableItems[selectedPart];

        if (Input.GetKey("up") || Input.GetKey("down")){
            if (Input.GetKey("up")){
                moveItem.Move(moveVelocity);
            }else{
                moveItem.Move(-moveVelocity);
            }

            print("Angle: " + moveItem.GetAngle().ToString());
        }

        if (Input.GetKeyUp("up") || Input.GetKeyUp("down")){
            moveItem.Move(0);
        }

        if (Input.GetKeyDown("right")){
            updateSelectedJoint(1);
        }
        if (Input.GetKeyDown("left")){            
            updateSelectedJoint(-1);
        }
    }
}