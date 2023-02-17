using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* En el juego debe haber un GameObject llamado "Main Camera" con un componente llamado Camera*/ 
public class PlayerController : MonoBehaviour
{
    public CharacterController player; //El Heroe o jugador
    public Camera mainCamera;//La camara principal
    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;
    public float playerSpeed = 3f;    
    public float gravity = 9.8f;
    public float fallVelocity = 0;
    public float jumpForce = 6f;
    public float slideVelocity = 2f;
    public float slopeForceDown = 30f;
    public bool isPlayerGrounded = false;
    public bool isOnSlope = false;//Si está en una rampa
    private Vector3 hitNormal;

    public Animator playerAnimatorController;




    
    // Start is called before the first frame update
    void Start()
    {
        /*Obtiene la referencia al jugador o Heroe de forma directa ya que es el script del player*/
        player = GetComponent<CharacterController>();
        playerAnimatorController = GetComponent<Animator>();
        /* Busca la camara principal con la función GameObject.Find("Main Camera")
        * de este objeto extrae el componente Camera y de esta forma asigna la camara desde codigo*/ 
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();        
    }

    // Update is called once per frame
    void Update()
    {   
        Vector3 playerInput = GetPlayerInput();
        Vector3 playerDirection = new Vector3(0, 0, 0);
        if(playerInput.magnitude >= 0.1f){
            /*Obtiene el ángulo de la dirección del movimiento (componentes x e y de Input) en radianes 
             y lo pasa a grados. Luego lo suma al float que es el angulo del vector y de la camara en coordenadas del mundo (euler)
             de este modo alinea el m ovimiento del jugador con la orientación de la camara, para que los controles se orienten con 
             la dirección de la camara. Este será el nuevo ángulo que debe adoptar el jugador*/
            //float targetAngle = Mathf.Atan2(playerInput.x, playerInput.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float targetAngle = Mathf.Atan2(playerInput.x, playerInput.z) * Mathf.Rad2Deg; 
            if(targetAngle == 180){
                playerAnimatorController.SetTrigger("StationaryToTurn180");    
            }
            //Debug.Log(" targetAngle " + targetAngle);
            targetAngle += mainCamera.transform.eulerAngles.y;
            
            /*Mathf.SmoothDampAngle hace rotar suavemente desde el ángulo actual del vector y hasta el nuevo angulo
            turnSmoothVelocity se pasa como referencia y adopta valores que le impone la función
            turnSmoothTime el el tiempo en que hacer estas transformaciones angulares*/
            float angle = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            /* No entiendo totalmente como funciona el siguiente segmento de codigo
            Quaternion.Euler(0f, targetAngle, 0f) genera una rotación en el eje Y de tantos grados como indique targetAngle.
            La rotación en el eje Y significa que este se usará como pivote y repercutirá en un cambio en los angulos de X y Z
            Luego al multiplicarlo por Vector3.forward que es igual a (0, 0, 1) se convertirá en un desplazamiento en el eje Z */
            //Vector3 playerDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            playerDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            /* A la playerDirection del jugador se la multiplica por playerSpeed antes
            de aplicar la gravedad con GetGravity(playerDirection) para no afectar la  velocidad de la gravedad
            o sea que la caida no sea afectada con la velocidad horizontal*/
            playerDirection = playerDirection.normalized * playerSpeed;
            
            
            player.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        /* Aunque no se mueva el jugador será afectado por la gravedad */
        playerDirection = GetGravity(playerDirection);
        playerDirection = PlayerSkill(playerDirection);
        player.Move(playerDirection * Time.deltaTime);    
        float playerVelocityMagnitud =  player.velocity.magnitude * playerSpeed;
        //float playerInputMagnitud = playerInput.magnitude * playerSpeed;
        playerAnimatorController.SetFloat("PlayerWalkVelocity", playerVelocityMagnitud);        
    }

    private Vector3 GetPlayerInput(){
        float verticalMove = Input.GetAxis("Vertical");//Corresponde a X en un Vector2
        float horizontalMove = Input.GetAxis("Horizontal");//Corresponde a Y en un Vector2
        //Compone un Vector3 con los input y la velocidad de caida
        Vector3 input = new Vector3(horizontalMove, 0, verticalMove);        
        // Devuelve un vector normalizado, los valores de sus componentes no superan 1
        return input.normalized;
    }

    private Vector3 PlayerSkill(Vector3 move){
        //isPlayerGrounded = player.isGrounded;
        if(player.isGrounded && Input.GetButtonDown("Jump")){//Si está en el suelo y preciono espacio
            fallVelocity = jumpForce;   
            playerAnimatorController.SetTrigger("PlayerJump");   
        }
        move.y = fallVelocity;
        return move;         
    }

    private Vector3 GetGravity(Vector3 move){
        isPlayerGrounded = player.isGrounded;
        if(player.isGrounded){//Si está en el suelo
            fallVelocity = -2f; //Para simular el empuje constante de la gravedad
            playerAnimatorController.SetBool("IsGrounded", true); 
        } else{//Si está en el aire, simula la caida
            /** Como silula una aceleración mientras caiga de más alto más aceleración */
            fallVelocity -= gravity * Time.deltaTime;//Le resta el valor de gravity * Time.deltaTime a fallVelocity
            playerAnimatorController.SetBool("IsGrounded", false); 
            playerAnimatorController.SetFloat("PlayerVerticalVelocity", fallVelocity);
        }   
           
        move.y = fallVelocity;
        move = SlideDown(move);//Aplica si es necesario la función de deslizamiento en pendientes
        /*Devuelve el vector parametro con la componente Y y la dinamica de pendiente modificada*/
        return move;
    }    

    private Vector3 SlideDown(Vector3 move){
        //slopAngle = Vector3.Angle(Vector3.up, hitNormal);
        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) > player.slopeLimit;
        //componente_Y = hitNormal.y;
        float slopAngleAcceleration = 1f - hitNormal.y;//Con esto se acelera la caida mientras mayor sea la pendiente
        if(isOnSlope){
            move.x += slopAngleAcceleration * hitNormal.x * slideVelocity;
            move.z += slopAngleAcceleration * hitNormal.z * slideVelocity;
            move.y -= slopeForceDown;
        }
        return move;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        hitNormal = hit.normal; 
    }
    private void OnAnimatorMove() {
        
    }
   
}
        
