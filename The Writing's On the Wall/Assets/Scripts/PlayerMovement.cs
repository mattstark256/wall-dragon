using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Transform head;
    [SerializeField]
    private float mouseSensitivity = 3;
    [SerializeField]
    private float maxAcceleration = 3;
    [SerializeField]
    private float maxSpeed = 3;

    private float yaw;
    private float pitch;

    private Vector3 moveVelocity = Vector3.zero;

    private float storedYaw;
    private float storedPitch;
    private Vector3 storedPosition;
    private bool coroutineInProgress = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = head.eulerAngles.y;
        pitch = head.eulerAngles.x;
    }


    void Update()
    {
        if (coroutineInProgress) return;

        Vector2 mouseVector = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        mouseVector *= mouseSensitivity;
        yaw += mouseVector.x;
        pitch -= mouseVector.y;
        head.rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 targetVelocity = head.rotation * inputVector.normalized * maxSpeed;

        Vector3 amountToAccelerate = targetVelocity - moveVelocity;
        if (amountToAccelerate.magnitude > maxAcceleration * Time.deltaTime) { amountToAccelerate = amountToAccelerate.normalized * maxAcceleration * Time.deltaTime; }
        moveVelocity += amountToAccelerate;

        transform.position += moveVelocity * Time.deltaTime;
    }


    public void StoreState()
    {
        storedYaw = yaw;
        storedPitch = pitch;
        storedPosition = transform.position;
    }


    public void MoveToStoredState(float duration) { StartCoroutine(MoveToStoredStateCoroutine(duration)); }
    private IEnumerator MoveToStoredStateCoroutine(float duration)
    {
        coroutineInProgress = true;

        float initialYaw = yaw;
        float initialPitch = pitch;
        Vector3 initialPosition = transform.position;

        // Make it take the shortest route
        storedYaw = initialYaw + (storedYaw - initialYaw + 360 * 10 + 180) % 360 - 180;

        float f = 0;
        while (f < 1)
        {
            f += Time.deltaTime / duration;
            if (f > 1) f = 1;
            float smoothedF = Mathf.SmoothStep(0, 1, f);

            transform.position = Vector3.Lerp(initialPosition, storedPosition, smoothedF);
            yaw = Mathf.Lerp(initialYaw, storedYaw, smoothedF);
            pitch = Mathf.Lerp(initialPitch, storedPitch, smoothedF);
            head.rotation = Quaternion.Euler(pitch, yaw, 0);

            yield return null;
        }

        coroutineInProgress = false;
        moveVelocity = Vector3.zero;
    }
}
