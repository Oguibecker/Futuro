using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    private CharacterController characterController;
    [SerializeField]
    private Vector3 _velocity;

    public Vector3 Velocity 
    { 
        get {  return _velocity; }
    }

    [SerializeField]
    private Vector3 move;

    [SerializeField]
    private float speed = 8f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Move()
    {
        move = new Vector3(0, 0, 1);
        characterController.Move(move * Time.deltaTime * speed);
    }
}
