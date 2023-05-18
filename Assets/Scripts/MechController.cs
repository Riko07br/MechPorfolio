using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechController : MonoBehaviour
{
    [Header("Stats")]    
    [SerializeField] float moveSpeed = 20f;             //Velocidade de movimento
    [SerializeField] float jumpForce = 20f;             //Forca de pulo
    [SerializeField] float weight = 10f;                //Peso, usado no calculo da gravidade
    [SerializeField] float groundCheckRadius;           //Usado no teste de grounded

    [Header("Legs")]
    [SerializeField] Transform legsParent;              //Parte que vai rotacionar e influenciar a direcao do movimento
    [SerializeField] float legRotationSpeed = .7f;      //Velocidade de rotação das pernas
    [SerializeField] bool debugSqrVelocity = false;     //Flag para mostrar a velcidade
    [SerializeField] float legMoveAnimationSpeed = 3f;

    //Gravidade---------------------------------
    readonly float maxFallSpeed = -150f;                //evita acumulo de velocidade de queda
    readonly float groundCheckDistance = .17f;          //Usado no tese de grounded

    bool isGrounded = false;                            //flag para chcar se esta no chao
    bool canGroundCheck = true;                         //flag para evitar checar grounded logo apos pular
    int groundLayer;                                    //camada do chao
    float airTime = 0f;                                 //tempo no ar, usado para contar o tempo para reativar a checagem de chao
    float verticalSpeed = 0f;                           //velocidade vertical para pular e trazer ao chao
    Vector3 groundCheckOffset;                          //offset para calcular o ray de grounded

    //Pernas-------------------------------------    
    readonly string animatorMoveFloat = "move";         //nome da variavel responsavel pela animacao de movimento
    readonly string animatorRotateFloat = "rotate";     //nome da variavel responsavel pela animacao de rotacao
    readonly string animatorMoveSpeedFloat = "moveSpeed";//nome da variavel responsavel pela velocidade das animacoes de movimento e rotacao

    Animator legsAnimator;                              //Componente para animar as pernas

    //Componentes--------------------------------
    Rigidbody movementRb;                               //rb responsavel pelo movimento

    //Propriedades--------------------------------
    Vector2 CurrentInput { get; set; }

    //Metodos-------------------------------------

    
    void RotateTorso()
    {
        //TODO: metodo para rotacionar o torso para a camera com limitacoes
    }

    void AnimateLegs()
    {
        //retona caso nao tenha animator nas pernas
        if (legsAnimator == null)
            return;

        //usado para testar a velocidade de animacao
        if (debugSqrVelocity)
            Debug.Log(movementRb.velocity.sqrMagnitude);

        //limita a variacao de velocidade de animacao de 0 a 1
        float clampedSpeed = Mathf.Clamp01(movementRb.velocity.sqrMagnitude / legMoveAnimationSpeed);
        
        //evita pequeno movimentos indesejados
        if (clampedSpeed < .05f)
            clampedSpeed = 0f;

        //evita que as pernas parem de se mover ao girar sem sair do lugar
        if(clampedSpeed < .5f && Mathf.Abs(CurrentInput.x) > .5f)
            clampedSpeed = 1f;
        
        legsAnimator.SetFloat(animatorMoveSpeedFloat, clampedSpeed);        
        legsAnimator.SetFloat(animatorMoveFloat, Vector3.Dot(movementRb.velocity, legsParent.forward));
        legsAnimator.SetFloat(animatorRotateFloat, CurrentInput.x);
    }

    //Metodos Unity------------------------------
    private void Awake()
    {
        //configurar pernas
        legsAnimator = legsParent.GetComponentInChildren<Animator>();

        //define as variaveis e componentes
        groundLayer = LayerMask.GetMask("Default");        
        groundCheckOffset = new Vector3(0f, groundCheckRadius + .1f, 0f);
        movementRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CurrentInput = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            canGroundCheck = false;
            isGrounded = false;
            airTime = 0f;
            verticalSpeed = jumpForce;            
        }

        Vector3 orientationRotation = legsParent.rotation.eulerAngles;
        orientationRotation.y += (CurrentInput.x * legRotationSpeed);

        //evita overflow na rotacao
        if (orientationRotation.y > 360f)
            orientationRotation.y -= 360f;
        else if (orientationRotation.y < 0f)
            orientationRotation.y += 360;

        legsParent.rotation = Quaternion.Euler(orientationRotation);

        AnimateLegs();
    }

    private void FixedUpdate()
    {
        //teste para grounded com coyote time
        if (canGroundCheck && Physics.SphereCast(transform.position + groundCheckOffset, groundCheckRadius, Vector3.down, out _, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            if (!isGrounded)
                isGrounded = true;

            airTime = 0f;
        }
        else
        {
            airTime += Time.fixedDeltaTime;
            if (airTime > .2f)
            {
                isGrounded = false;
                canGroundCheck = true;
            }
        }

        //pega o input para ir para frente
        Vector3 inputDir = legsParent.forward * CurrentInput.y * moveSpeed * Time.fixedDeltaTime * 100f;

        //adiciona drag para melhor movimento
        movementRb.drag = inputDir.sqrMagnitude > 0f || !isGrounded ? 6f : 10f;

        //adiciona a velocidade vertical e leva em conta a gravidade
        verticalSpeed = Mathf.Clamp(verticalSpeed - Mathf.Pow(weight * Time.fixedDeltaTime, 2), isGrounded ? 0f : maxFallSpeed, 400f);
        inputDir += new Vector3(0, verticalSpeed, 0);

        movementRb.AddForce(inputDir, ForceMode.Acceleration);
        
    }
}
