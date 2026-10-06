using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("巡逻")]
    public Transform leftPoint;
    public Transform rightPoint;
    public float patrolSpeed = 2f;

    [Header("仇恨")]
    public float detectionRange = 5f;
    public float chaseSpeed = 3f;
    public float loseTargetRange = 8f;

    private Rigidbody2D rb;
    private Transform player;
    private bool chasing;
    private bool movingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FixedUpdate()
    {
        // 如果主角被删除、死亡或场景刚加载，重新查找
        if (player == null)
        {
            FindPlayer();

            // 当前没有带 Player 标签的对象时，敌人继续巡逻
            if (player == null)
            {
                chasing = false;
                Patrol();
                return;
            }
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (!chasing && distance <= detectionRange)
        {
            chasing = true;
        }

        if (chasing && distance > loseTargetRange)
        {
            chasing = false;
        }

        if (chasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            player = null;
        }
    }

    private void Patrol()
    {
        if (leftPoint == null || rightPoint == null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        Transform target = movingRight ? rightPoint : leftPoint;

        float direction = Mathf.Sign(
            target.position.x - transform.position.x
        );

        rb.velocity = new Vector2(
            direction * patrolSpeed,
            rb.velocity.y
        );

        FaceDirection(direction);

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.05f)
        {
            movingRight = !movingRight;
        }
    }

    private void ChasePlayer()
    {
        if (player == null)
        {
            chasing = false;
            Patrol();
            return;
        }

        float direction = Mathf.Sign(
            player.position.x - transform.position.x
        );

        rb.velocity = new Vector2(
            direction * chaseSpeed,
            rb.velocity.y
        );

        FaceDirection(direction);
    }

    private void FaceDirection(float direction)
    {
        if (direction == 0f)
        {
            return;
        }

        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x) * Mathf.Sign(direction);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            loseTargetRange
        );
    }
}