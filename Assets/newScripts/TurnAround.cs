using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAround : MonoBehaviour {

    float turnSpeed = 1.0f;

    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, turnSpeed, 0));
    }
}
