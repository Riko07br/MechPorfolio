using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechController : MonoBehaviour
{
    readonly float maxFallSpeed = -150f;
    readonly float groundCheckDistance = .17f;

    [Header("Stats")]    
    [SerializeField] float moveSpeed = 180f;
    [SerializeField] float jumpForce = 50f;
    [SerializeField] float weight = 10f;    //sado no calculo da gravidade
    [SerializeField] float groundCheckRadius;   //usado no teste de grounded

    [Header("Legs")]
    [SerializeField] Transform legsParent;  //parte que vai rotacionar e influenciar a direcao do movimento
    [SerializeField] float legRotationSpeed = 10f; //velocidade de rotação das pernas

    //gravity--------------------
    bool isGrounded = false;    //flag para chcar se esta no chao
    bool canGroundCheck = true; //flag para evitar checar grounded logo apos pular
    int groundLayer;    //camada do chao
    float airTime = 0f; //tempo no ar, usado para contar o tempo para reativar a checagem de chao
    float verticalSpeed = 0f;   //vevlocidade vertical para pular e trazer ao chao
    Vector3 groundCheckOffset;  //offset para calcular o ray de grounded

    //componentes--------------
    Rigidbody movementRb;  //rb responsavel pelo movimento


    bool Grounded
    {
        get => isGrounded;
        set
        {
            if (value != isGrounded)
            {                
                isGrounded = value;
            }

        }
    }

    float AirTime
    {
        get => airTime;
        set
        {
            airTime = value;            
        }
    }

    Vector2 CurrentInput { get; set; }

    private void Awake()
    {
        groundLayer = LayerMask.GetMask("Default");        
        movementRb = GetComponent<Rigidbody>();

        groundCheckOffset = new Vector3(0f, groundCheckRadius + .1f, 0f);
    }

    private void Update()
    {
        CurrentInput = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetButtonDown("Jump") && Grounded)
        {
            canGroundCheck = false;
            Grounded = false;
            AirTime = 0f;
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
    }

    private void FixedUpdate()
    {
        //teste para grounded com coyote time
        if (canGroundCheck && Physics.SphereCast(transform.position + groundCheckOffset, groundCheckRadius, Vector3.down, out _, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            if (!Grounded)
                Grounded = true;

            AirTime = 0f;
        }
        else
        {
            AirTime += Time.fixedDeltaTime;
            if (AirTime > .2f)
            {
                Grounded = false;
                canGroundCheck = true;
            }
        }

        //pega o input para ir para frente
        Vector3 inputDir = legsParent.forward * CurrentInput.y * moveSpeed * Time.fixedDeltaTime * 100f;

        //adiciona drag para melhor movimento
        movementRb.drag = inputDir.sqrMagnitude > 0f || !Grounded ? 6f : 10f;

        //adiciona a velocidade vertical e leva em conta a gravidade
        verticalSpeed = Mathf.Clamp(verticalSpeed - Mathf.Pow(weight * Time.fixedDeltaTime, 2), Grounded ? 0f : maxFallSpeed, 400f);
        inputDir += new Vector3(0, verticalSpeed, 0);


        movementRb.AddForce(inputDir, ForceMode.Acceleration);
    }
}
