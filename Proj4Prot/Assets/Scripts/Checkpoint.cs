using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Checkpoint : MonoBehaviour
{
    public int beforeLevel;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetCheckpoint(transform.position);
                player.laneMovementEnabled = beforeLevel;
                player.currentLane = 1;
            }
        }
    }
}
