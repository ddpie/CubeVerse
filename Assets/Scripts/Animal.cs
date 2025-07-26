using UnityEngine;
using System.Collections;

public class Animal : MonoBehaviour
{
    [Header("基础设置")]
    public AnimalType animalType;
    public float moveSpeed = 2f;
    public float jumpForce = 3f;
    public float idleTime = 2f;
    public float moveTime = 3f;
    
    [Header("颜色设置")]
    public Color mainColor = Color.white;
    public Color secondaryColor = Color.gray;
    
    protected bool isMoving = false;
    protected Vector3 moveDirection;
    protected float actionTimer;
    protected bool isJumping = false;
    protected float groundY;
    protected float gravity = 20f; // 添加重力
    protected float verticalVelocity = 0f; // 垂直速度
    
    public enum AnimalType
    {
        Rabbit,
        Chicken,
        Cat,
        Dog,
        Sheep,
        Tiger,
        Lion,
        Elephant
    }
    
    protected virtual void Start()
    {
        // 初始化地面位置
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            transform.position = hit.point;
            groundY = hit.point.y;
        }
        
        StartCoroutine(ActionRoutine());
    }
    
    protected virtual void Update()
    {
        ApplyGravity();
        
        if (isMoving && !isJumping)
        {
            Move();
        }
    }

    protected void ApplyGravity()
    {
        if (isJumping)
            return;

        RaycastHit hit;
        float rayDistance = 10f;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        // 检测下方是否有地面
        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance))
        {
            float distanceToGround = hit.distance - 0.1f;
            
            if (distanceToGround > 0)
            {
                // 如果在空中，应用重力
                verticalVelocity -= gravity * Time.deltaTime;
                transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
            }
            else
            {
                // 如果在地面上或略微陷入地面，调整位置并重置垂直速度
                transform.position = new Vector3(
                    transform.position.x,
                    hit.point.y,
                    transform.position.z
                );
                verticalVelocity = 0;
            }
            
            groundY = hit.point.y;
        }
        else
        {
            // 如果检测不到地面，继续下落
            verticalVelocity -= gravity * Time.deltaTime;
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
        }
    }

    protected virtual void Move()
    {
        // 在移动前检查前方地形
        Vector3 nextPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        
        // 射线检测前方地形
        RaycastHit hit;
        Vector3 rayStart = nextPosition + Vector3.up * 0.1f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 2f))
        {
            // 如果高度差太大，改变方向
            if (Mathf.Abs(hit.point.y - groundY) > 1f)
            {
                moveDirection = Quaternion.Euler(0, Random.Range(90f, 270f), 0) * moveDirection;
                return;
            }
            
            // 只更新水平位置，让重力系统处理垂直运动
            transform.position = new Vector3(
                nextPosition.x,
                transform.position.y,
                nextPosition.z
            );
        }
        else
        {
            // 如果前方没有地形，改变方向
            moveDirection = Quaternion.Euler(0, Random.Range(90f, 270f), 0) * moveDirection;
        }
    }
    
    protected virtual IEnumerator ActionRoutine()
    {
        while (true)
        {
            // 增加跳跃的概率
            if (Random.value < 0.5f && !isJumping)
            {
                StartJump();
                // 跳跃后短暂等待
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            }
            else if (!isJumping)
            {
                StartMove();
                // 移动一段时间
                yield return new WaitForSeconds(Random.Range(1f, 2f));
            }
            
            // 停止动作
            StopMove();
            
            // 短暂休息
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }
    
    protected virtual void StartMove()
    {
        if (!isJumping)
        {
            isMoving = true;
            moveDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
        }
    }
    
    protected virtual void StopMove()
    {
        isMoving = false;
    }
    
    protected virtual void StartJump()
    {
        if (!isJumping)
        {
            StartCoroutine(JumpRoutine());
        }
    }
    
    protected virtual IEnumerator JumpRoutine()
    {
        isJumping = true;
        verticalVelocity = jumpForce; // 设置初始向上速度
        
        // 等待直到回到地面
        while (true)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
            
            // 检查是否着陆
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.2f))
            {
                transform.position = new Vector3(
                    transform.position.x,
                    hit.point.y,
                    transform.position.z
                );
                break;
            }
            
            yield return null;
        }
        
        verticalVelocity = 0;
        isJumping = false;
    }
}
