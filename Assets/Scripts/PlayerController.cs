using System.Collections;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public enum ControlState
{
     idle,
     walking,
     running,
     attacking,
     swiping,
     jumping,
     jumpRecovery
}

public class PlayerController : MonoBehaviour
{
     public float moveSpeed = 0.7f;
     public float walkSpeed = 0.7f;
     public float runSpeed = 5f;
     public float jumpForce = 10f;
     public float gravity = -9.81f;
     public float mouseSensitivity = 50f;
     public float jumpTime = 1.0f;
     public float recovertTime = 1.0f;
     public GameObject AlienModelOrientation;

     private CharacterController controller;
     [SerializeField]
     private Animator animator;
     public Vector3 velocity;
     private bool isRunning = false;
     private bool IsOnGround => transform.position.y < 0.01f;

     public ControlState controlState = ControlState.idle;

     void Start()
     {
          controller = GetComponent<CharacterController>();
          LockCursor();
     }

     void Update()
     {
          CheckAttackStates();
          animator.ResetTrigger("Jump");
          switch (controlState)
          {
               case ControlState.idle:
               case ControlState.walking:
               case ControlState.running:
                    HandleMovement();
                    HandleAttack();
                    HandleSwipe();
                    HandleJumping();
                    break;
               case ControlState.jumping:
                    HandleMovement();
                    break;
               case ControlState.attacking:
                    HandleAttack();
                    HandleSwipe();
                    break;
               case ControlState.jumpRecovery:
               case ControlState.swiping:
                    //nothing
                    break;
          }

          MouseRotation();

          GravityTick();

          if(Input.GetKeyDown(KeyCode.Escape)) { UnlockCursor(); }
     }

     private void CheckAttackStates()
     {
          if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
          {
               controlState = ControlState.attacking;
          }
          else if(animator.GetCurrentAnimatorStateInfo(0).IsName("Swipe"))
          {
               controlState = ControlState.swiping;
          }
     }

     private void HandleSwipe()
     {
          if (Input.GetMouseButtonDown(1))
          {
               controlState = ControlState.swiping;
               animator.SetTrigger("Swipe");
               StartCoroutine(SwipeDelay());
          }
     }

     private void HandleAttack()
     {
          if(Input.GetMouseButtonDown(0))
          {
               LockCursor();
               controlState = ControlState.attacking;
               animator.SetTrigger("Attack");
               StartCoroutine(AttackDelay());
          }
     }

     private IEnumerator SwipeDelay()
     {
          yield return new WaitForSeconds(1.0f);
          yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsName("Swipe"));
          controlState = ControlState.idle;
     }

     private IEnumerator AttackDelay()
     {
          yield return new WaitForSeconds(1.0f);
          yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
          controlState = ControlState.idle;
     }

     private void HandleMovement()
     {
          float horizontal = Input.GetAxis("Horizontal");
          float vertical = Input.GetAxis("Vertical");
          isRunning = Input.GetKey(KeyCode.LeftShift);
          moveSpeed = isRunning ? runSpeed : walkSpeed;

          // Calculate movement direction based on character's rotation and input
          Vector3 direction = new Vector3();
          if (horizontal != 0)
          {
               direction += transform.right * horizontal;
          }
          if (vertical != 0)
          {
               direction += transform.forward * vertical;
          }

          if (direction.magnitude >= 0.1f)
          {
               controlState = isRunning ? ControlState.running : ControlState.walking;

               direction.Normalize();
               AlienModelOrientation.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

               controller.Move(direction * moveSpeed * Time.deltaTime);
               if(isRunning)
               {
                    animator.SetBool("IsRunning", true);
                    animator.SetBool("IsWalking", false);
               }
               else
               {
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("IsRunning", false);
               }
          }
          else
          {
               controlState = ControlState.idle;
               animator.SetBool("IsRunning", false);
               animator.SetBool("IsWalking", false);
          }
     }


     private void HandleJumping()
     {
          if (Input.GetKeyDown(KeyCode.Space) && IsOnGround)
          {
               controlState = ControlState.jumping;
               animator.SetTrigger("Jump");
               StartCoroutine(DelayJump(0.3f));
          }
     }

     private IEnumerator DelayJump(float time)
     {
          yield return new WaitForSeconds(time);
          velocity.y = jumpForce; // Set the vertical velocity directly
          yield return new WaitForSeconds(jumpTime);
          yield return JumpRecovery();
     }

     private IEnumerator JumpRecovery()
     {
          controlState = ControlState.jumpRecovery;
          yield return new WaitForSeconds(recovertTime);
          yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"));
          controlState = ControlState.idle;
     }

     private void MouseRotation()
     {
          // Get mouse input
          float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

          // Rotate the player's transform
          transform.Rotate(Vector3.up * mouseX);
     }

     private void GravityTick()
     {
          controller.Move(velocity * Time.deltaTime);
          if (!controller.isGrounded)
          {
               velocity.y += gravity * Time.deltaTime;
          }
          else
          {
               velocity.y = 0;
          }

          if(transform.position.y < -20f)
          {
               transform.position = Vector3.up * 5;
          }
     }

     void LockCursor()
     {
          Cursor.lockState = CursorLockMode.Locked;
          Cursor.visible = false;
     }

     void UnlockCursor()
     {
          Cursor.lockState = CursorLockMode.Confined;
          Cursor.visible = true;
     }
}