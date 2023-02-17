using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushRigidBody : MonoBehaviour
{
    public float pushPower = 3.0f;
    private float targetMass;
    
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody body = hit.collider.attachedRigidbody;

        /*Si no tiene Rigibody o Si es inamovible*/
        if(body == null || body.isKinematic){
            return;
        }
        /*Si chocamos con el objeto mientras nos estamos moviendo hacia abajo*/
        if(hit.moveDirection.y < -0.3){
            return;
        }
        targetMass = body.mass;
        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.velocity = pushDirection * pushPower / targetMass;
    }
}
