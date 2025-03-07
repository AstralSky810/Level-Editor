using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Animator animator;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        ApplyGravity();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // 计算目标旋转角度
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationSpeed,
                0.1f);

            transform.rotation = Quaternion.Euler(0, angle, 0);

            // 移动角色
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            animator.SetFloat("Speed", moveDirection.magnitude);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }

    void ApplyGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            Block block = other.GetComponent<Block>();
            if (block != null && !InventorySystem.Instance.IsFull())
            {
                InventorySystem.Instance.AddItem(block.data);
                Destroy(other.gameObject);
            }
        }
    }
}
