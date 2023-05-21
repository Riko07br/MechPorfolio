using Unity.VisualScripting;
using UnityEngine;

//componente colocado diretamente na camera, ela nao deve ter parent na hierarquia
public class CameraController : MonoBehaviour
{
    [SerializeField] bool isCameraDisabled = false; //flag para desativar o movimento da camera
    [SerializeField] bool isCameraFollowing = true; //flag para a camera nao seguir o player
    [Space]
    [SerializeField] Transform target;              // Ponto em torno do qual a camera orbitara
    [SerializeField] float orbitSpeed = 10f;        //multiplicador de velocidade do mouse
    [SerializeField] float verticalLimit = 10;      //angulo limitante para baixo e para cima
    [SerializeField] float verticalOffset = 25f;    //offset inicial do angulo 
    [Space]
    [SerializeField] float height = 5f;             // Altura da c�mera ao ponto de refer�ncia
    [SerializeField] float distance = 5f;           // Dist�ncia da c�mera ao ponto de refer�ncia    
    [SerializeField] float minDistance = 1f;        // Dist�ncia m�nima permitida
    [SerializeField] float maxDistance = 10f;       // Dist�ncia m�xima permitida

    float rotationX = 0f;                           // Rota��o vertical atual da c�mera    
    Transform refParent, refChild;                  //Referencias da camera, criadas ao inicializa-la

    //Retorna o Objeto referencia que rotationa em Y
    public Transform GetRefParent { get => refParent; }

    private void Awake()
    {
        //inicializa escondendo o cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //cria os objetos referencia e configura
        refParent = new GameObject("Ref Parent").transform;
        refChild = new GameObject("Ref Child").transform;
        refChild.parent = refParent;
        refParent.position = transform.position;
    }  

    void LateUpdate()
    {
        if (isCameraDisabled)
            return;

        //Obtem o movimento do mouse horizontal e vertical
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");

        //Rotaciona a c�mera horizontalmente em torno do ponto de refer�ncia
        refParent.RotateAround(target.position, Vector3.up, horizontal * orbitSpeed * Time.deltaTime);

        //Rotaciona a c�mera verticalmente
        rotationX -= vertical * orbitSpeed * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -verticalOffset - verticalLimit, -verticalOffset + verticalLimit);
        refChild.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        //Calcula a nova posi��o da c�mera
        Vector3 offset = new Vector3(0f, height, -distance);
        Quaternion rotation = Quaternion.Euler(0f, refParent.rotation.eulerAngles.y, 0f);
        refParent.position = target.position + rotation * offset;

        //Verifica se a dist�ncia est� dentro dos limites
        if (isCameraFollowing)
            distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y, minDistance, maxDistance);
        
        //Aplica a rotacao total do child na camera
        transform.SetPositionAndRotation(refChild.position, refChild.rotation);
    }
}
