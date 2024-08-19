using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("玩家输入数据")]
    private float horizontal;
    private float vertical;
    private bool isGrounded = true;
    private float moveSpeed = 2f;
    private float angleYY;
    private float rotateSpeed = 5f;
    private bool isAiming;
    private float fireTime = 0.0f;
    private float mouseX;

    public void InitPlayerDataNet() {
        
    }
    
    private void Update() {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        
        
        if (horizontal != 0 || vertical != 0) {
            PlayerController_Client.Instance.Move(horizontal, vertical);
            PlayerController_Client.Instance.AnimRot(horizontal, vertical);
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (Cursor.visible) {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            } else {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            isGrounded = !isGrounded;
            PlayerController_Client.Instance.AnimSetBool("isCrouch", isGrounded);
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            PlayerController_Client.Instance.AnimSetBool("isJump", true);
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)) {
            PlayerController_Client.Instance.PlayerAttack();
        }

        if (Input.GetMouseButtonDown(1)) {
            PlayerController_Client.Instance.CharacterAim();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            PlayerController_Client.Instance.AnimSetBool("isReload", true);
        }

        PlayerController_Client.Instance.CharacterRotate(mouseX);
    }
}
