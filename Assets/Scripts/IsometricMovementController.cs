using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricMovementController : MonoBehaviour
{

    public float movementSpeed = 1f;
    IsometricCharacterRenderer isoRenderer;
    Vector2 destination;

    Rigidbody2D rbody;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        destination = rbody.position;
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
    }

    public void SetDestination(Vector2 destination)
    {
        this.destination = destination;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 currentPos = rbody.position;
        // float horizontalInput = Input.GetAxis("Horizontal");
        // float verticalInput = Input.GetAxis("Vertical");
        Vector2 inputVector = (destination - currentPos);
        if (inputVector.magnitude < 0.05f) {
            isoRenderer.SetDirection(Vector2.zero);
            return;
        }
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        Vector2 movement = inputVector * movementSpeed;
        Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        isoRenderer.SetDirection(movement);
        rbody.MovePosition(newPos);
    }
}