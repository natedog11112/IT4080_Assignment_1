using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;



public class Player : NetworkBehaviour {
    


    public float movementSpeed = 50f;
    public float rotationSpeed = 130f;
    public NetworkVariable<Color> PlayerColor2 = new NetworkVariable<Color>(Color.red);
    private Camera playerCamera;

    private GameObject playerBody;

    private void NetworkInit()
    {
        playerBody = transform.Find("playerBody").gameObject;
        
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;
        
        ApplyColor();
        PlayerColor2.OnValueChanged += OnPlayerColorChanged;
        
    }

    private void Awake() {
        NetworkHelper.Log(this, "Awake");
    }
    private void Start()
    {
        NetworkHelper.Log(this, "Start");
    }

    public override void OnNetworkSpawn()
    {
        NetworkHelper.Log(this, "OnNetworkSpawn");
        NetworkInit();
        base.OnNetworkSpawn();
    }
    private void Update() {
        if (IsOwner){
            
            HandleInput();
        }
        
    }

    public void OnPlayerColorChanged(Color previous, Color current)
    {
        ApplyColor();
    }
    private void HandleInput()
    {
        Vector3 movement = CalcMovement();
        Vector3 rotation = CalcRotation();

        if(movement != Vector3.zero || rotation != Vector3.zero)
        {
            
            MoveServerRpc(CalcMovement(), CalcRotation(), OwnerClientId);
        }
        
    }

    private void ApplyColor()
    {
        NetworkHelper.Log(this, $"Applying color {PlayerColor2.Value}");
        playerBody.GetComponent<MeshRenderer>().material.color = PlayerColor2.Value;
    }

    [ServerRpc(RequireOwnership = true)]
    private void MoveServerRpc(Vector3 movement, Vector3 rotation, ulong ClientId)
    {
                if (ClientId > 0)
                {
                    if(playerBody.transform.position.x + movement.x > 5)
                    {
                        movement.x = -1;
                    }
                    if(playerBody.transform.position.x + movement.x < 0)
                    {
                        movement.x = 1;
                    }
                    if(playerBody.transform.position.z + movement.z > 5)
                    {
                        movement.z = -1;
                    }
                    if(playerBody.transform.position.z + movement.z < 0)
                    {
                        movement.z = 1;
                    }
                    transform.Translate(movement);
                    transform.Rotate(rotation);
                }
                else
                {
                    transform.Translate(movement);
                    transform.Rotate(rotation);
                }
    }

    // Rotate around the y axis when shift is not pressed
    private Vector3 CalcRotation() {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Vector3 rotVect = Vector3.zero;
        if (!isShiftKeyDown) {
            rotVect = new Vector3(0, Input.GetAxis("Horizontal"), 0);
            rotVect *= rotationSpeed * Time.deltaTime;
        }
        return rotVect;
    }


    // Move up and back, and strafe when shift is pressed
    private Vector3 CalcMovement() {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = 0.0f;
        z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown) {
            x_move = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed * Time.deltaTime;

        return moveVect;
    }
}
