using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyImpactSound : MonoBehaviour {

    public AudioCollection impactSound;

    /*----Liste des layers par nombres
     * wood: 1
     * metal: 2
     * water: 3
     * concrete: 4
     * ground: 5
     * tapis: 6
     * 
     * */

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.relativeVelocity.magnitude > 2)
            AudioManager.instance.PlayOneShotSound(impactSound.audioGroup, impactSound[0], transform.position, impactSound.volume, impactSound.spatialBlend, impactSound.priority);
    }

}
