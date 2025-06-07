using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Checkpoint : MonoBehaviour
{
    public int beforeLevel;
    public int speedBooster = 0;

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
                    player.SpeedUp();
                    speedBooster = 0;
                }
            }
        }
    }
}
