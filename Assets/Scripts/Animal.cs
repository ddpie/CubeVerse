using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("动画设置")]
    public float animationSpeed = 1f;

    protected bool isMoving = false;
    protected Vector3 moveDirection;
    protected float actionTimer;
    protected bool isJumping = false;
    protected float groundY;
    protected float gravity = 15f; // 降低重力，让跳跃更明显
    protected float verticalVelocity = 0f;

    // 动画相关
    protected float animTime = 0f;
    protected List<Transform> bodyParts = new List<Transform>();
    protected Transform headPart;
    protected Transform tailPart;
    protected List<Transform> legParts = new List<Transform>();
    protected List<Transform> earParts = new List<Transform>();
    protected Vector3 originalScale;
    protected Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    protected Dictionary<Transform, Quaternion> originalRotations = new Dictionary<Transform, Quaternion>();
    protected float targetRotationY;
    protected float currentRotationY;

    // 玩家交互
    protected Transform playerTransform;
    protected float playerDetectRadius = 5f;
    protected bool isAlerted = false;
    protected float alertCooldown = 0f;
    
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

        // 初始化动画系统
        InitializeAnimationParts();
        originalScale = transform.localScale;
        currentRotationY = transform.eulerAngles.y;
        targetRotationY = currentRotationY;

        // 查找玩家
        if (GameManager.Instance != null)
        {
            playerTransform = GameManager.Instance.playerTransform;
        }
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
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

        // 更新动画
        UpdateAnimations();

        // 平滑转向
        UpdateRotation();
    }

    /// <summary>
    /// 初始化动画部件引用
    /// </summary>
    protected virtual void InitializeAnimationParts()
    {
        // 遍历所有子物体，根据位置分类
        foreach (Transform child in transform)
        {
            // 记录原始位置和旋转
            originalPositions[child] = child.localPosition;
            originalRotations[child] = child.localRotation;
            bodyParts.Add(child);

            // 根据位置判断部件类型
            Vector3 localPos = child.localPosition;

            // 头部 - 通常在Y轴较高且Z轴偏前的位置
            if (localPos.y > 0.7f && localPos.z > 0)
            {
                if (headPart == null || localPos.y > headPart.localPosition.y)
                {
                    headPart = child;
                }
            }

            // 尾巴 - Z轴负方向
            if (localPos.z < -0.3f && localPos.y > 0.2f)
            {
                if (tailPart == null || localPos.z < tailPart.localPosition.z)
                {
                    tailPart = child;
                }
            }

            // 腿 - Y轴较低的位置
            if (localPos.y < 0.25f && Mathf.Abs(localPos.x) > 0.1f)
            {
                legParts.Add(child);
            }

            // 耳朵 - Y轴较高，X轴有偏移（左右两边）
            if (localPos.y > 1f && Mathf.Abs(localPos.x) > 0.15f)
            {
                earParts.Add(child);
            }
        }
    }

    /// <summary>
    /// 更新所有动画
    /// </summary>
    protected virtual void UpdateAnimations()
    {
        animTime += Time.deltaTime * animationSpeed;

        // 检测玩家距离
        CheckPlayerProximity();

        if (isMoving && !isJumping)
        {
            // 走路动画
            UpdateWalkAnimation();
        }
        else if (!isJumping)
        {
            // 待机动画 - 呼吸
            UpdateIdleAnimation();
        }

        // 尾巴总是摇摆
        UpdateTailAnimation();

        // 耳朵动画
        UpdateEarAnimation();
    }

    /// <summary>
    /// 检测玩家距离并做出反应
    /// </summary>
    protected virtual void CheckPlayerProximity()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 更新警觉冷却
        if (alertCooldown > 0) alertCooldown -= Time.deltaTime;

        if (distance < playerDetectRadius)
        {
            if (!isAlerted && alertCooldown <= 0)
            {
                isAlerted = true;
                OnPlayerNearby();
            }

            // 头部看向玩家
            if (headPart != null)
            {
                Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
                float angleToPlayer = Mathf.Atan2(dirToPlayer.x, dirToPlayer.z) * Mathf.Rad2Deg;
                float relativeAngle = Mathf.DeltaAngle(currentRotationY, angleToPlayer);
                relativeAngle = Mathf.Clamp(relativeAngle, -60f, 60f); // 限制头部转动范围

                if (originalRotations.ContainsKey(headPart))
                {
                    Quaternion targetRot = originalRotations[headPart] * Quaternion.Euler(0, relativeAngle, 0);
                    headPart.localRotation = Quaternion.Lerp(headPart.localRotation, targetRot, Time.deltaTime * 3f);
                }
            }
        }
        else
        {
            if (isAlerted)
            {
                isAlerted = false;
                alertCooldown = 3f; // 3秒冷却
            }
        }
    }

    /// <summary>
    /// 玩家靠近时的反应
    /// </summary>
    protected virtual void OnPlayerNearby()
    {
        // 根据动物类型有不同反应
        switch (animalType)
        {
            case AnimalType.Rabbit:
            case AnimalType.Chicken:
                // 胆小的动物：逃跑
                if (!isJumping && playerTransform != null)
                {
                    Vector3 awayFromPlayer = (transform.position - playerTransform.position).normalized;
                    moveDirection = new Vector3(awayFromPlayer.x, 0, awayFromPlayer.z);
                    isMoving = true;
                }
                break;
            case AnimalType.Dog:
                // 狗：兴奋，摇尾巴
                StartCoroutine(ExcitedTailWag());
                break;
            case AnimalType.Cat:
                // 猫：停下来看
                isMoving = false;
                break;
            case AnimalType.Tiger:
            case AnimalType.Lion:
                // 猛兽：可能靠近玩家
                if (playerTransform != null)
                {
                    Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
                    moveDirection = new Vector3(toPlayer.x, 0, toPlayer.z);
                    isMoving = true;
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 耳朵动画
    /// </summary>
    protected virtual void UpdateEarAnimation()
    {
        if (earParts.Count == 0) return;

        float earTwitch = 0f;

        // 警觉时耳朵更活跃
        if (isAlerted)
        {
            earTwitch = Mathf.Sin(animTime * 15f) * 10f;
        }
        else
        {
            // 偶尔抖动
            earTwitch = Mathf.Sin(animTime * 3f) * 5f;
        }

        foreach (var ear in earParts)
        {
            if (ear != null && originalRotations.ContainsKey(ear))
            {
                // 左右耳朵相反方向
                float side = ear.localPosition.x > 0 ? 1f : -1f;
                Quaternion targetRot = originalRotations[ear] * Quaternion.Euler(earTwitch * side, 0, earTwitch * 0.5f);
                ear.localRotation = Quaternion.Lerp(ear.localRotation, targetRot, Time.deltaTime * 5f);
            }
        }
    }

    /// <summary>
    /// 走路动画
    /// </summary>
    protected virtual void UpdateWalkAnimation()
    {
        float walkCycle = animTime * 8f; // 走路频率

        // 身体上下摆动
        float bodyBob = Mathf.Sin(walkCycle * 2f) * 0.03f;
        transform.localScale = originalScale * (1f + bodyBob * 0.5f);

        // 身体左右摇摆
        float bodySway = Mathf.Sin(walkCycle) * 2f;

        // 腿部动画
        for (int i = 0; i < legParts.Count; i++)
        {
            if (legParts[i] != null && originalPositions.ContainsKey(legParts[i]))
            {
                // 交替抬腿
                float legPhase = walkCycle + (i % 2 == 0 ? 0 : Mathf.PI);
                float legLift = Mathf.Max(0, Mathf.Sin(legPhase)) * 0.05f;
                float legSwing = Mathf.Sin(legPhase) * 0.03f;

                Vector3 origPos = originalPositions[legParts[i]];
                legParts[i].localPosition = origPos + new Vector3(0, legLift, legSwing);
            }
        }

        // 头部轻微摆动
        if (headPart != null && originalRotations.ContainsKey(headPart))
        {
            Quaternion origRot = originalRotations[headPart];
            float headBob = Mathf.Sin(walkCycle * 2f) * 3f;
            headPart.localRotation = origRot * Quaternion.Euler(headBob, 0, 0);
        }
    }

    /// <summary>
    /// 待机动画 - 呼吸
    /// </summary>
    protected virtual void UpdateIdleAnimation()
    {
        float breathCycle = animTime * 2f; // 呼吸频率

        // 身体呼吸起伏
        float breathScale = 1f + Mathf.Sin(breathCycle) * 0.02f;
        transform.localScale = originalScale * breathScale;

        // 头部偶尔左右看
        if (headPart != null && originalRotations.ContainsKey(headPart))
        {
            Quaternion origRot = originalRotations[headPart];
            float lookAround = Mathf.Sin(animTime * 0.5f) * 15f;
            headPart.localRotation = origRot * Quaternion.Euler(0, lookAround, 0);
        }

        // 重置腿部位置
        foreach (var leg in legParts)
        {
            if (leg != null && originalPositions.ContainsKey(leg))
            {
                leg.localPosition = Vector3.Lerp(leg.localPosition, originalPositions[leg], Time.deltaTime * 5f);
            }
        }
    }

    /// <summary>
    /// 尾巴摇摆动画
    /// </summary>
    protected virtual void UpdateTailAnimation()
    {
        if (tailPart == null || !originalRotations.ContainsKey(tailPart)) return;

        Quaternion origRot = originalRotations[tailPart];
        float wagSpeed = isMoving ? 12f : 3f; // 移动时摇得快
        float wagAmount = isMoving ? 25f : 10f;

        float wagAngle = Mathf.Sin(animTime * wagSpeed) * wagAmount;
        tailPart.localRotation = origRot * Quaternion.Euler(0, wagAngle, 0);
    }

    /// <summary>
    /// 平滑转向面朝移动方向
    /// </summary>
    protected virtual void UpdateRotation()
    {
        if (isMoving && moveDirection.sqrMagnitude > 0.01f)
        {
            targetRotationY = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        }

        // 平滑旋转
        currentRotationY = Mathf.LerpAngle(currentRotationY, targetRotationY, Time.deltaTime * 5f);
        transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
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

        // 起跳前压扁 (蓄力)
        float squashTime = 0.1f;
        float elapsed = 0f;
        while (elapsed < squashTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / squashTime;
            // 压扁：X和Z变宽，Y变矮
            transform.localScale = new Vector3(
                originalScale.x * (1f + t * 0.2f),
                originalScale.y * (1f - t * 0.3f),
                originalScale.z * (1f + t * 0.2f)
            );
            yield return null;
        }

        verticalVelocity = jumpForce; // 设置初始向上速度
        float jumpStartY = transform.position.y;
        float minJumpHeight = 0.3f; // 至少跳这么高才检测落地
        bool reachedMinHeight = false;

        // 跳跃中拉伸
        while (true)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;

            // 检查是否达到最小跳跃高度
            if (transform.position.y > jumpStartY + minJumpHeight)
            {
                reachedMinHeight = true;
            }

            // 根据垂直速度调整形状
            float stretchFactor = Mathf.Clamp(verticalVelocity / jumpForce, -1f, 1f);
            if (stretchFactor > 0)
            {
                // 上升时拉伸
                transform.localScale = new Vector3(
                    originalScale.x * (1f - stretchFactor * 0.15f),
                    originalScale.y * (1f + stretchFactor * 0.25f),
                    originalScale.z * (1f - stretchFactor * 0.15f)
                );
            }
            else
            {
                // 下落时轻微压扁
                transform.localScale = new Vector3(
                    originalScale.x * (1f - stretchFactor * 0.1f),
                    originalScale.y * (1f + stretchFactor * 0.15f),
                    originalScale.z * (1f - stretchFactor * 0.1f)
                );
            }

            // 只有在达到最小高度后且开始下落时才检测落地
            if (reachedMinHeight && verticalVelocity < 0)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 0.6f))
                {
                    transform.position = new Vector3(
                        transform.position.x,
                        hit.point.y,
                        transform.position.z
                    );
                    break;
                }
            }

            // 安全检查：防止无限下落
            if (transform.position.y < jumpStartY - 10f)
            {
                transform.position = new Vector3(transform.position.x, jumpStartY, transform.position.z);
                break;
            }

            yield return null;
        }

        // 着陆压扁效果
        elapsed = 0f;
        float landSquashTime = 0.15f;
        while (elapsed < landSquashTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / landSquashTime;
            // 先压扁再恢复
            float squash = Mathf.Sin(t * Mathf.PI) * 0.25f;
            transform.localScale = new Vector3(
                originalScale.x * (1f + squash),
                originalScale.y * (1f - squash),
                originalScale.z * (1f + squash)
            );
            yield return null;
        }

        transform.localScale = originalScale;
        verticalVelocity = 0;
        isJumping = false;
    }

    /// <summary>
    /// 特殊动作 - 可以被子类重写
    /// </summary>
    protected virtual void PlaySpecialAction()
    {
        // 不同动物可以有不同的特殊动作
        switch (animalType)
        {
            case AnimalType.Rabbit:
                // 兔子：快速跳跃
                if (!isJumping) StartJump();
                break;
            case AnimalType.Chicken:
                // 鸡：扑腾翅膀
                StartCoroutine(FlapWings());
                break;
            case AnimalType.Dog:
                // 狗：摇尾巴更快
                StartCoroutine(ExcitedTailWag());
                break;
            default:
                break;
        }
    }

    protected IEnumerator FlapWings()
    {
        // 简单的翅膀扑腾效果 - 整体上下抖动
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float flap = Mathf.Sin(elapsed * 30f) * 0.05f;
            transform.position += Vector3.up * flap;
            yield return null;
        }
    }

    protected IEnumerator ExcitedTailWag()
    {
        // 兴奋时尾巴摇得更快
        float originalAnimSpeed = animationSpeed;
        animationSpeed = 3f;
        yield return new WaitForSeconds(2f);
        animationSpeed = originalAnimSpeed;
    }
}
