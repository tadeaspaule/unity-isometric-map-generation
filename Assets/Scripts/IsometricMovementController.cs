using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricMovementController : MonoBehaviour
{

    public float movementSpeed = 1f;
    IsometricCharacterRenderer isoRenderer;
    Vector2 destination;

    Rigidbody2D rbody;

    List<Vector3> path = new List<Vector3>();

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

    public void SetRoomDestination(Vector3 destination)
    {
        path = Pathfinder.GetPath(rbody.position, destination, null);
        SetDestination(path[0]);
        path.RemoveAt(0);
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
            if (path.Count > 0) {
                SetDestination(path[0]);
                path.RemoveAt(0);
            }
            return;
        }
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        Vector2 movement = inputVector * movementSpeed;
        Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        isoRenderer.SetDirection(movement);
        rbody.MovePosition(newPos);
    }
}
