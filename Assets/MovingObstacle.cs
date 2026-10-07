using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private RectTransform ObjectToMove;
    [SerializeField] private RectTransform effect;
    [SerializeField] private RectTransform targetEffect;
    [SerializeField] private float speed = 1f;

    private bool isMoving = false;

    [SerializeField] private ObjectivesManager om;
    private bool isCompleted=false;

    void Update()
    {
        if (isMoving)
        {
            Move();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!isCompleted)
            {
                om.UpdateObjective();
                isCompleted = true;
            }
            isMoving = true;
        }
    }

    public void Move()
    {
        ObjectToMove.position = Vector3.MoveTowards(
            ObjectToMove.position,
            target.position,
            speed * Time.deltaTime
        );

        effect.position = Vector3.MoveTowards(
            effect.position,
            targetEffect.position,
            speed * Time.deltaTime
        );

        if (ObjectToMove.position == target.position &&
            effect.position == targetEffect.position)
        {
            isMoving = false;
        }
    }
}
