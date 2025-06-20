using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kino;

public class CutsceneManager : MonoBehaviour
{

    public AnalogGlitch glitchElement;
    public bool glitchState;

    // Start is called before the first frame update
    //void Start()
    //{
        //glitchElement = GetComponent<AnalogGlitch>();
    //}

    // Update is called once per frame
    void Update()
    {
        glitchElement.enabled = glitchState;
    }

}
