using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = 9.8f;
    
    [Header("视角设置")]
    public float mouseSensitivity = 2f;
    public float lookUpLimit = 90f;
    public float lookDownLimit = -90f;
    
    [Header("天气控制")]
    public KeyCode toggleRainKey = KeyCode.R; // 按R键切换雨的状态
    public KeyCode toggleSnowKey = KeyCode.S; // 按S键切换雪的状态
    public KeyCode toggleDayNightKey = KeyCode.D; // 按D键切换日夜状态
    public KeyCode triggerLightningKey = KeyCode.L; // 按L键触发闪电
    
    private CharacterController characterController;
    private Camera playerCamera;
    private float rotationX = 0;
    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded;
    
    void Awake()
    {
        // 设置玩家标签，便于查找
        gameObject.tag = "Player";
    }
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        if (playerCamera == null)
        {
            Debug.LogError("未找到玩家相机！");
            return;
        }
        
        // 锁定并隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("玩家控制器已初始化");
    }
    
    void Update()
    {
        // 检查是否在地面上
        isGrounded = characterController.isGrounded;
        
        // 处理旋转
        HandleRotation();
        
        // 处理移动
        HandleMovement();
        
        // 处理天气控制
        HandleWeatherControl();
    }
    
    void HandleRotation()
    {
        // 水平旋转整个玩家
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);
        
        // 垂直旋转只旋转相机
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, lookDownLimit, lookUpLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
    
    void HandleMovement()
    {
        // 检查CharacterController是否启用，如果未启用则不处理移动
        if (characterController == null || !characterController.enabled)
        {
            return;
        }
        
        // 如果在地面上，重置Y方向速度
        if (isGrounded)
        {
            moveDirection.y = -0.5f; // 小的向下力，确保与地面接触
            
            // 跳跃
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
            }
        }
        
        // 应用重力
        moveDirection.y -= gravity * Time.deltaTime;
        
        // 获取输入方向
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // 计算移动方向（相对于玩家朝向）
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // 确定速度（按住Shift键奔跑）
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        // 应用移动
        characterController.Move((move * currentSpeed + new Vector3(0, moveDirection.y, 0)) * Time.deltaTime);
    }
    
    void HandleWeatherControl()
    {
        // 按R键切换雨的状态
        if (Input.GetKeyDown(toggleRainKey))
        {
            // 查找GameManager并切换雨的状态
            GameManager manager = GameManager.Instance;
            if (manager != null)
            {
                manager.ToggleRain();
            }
        }
        
        // 按S键切换雪的状态
        if (Input.GetKeyDown(toggleSnowKey))
        {
            // 查找GameManager并切换雪的状态
            GameManager manager = GameManager.Instance;
            if (manager != null)
            {
                manager.ToggleSnow();
            }
        }
        
        // 按D键切换日夜状态
        if (Input.GetKeyDown(toggleDayNightKey))
        {
            // 查找GameManager并切换日夜状态
            GameManager manager = GameManager.Instance;
            if (manager != null)
            {
                manager.ToggleDayNight();
            }
        }
        
        // 按L键触发闪电
        if (Input.GetKeyDown(triggerLightningKey))
        {
            // 查找GameManager并触发闪电
            GameManager manager = GameManager.Instance;
            if (manager != null)
            {
                Debug.Log("PlayerController: 按下L键，触发闪电");
                manager.TriggerLightning();
            }
        }
    }
}
