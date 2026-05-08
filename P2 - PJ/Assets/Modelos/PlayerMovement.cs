using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementFly : MonoBehaviour
{
    public float speed = 5f;
    public float verticalSpeed = 5f;

    private Rigidbody rb;
    private Vector3 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // importante para volar
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float z = Input.GetAxisRaw("Vertical");   // W/S

        float y = 0f;

        if (Input.GetKey(KeyCode.Space))
            y = 1f;

        if (Input.GetKey(KeyCode.LeftShift))
            y = -1f;

        movement = new Vector3(x, y, z).normalized;
    }

    void FixedUpdate()
    {
        Vector3 newPosition = rb.position + new Vector3(
            movement.x * speed,
            movement.y * verticalSpeed,
            movement.z * speed
        ) * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);

        // Rotar hacia la dirección de movimiento (solo en plano XZ)
        Vector3 horizontalMove = new Vector3(movement.x, 0, movement.z);

        if (horizontalMove != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(horizontalMove);
            rb.MoveRotation(rot);
        }
    }
}
