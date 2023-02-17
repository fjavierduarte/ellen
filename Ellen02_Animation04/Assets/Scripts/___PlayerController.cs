using UnityEngine;

public class __PlayerController : MonoBehaviour
{   
    
    public float gravity = 9.8f;
    public float jumpForce = 10; // Esto se pùede modificar en el editor pero en caso de olvido tendrá un valor por defecto  
    public CharacterController player;//El propio jugador
    public float playerSpeed = 3f;//Velocidad del jugador
    public float pushPower = 9.0f;
    public float slideVelocity;
    public float slopeForceDown;
    public float factorDivision  = 4.2f;  
    //public Camera mainCamera;//La camara principal
    //private Vector3 camForward;
    //private Vector3 camRight;
    private GameObject collidedObject; //Guarda el objeto con el que colisiona  
    private float fallVelocity;
    public bool isJumpButton = false;
    public bool isGrounded = false;
    private bool isOnSlope = false;
    private Vector3 hitNormal;
    private int clampMagnitude = 1;
    

  
    void Start()
    {
        player = GetComponent<CharacterController>();
        /* Busca la camara principal con la función GameObject.Find("Main Camera")
        * del este objeto extrae el componente Camera y de esta forma asigna la camara desde codigo
        */ 
        //mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();                   
    }

    // Update is called once per frame. Update es llamada una vez por cada frame
    void Update()
    {       
        Vector3 movePlayer = GetPlayerInput() * playerSpeed; 
        player.transform.LookAt(player.transform.position + movePlayer);
        movePlayer += GetGravity();   
        player.Move(movePlayer * Time.deltaTime);
        
    }

    // Obtiene la entradas del usuario 
    private Vector3 GetPlayerInput(){
        // Detecta si se oprimió el boton de salto
        if(Input.GetButtonDown("Jump")){   
            isJumpButton = true; // Esto se utilizará en GetGravity()
        }
        float xMove = Input.GetAxis("Horizontal");//Entrada del eje x
        float zMove = Input.GetAxis("Vertical");//Entrada del eje z
        Vector3 playerInput = new Vector3(xMove, 0, zMove);//Composición del vector con las entradas del jugador
        playerInput = Vector3.ClampMagnitude(playerInput, clampMagnitude); //Restringe las magnitudes del vector a 1 
        return playerInput; 
    }

    private Vector3 GetGravity(){
        Vector3 playerComponent = new Vector3(0, 0, 0); // Establece un vector.
        isGrounded = player.isGrounded;
        if(player.isGrounded){ // Si está en el suelo
            if(isJumpButton){// Si se apreto el botón de salto y se detectó en GetPlayerInput()
                /*La velocidad de caida es igual al salto, que es positivo por esa razón va hacia arriba*/
                fallVelocity = jumpForce; // El valor se establece en el editor de Unity
                isJumpButton = false;// Vuelve a false y solo una nueva presión del botón puede cambiarla, esto evita la repetición por mantener pulsado el boton
            } else {
                //En este punto, el jugador no saltó y se encuentre en el suelo
                fallVelocity = -gravity * Time.deltaTime; // Se le descuenta a la velocidad de caida la fuerza de gravedad, en todo momento la gravedad tira al jugador hacia en suelo

                /** collidedObject.transform.localScale.y es la altura o eje y del objeto contra el que colisionó
                    Si el objeto con el que colisionó es más alto que el stepOffset
                    aplica la dinamica de deslizamiento desde un plano inclinado
                */
                if(collidedObject.transform.localScale.y > player.stepOffset){ // Si el objeto con el que colisionó es más alto que el stepOffset
                    /*Detecta si esta en una loma mayor a la cuesta que puede subir
                    esto solo lo hace si primero detecto que está en el suelo*/
                    isOnSlope = Vector3.Angle(Vector3.up, hitNormal) > player.slopeLimit;
                    if(isOnSlope){//Si esta en una cuesta o rampa
                        //Modifica los componentes del vector del player
                        playerComponent.x = ((1f - hitNormal.y) * hitNormal.x) * slideVelocity;
                        playerComponent.z = ((1f - hitNormal.y) * hitNormal.z) * slideVelocity;
                        //(1f - hitNormal.y) hace que mientras más empinada la rampa el multiplicador se hacerque más a 1
                        fallVelocity -= slopeForceDown; //Se descuenta tambien la fuerza de la rampa hacia abajo
                        //Ya se habia descontado la fuerza de gravedad, esto hace que caiga más suave por la rampa
                    }    
                }
            }

        } else {
            //si está en el aire
            isJumpButton = false;
            fallVelocity -= gravity * Time.deltaTime;//La velocidad de caida se acelera
        }
        
        playerComponent.y = fallVelocity;
        return playerComponent;
    } 

    /*Funcion de colisión*/

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        hitNormal = hit.normal; //Guarda la normal del objeto con el que colisionó   
        collidedObject = hit.gameObject;//Guarda el objeto actual con el que colisionó

        /*Mueve los objetos que contengan un Rigidbody y que son moviles*/
        Rigidbody body = hit.collider.attachedRigidbody;
        if(body == null || body.isKinematic){
            return;
        }
        /*Detecta si no golpea al objeto desde arriba*/
        if(hit.moveDirection.y < -0.3f){ 
            return;
        }
        float bodytMass = body.mass;//Guarda la masa del objeto
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);//Establece la dirección de empuje
        body.velocity = pushDir * pushPower / bodytMass;/*Multiplica el vector por un escalar que es el
        resultado de la fuesza de empuje dividida por la masa para obtener empujes diferentes dependiendo de su masa*/ 
        

    }
}
