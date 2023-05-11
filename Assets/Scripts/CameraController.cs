using UnityEngine;

//componente colocado diretamente na camera, ela nao deve ter parent na hierarquia
public class CameraController : MonoBehaviour
{    
    [SerializeField] float mouseSpeed = 10f;    //multiplicador de velocidade do mouse
    [SerializeField] bool isCameraDisabled = false;   //flag para desativar o movimento da camera
    [SerializeField] bool isCameraFollowing = true; //flag para a camera nao seguir o player

    [Tooltip("Objeto que a camera vai seguir, deve estar no player")]
    [SerializeField] Transform camRefParent;   //os objetos para o scrip funcionar terao esse obj como parent 

    [SerializeField] Transform camLimitingParent;   //referencia para limitar a rotacao lateral da camera, tambem e uma flag
    
    [Header("Limites verticais")]
    [Tooltip("Quanto maior, mais a camera consegue rotacionar para baixo")]
    [SerializeField] float minXAngle = 10; //angulos limitantes para baixo e para cima
    [Tooltip("Quanto menor, mais a camera consegue rotacionar para cima")]
    [SerializeField] float maxXAngle = 350; //angulos limitantes para baixo e para cima

    Transform cameraPivot; //Parent do cameraFollow, colocado no player, vai girar somente em Y
    Transform cameraFollow; //Vai girar somente em X e serve como referencia principal para a camera

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraPivot = new GameObject().transform;
        cameraPivot.name = "CameraPivot";
        cameraPivot.parent = camRefParent;

        cameraFollow = new GameObject().transform;
        cameraFollow.name = "Camera Follow";
        cameraFollow.parent = cameraPivot;

        cameraPivot.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        cameraFollow.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private void Update()
    {
        if (isCameraFollowing)
            transform.position = cameraFollow.position;

        if (isCameraDisabled)
            return;

        Vector2 mouseMove = new (-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

        //resolve a rotacao de cima para baixo
        float camAngle = cameraFollow.localEulerAngles.x + (mouseMove.x * mouseSpeed);
        
        if (camAngle > 180 && camAngle < maxXAngle)
            camAngle = maxXAngle;
        else if (camAngle < 180 && camAngle > minXAngle)
            camAngle = minXAngle;        

        cameraFollow.localEulerAngles = new Vector3(camAngle, 0f);

        //resolve a rotacao para os lados
        float camPivotAngle = cameraPivot.localEulerAngles.y + (mouseMove.y * mouseSpeed);

        if (camPivotAngle > 360f)
            camPivotAngle -= 360f;
        else if (camPivotAngle < 0f)
            camPivotAngle += 360;

        cameraPivot.localEulerAngles = new Vector3(0f, camPivotAngle);

        //limita a rotacao em relacao as pernas para um angulo maximo
        if (camLimitingParent == null)
            return;

        float YAngle = Vector3.SignedAngle(cameraPivot.forward, camLimitingParent.forward, Vector3.up);

        if (YAngle < -90f)        
            cameraPivot.Rotate(Vector3.up, 90f + YAngle);        
        else if (YAngle > 90f)        
            cameraPivot.Rotate(Vector3.up, -90f + YAngle);

        //aplica a rotacao levando em conta somente para onde o pivot aponta (evita gira da camera)
        transform.rotation = Quaternion.LookRotation(cameraFollow.forward, Vector3.up);
    }
}
