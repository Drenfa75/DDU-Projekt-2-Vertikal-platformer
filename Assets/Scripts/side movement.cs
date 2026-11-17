using UnityEngine;

public class LeftRightMover : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float distance = 4f; // how far to travel from the start position in each direction

    private Vector3 startPos;
    private int direction = -1; // 1 = right, -1 = left

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        // Flip direction when reaching either edge
        if (transform.position.x >= startPos.x + distance)
            direction = -1;
        else if (transform.position.x <= startPos.x - distance)
            direction = 1;
    }
}