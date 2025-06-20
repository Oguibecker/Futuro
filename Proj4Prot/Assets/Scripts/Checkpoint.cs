using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int beforeLevel;
    public int speedBooster = 0;

    public GameObject mirrorArches;

    public enum Gimmick
    {
        None,
        FirstPerson,
        TurnCam180,
        TopDown,
        Mirrors,
        Depleted
    }

    public Gimmick chosenGimmick;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.lastCheckpoint = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
                player.laneMovementEnabled = beforeLevel;
                if (beforeLevel == 0) {player.currentLane = 1;};
                StartCoroutine(player.passedCheckpoint(beforeLevel));
                if (speedBooster == 1){ //speeds up player if option is enabled, and removes it from instance as to not speed up on respawn.
                    //player.SpeedUp();
                    speedBooster = 0;
                }
                
                if (chosenGimmick == Gimmick.None)
                {
                    if (player.gimmickStateTD == true) {player.camTopDown(false); player.gimmickStateTD = false;}
                    if (player.gimmickState180 == true) {player.camTurn180(false); player.gimmickState180 = false;}
                    player.reversedKeys = false;
                }
                else if (chosenGimmick == Gimmick.FirstPerson)  {player.firstPerson();}
                else if (chosenGimmick == Gimmick.TurnCam180)   {player.camTurn180(true); player.gimmickState180 = true;}
                else if (chosenGimmick == Gimmick.TopDown)
                {
                    player.camTopDown(true);
                    player.gimmickStateTD = true;
                } else if (chosenGimmick == Gimmick.Mirrors)
                {
                    mirrorArches.transform.localScale = new Vector3(1f,1f,1f);
                    player.reversedKeys = true;
                }

                chosenGimmick = Gimmick.Depleted;


            }
        }
    }
}
