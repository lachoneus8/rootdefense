using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LadybugController : MonoBehaviour
{
    public float speed;
    public float turnSpeed;

    public float jumpSpeed;

    public float arrowBoost;

    public Transform carriedItemPosition;

    public GameController gameController;

    private GameObject currentCarriedObject = null;
    private bool carriedObjectIsWater = false;

    private bool attackingTarget = false;

    public bool SetCarriedItem(GameObject carriedObject, bool isWater)
    {
        if (currentCarriedObject != null)
        {
            return false;
        }
        carriedObject.transform.SetParent(carriedItemPosition, false);
        currentCarriedObject = carriedObject;
        carriedObjectIsWater = isWater;

        return true;
    }

    public bool IsCarryingItem()
    {
        return currentCarriedObject != null;
    }

    public void RemoveCarriedItem(out bool wasWater)
    {
        wasWater = carriedObjectIsWater;
        Destroy(currentCarriedObject);
        currentCarriedObject = null;
    }

    public void AttackTarget(AphidController aphidController)
    {
        if (!attackingTarget)
        {
            attackingTarget = true;

            StartCoroutine(AttackTargetCoroutine(aphidController));
        }
    }

    private IEnumerator AttackTargetCoroutine(AphidController aphidController)
    {
        var targetPosition = aphidController.transform.position;
        targetPosition.y += .7f;
        transform.position = targetPosition;

        var bottom = targetPosition.y;
        var top = bottom + .8f;

        for (int i = 0; i < 5; ++i)
        {
            // Hop up
            while (transform.position.y < top)
            {
                var curPos = transform.position;
                curPos.y += 5 * Time.deltaTime;
                transform.position = curPos;

                yield return null;
            }

            // Hop down
            while (transform.position.y > bottom)
            {
                var curPos = transform.position;
                curPos.y -= 5 * Time.deltaTime;
                transform.position = curPos;

                yield return null;
            }
        }

        attackingTarget = false;

        gameController.RemoveInteractable(aphidController);
        Destroy(aphidController.gameObject);
    }

    private float ySpeed;

    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (attackingTarget)
        {
            return;
        }

        if (gameController.IsGameOver())
        {
            return;
        }

        ySpeed += Physics.gravity.y * Time.deltaTime;

        if (characterController.isGrounded)
        {
            ySpeed = -0.5f;
            if (Keyboard.current.spaceKey.isPressed)
            {
                ySpeed = jumpSpeed;
            }
        }

        var movement = new Vector3();

        if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
        {
            movement = transform.forward * speed;
        }

        if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
        {
            movement = -transform.forward * speed;
        }

        var deltaX = Mouse.current.delta.x.ReadValue();
        var curMouseRot = transform.rotation.eulerAngles;
        curMouseRot.y += deltaX * turnSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(curMouseRot);

        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
        {
            var curRot = transform.rotation.eulerAngles;

            curRot.y -= turnSpeed * Time.deltaTime * arrowBoost;

            transform.rotation = Quaternion.Euler(curRot);
        }


        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            var curRot = transform.rotation.eulerAngles;

            curRot.y += turnSpeed * Time.deltaTime * arrowBoost;

            transform.rotation = Quaternion.Euler(curRot);
        }

        movement.y = ySpeed;
        characterController.Move(movement * Time.deltaTime);
    }
}
