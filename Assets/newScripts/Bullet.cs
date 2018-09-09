using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float bulletSpeed = 500.0f;
    public int predictionStepPerFrame = 6;
    public Vector3 bulletVelocity;

    private void Start()
    {
        bulletVelocity = transform.forward * bulletSpeed;
    }

    private void Update()
    {
        Vector3 point1 = transform.position;
        float stepSize = 1.0f / predictionStepPerFrame;
        for(float step = 0; step < 1; step += stepSize)
        {
            bulletVelocity += Physics.gravity * stepSize * Time.deltaTime;
            Vector3 point2 = point1 + bulletVelocity * stepSize * Time.deltaTime;

            Ray ray = new Ray(point1, point2 - point1);
            if(Physics.Raycast(ray, (point2 - point1).magnitude))
            {
                Debug.Log("hit");
            }

            point1 = point2;
            transform.position = point2;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 point1 = transform.position;
        Vector3 predictedBulletVelocity = bulletVelocity;
        float stepSize = 0.01f;

        for(float step = 0; step<1; step+=stepSize)
        {
            predictedBulletVelocity += Physics.gravity * stepSize;
            Vector3 point2 = point1 + predictedBulletVelocity * stepSize;
            Gizmos.DrawLine(point1, point2);
            point1 = point2;
        }
    }
}
