using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using JetBrains.Annotations;
using Unity.Collections;



public class Player : NetworkBehaviour {
    


    public float movementSpeed = 50f;
    public float rotationSpeed = 130f;
    public NetworkVariable<Color> PlayerColor1 = new NetworkVariable<Color>(Color.red);
    public NetworkVariable<FixedString128Bytes> txtPlayerName = new NetworkVariable<FixedString128Bytes>("");
    public BulletSpawner bulletSpawner;
    public NetworkVariable<int> ScoreNetVar = new NetworkVariable<int>(0);
    private Camera playerCamera;

    private GameObject playerBody;

    public string HitString;

    public TextMeshProUGUI TextBox;

    public float scrollValue;

    public string test;

    private void NetworkInit()
    {
        playerBody = transform.Find("playerBody").gameObject;
        
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;

        TextBox = transform.Find("Camera/Canvas/PlayerData/HitCountTxt").GetComponent<TextMeshProUGUI>();
        
        
        ApplyColor();
        PlayerColor1.OnValueChanged += OnPlayerColorChanged;

        if (IsClient) {
            ScoreNetVar.OnValueChanged += ClientOnScoreValueChanged;
        }
        
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

    private void ClientOnScoreValueChanged (int old, int current) {
        if (IsOwner) {
            NetworkHelper.Log(this, $"My score is {ScoreNetVar.Value}");
            int hits = ScoreNetVar.Value;
            HitString = hits.ToString();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (IsServer) {
            ServerHandleCollision(collision);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(IsServer) {
            if(other.CompareTag("PowerUp")) {
                other.GetComponent<BasePowerUp>().ServerPickUp(this);
            }
        }
    }

    private void ServerHandleCollision(Collision collision) {
        if(collision.gameObject.CompareTag("Bullet")) {
            ulong ownerId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            Player other = NetworkManager.Singleton.ConnectedClients[ownerId].PlayerObject.GetComponent<Player>();
            other.ScoreNetVar.Value += 1;
            Destroy(collision.gameObject);
        }
    }

    private void Update() {
        if (IsOwner){
            
            HandleInput();
            ScrollCam();
            if(HitString == null) {
                TextBox.text = "Hits: 0";
            }
            test = txtPlayerName.Value.ToString();
            TextBox.text = txtPlayerName.Value + "<br>Hits: " + HitString;
            if (Input.GetButtonDown("Fire1")) {
                NetworkHelper.Log("Requesting Fire");
                bulletSpawner.FireServerRpc();
            }
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
            
        MoveServerRpc(CalcMovement(), CalcRotation(), OwnerClientId);
        
        
    }

    private void ApplyColor()
    {
        NetworkHelper.Log(this, $"Applying color {PlayerColor1.Value}");
        playerBody.GetComponent<MeshRenderer>().material.color = PlayerColor1.Value;
    }

    [ServerRpc(RequireOwnership = true)]
    private void MoveServerRpc(Vector3 movement, Vector3 rotation, ulong ClientId)
    {
        
        Vector3 posNew = transform.localPosition + movement;
        if(posNew.x > 20 || posNew.x < -20 || posNew.z > 20 || posNew.z < -20) {
            if(posNew.x > 20) {
                posNew.x = 20;
            }
            if(posNew.x < -20) {
                posNew.x = -20;
            }
            if(posNew.z > 20) {
                posNew.z = 20;
            }
            if(posNew.z < -20) {
                posNew.z = -20;
            }
            transform.localPosition = posNew;
        }
        else {
            transform.Translate(movement);
        }
        transform.Rotate(rotation);
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
        bool isSpaceKeyDown = Input.GetKey(KeyCode.Space);
        float x_move = 0.0f;
        float z_move = 0.0f;
        z_move = Input.GetAxis("Vertical");

        if (isShiftKeyDown) {
            x_move = Input.GetAxis("Horizontal");
        } else if (isSpaceKeyDown) {
            movementSpeed = 100f;
        } else {
            movementSpeed = 50f;
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed * Time.deltaTime;

        return moveVect;
    }

    private void ScrollCam() {
        Vector3 pos = playerCamera.transform.localPosition;
        if(pos.y < 6) {
            pos.y = 6;
        }
        pos.y += Input.mouseScrollDelta.y / 2;
        scrollValue = pos.y;
        playerCamera.transform.localPosition = pos;
    }
}
