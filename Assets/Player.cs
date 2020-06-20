using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

public class Player : MonoBehaviour
{
    [SerializeField]
    public Vector3 eulerAngles = Vector3.zero;
    public float mouseSensitivity = 700.0f;
    public float movementSpeed = 8.0f;
    private Vector3 velocity;
    public float gravity = 4.0f;
    public float flySpeed = 16.0f;
    public float jumpSpeed = 16.0f;
    public bool fly = false;
    

    private CharacterController controller;

    public GameObject indicator;
    public ChunkManager chunkManager;

    private static string horizontalRotation = "Mouse X";
    private static string verticalRotation = "Mouse Y";
    private static string forwardAxis = "Vertical";
    private static string sidewaysAxis = "Horizontal";
    private static string jumpAxis = "Jump";

    private static string breakBlockKey = "Fire1";
    private static string placeBlockKey = "Fire2";

    private static string flyKey = "v";

    private static string saveGameKey = "e";
    private static string loadGameKey = "r";


    private float breakingTime;
    private BlockCoord breakingBlock;
    public float miningSpeed = 2.0f;

    public int selectedBlock = 0;
    public float selectedBlockScroll = 0;
    private int prevSelectedBlock = -1;
    private float placeTime = 0;
    private int prevGhostedBlockType = -1;

    [HideInInspector, SerializeField]
    public bool hasSpawned = false;

    void Start() {
        // Lock cursor to center
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if(!hasSpawned)
        {
            transform.position = chunkManager.PickSpawnSpot();
            hasSpawned = true;
            chunkManager.Refresh();
        }

        // Rotate camera
        eulerAngles.y += Input.GetAxis(horizontalRotation) * mouseSensitivity * Time.deltaTime;
        eulerAngles.x = Mathf.Clamp(eulerAngles.x - Input.GetAxis(verticalRotation) * mouseSensitivity * Time.deltaTime, -90.0f, 90.0f);

        transform.localRotation = Quaternion.Euler(eulerAngles);

        // Toggle fly (noclip)
        if(Input.GetKeyDown(flyKey))
        {
            fly = !fly;
            controller.detectCollisions = fly;

            if(fly)
            {
                velocity = Vector3.zero;
            }
        }

        // Handle block selection
        selectedBlockScroll = (Mathf.Repeat(selectedBlockScroll + Input.mouseScrollDelta.y, (float)Block.Type.Count));
        selectedBlock = Mathf.FloorToInt(selectedBlockScroll);

        // Handle block world related stuff
        if (chunkManager)
        {
            // Calculate movement
            float forward = Input.GetAxis(forwardAxis);
            float right = Input.GetAxis(sidewaysAxis);

            if (fly)
            {
                var movementDirection = (transform.localRotation * Vector3.forward * forward + transform.localRotation * Vector3.right * right);
                controller.transform.position += movementDirection.normalized * flySpeed * Time.deltaTime;
                //controller.Move(movementDirection.normalized * flySpeed * Time.deltaTime);
            }
            else
            {
                var chunk = chunkManager.Get(new BlockCoord(Vector3Int.FloorToInt(transform.position)).ChunkCoord);
                if (chunk != null && chunk.chunkMesh != null)
                {
                    if(controller.isGrounded)
                    {
                        if(velocity.y < 0)
                        {
                            velocity.y = 0;
                        }

                        velocity.x = 0;
                        velocity.z = 0;

                        if(Input.GetAxis(jumpAxis) > 0.0 && velocity.y < float.Epsilon)
                        {
                            velocity.y += Input.GetAxis(jumpAxis) * jumpSpeed;
                        }
                    }
                    else
                    {
                        velocity.y -= gravity * Time.deltaTime;
                    }

                    var movementRotation = Quaternion.Euler(new Vector3(0, eulerAngles.y, 0));
                    controller.Move(((movementRotation * new Vector3(right, 0, forward)).normalized * movementSpeed + velocity) * Time.deltaTime);
                }
            }

            // Block aim indicator
            BlockRaycastResult blockRaycastResult;
            bool raycastHit = chunkManager.Raycast(transform.position, transform.TransformDirection(Vector3.forward), 16.0f, out blockRaycastResult);
            
            if (indicator)
            {
                var renderer = indicator.GetComponent<MeshRenderer>();
                if(raycastHit)
                {
                    renderer.enabled = true;

                    // If breaking a block or currently having selected air -> indicator will be placed inside the targeted block
                    var targetBlock = (selectedBlock == 0 || breakingTime > 0.0f ? blockRaycastResult.hitBlock : blockRaycastResult.GetPlacementBlockPosition()).block;
                    indicator.transform.position = targetBlock + new Vector3(0.5f, 0.5f, 0.5f);

                    // If digging use air indicator, else use texture of the block
                    var ghostBlockType = breakingTime > 0.0f ? 0 : selectedBlock;
                    var mat = Block.Get((uint)ghostBlockType).material;

                    if (ghostBlockType != prevGhostedBlockType)
                    {
                        // TODO copy material instead?
                        prevGhostedBlockType = ghostBlockType;
                        renderer.material.SetTexture(Shader.PropertyToID("_MainTex"), mat.GetTexture(Shader.PropertyToID("_MainTex")));
                        renderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                    }
                }
                else
                {
                    renderer.enabled = false;
                }
            }

            // Mining
            if (Input.GetAxis(breakBlockKey) > 0.25f && raycastHit)
            {
                if (blockRaycastResult.hitBlock != breakingBlock)
                {
                    breakingTime = 0;
                    breakingBlock = blockRaycastResult.hitBlock;
                }

                breakingTime += Time.deltaTime * miningSpeed;

                if(breakingTime >= blockRaycastResult.block.hardness)
                {
                    chunkManager.SetBlock(blockRaycastResult.hitBlock, (uint)Block.Type.Air);
                    breakingTime = 0;
                }
            }
            else
            {
                breakingTime = 0;
            }

            // Block placing
            if(selectedBlock > 0)
            {
                if (Input.GetAxis(placeBlockKey) > 0.5f)
                {
                    if(placeTime <= 0.0f)
                    {
                        if(raycastHit)
                        {
                            var targetPosition = blockRaycastResult.GetPlacementBlockPosition();
                            if (chunkManager.GetBlock(targetPosition) == (uint)Block.Type.Air && targetPosition.block != Vector3Int.FloorToInt(transform.position))
                            {
                                chunkManager.SetBlock(targetPosition, (uint)selectedBlock);
                                placeTime = 0.3f;
                            }
                        }
                    }
                    else
                    {
                        placeTime -= Time.deltaTime;
                    }
                }
                else
                {
                    placeTime = 0.0f;
                }
            }
        }

        prevSelectedBlock = selectedBlock;

        // Save + Load

        if(Input.GetKeyDown(saveGameKey))
        {
            Save.SaveGame(this, Application.persistentDataPath + "/blockgame.save");
        }
        if(Input.GetKeyDown(loadGameKey))
        {
            if(!Save.LoadGame(this, Application.persistentDataPath + "/blockgame.save"))
            {
                Debug.LogError("Couldn't load save");
            }
        }
    }
}
