using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObject : MonoBehaviour
{
    public GameObject objectToPickUp;
    public GameObject PickedObject;
    public Transform interactionZone;

    public CharacterController player; //El Heroe o jugador

   
    void Start()
    {
        /*Obtiene la referencia al jugador o Heroe de forma directa ya que es el script del player*/
        player = GetComponent<CharacterController>(); 
        interactionZone =GameObject.Find("interactionZone").transform; //Aqui se ubicará el objeto
        //interactionZone =GameObject.Find("Mano").transform; //Se puede cambiar a voluntad el lugar donde se ubicará
    }



    private void Update() {

        //Vector3 localScale;
        //Vector3 localPosition;
        if(objectToPickUp != null && objectToPickUp.GetComponent<PickableObject>().isPickable == true && PickedObject == null){
            if(Input.GetKeyDown(KeyCode.F)){
                PickedObject = objectToPickUp;
                PickedObject.GetComponent<PickableObject>().isPickable = false;
                PickedObject.transform.SetParent(interactionZone);
                //localScale = player.transform.localScale;//nuevo
                PickedObject.transform.position = interactionZone.position;
                //localPosition = PickedObject.transform.position;//nuevo
                //localPosition.x += localScale.x;//nuevo
                //PickedObject.transform.localPosition = localPosition;//nuevo
                PickedObject.GetComponent<Rigidbody>().useGravity = false;
                PickedObject.GetComponent<Rigidbody>().isKinematic = true;

            }
        } else {
            if(PickedObject != null){
                if(Input.GetKeyDown(KeyCode.F)){
                    //PickedObject = objectToPickUp;
                    PickedObject.GetComponent<PickableObject>().isPickable = true;
                    PickedObject.transform.SetParent(null);
                    //PickedObject.transform.position = interactionZone.position;
                    PickedObject.GetComponent<Rigidbody>().useGravity = true;
                    PickedObject.GetComponent<Rigidbody>().isKinematic = false;
                    PickedObject = null;

                }
            }
        }
    }
}
