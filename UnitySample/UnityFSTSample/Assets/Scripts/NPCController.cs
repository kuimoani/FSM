using Dogfoot.AI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCController : MonoBehaviour
{
    [Header("Basic Character States")]
    public GameObject player;

    [Header("Patrol Character States")]
    public Transform[] waypoints;
    private int currentWayPoint;
    private readonly float waitingSecMAX = 3.0f;
    private float waitingSec = 0.0f;

    private TextMesh txtThought;
    private FST fst;

    [SerializeField]
    public float tickRate = 0.1f;

    void Start()
    {
        txtThought = GetComponentInChildren<TextMesh>();

        fst = new FST(
            new FSTNode("Battle")
                .Think(_ =>
                {
                    if (Vector2.Distance(this.transform.position, player.transform.position) < 0.75f)
                        return true;
                    return false;
                })
                .AddChildren(
                    new FSTNode("MoveTo")
                        .Think(_ => {
                            return Vector2.Distance(this.transform.position, player.transform.position) > 0.2f;
                        })
                        .Act(_ => { MoveTo(player, 1.0f); })
                        .EndAct(_ => { this.GetComponent<Rigidbody2D>().velocity = new Vector2(); }),
                    new FSTNode("Attack")
                        .Think(_ => {
                            return true;
                        })
                ),
            new FSTNode("Patrol")
                .AddChildren(
                    new FSTNode("MoveTo")
                        .Think(_ => {
                            return Vector2.Distance(this.transform.position, waypoints[currentWayPoint].position) > 0.2f;
                        })
                        .Act(_ => { MoveTo(waypoints[currentWayPoint].gameObject, 0.5f); })
                        .EndAct(_ => { this.GetComponent<Rigidbody2D>().velocity = new Vector2(); }),
                    new FSTNode("IdleFor")
                        .Think(this.IdleFor),
                    new FSTNode("ChangeDest")
                        .Think(_ =>
                        {
                            currentWayPoint = (currentWayPoint + 1) % waypoints.Length;
                            return true;
                        })
                )
        );

        InvokeRepeating("Tick", 0, tickRate);
    }

    public void Tick()
    {
        fst.Think();
        txtThought.text = fst.GetCurrentNodePath();
    }

    public void FixedUpdate()
    {
        
    }

    public void MoveTo(GameObject target, float speed)
    {
        Vector2 moveDir = target.transform.position - this.transform.position;
        this.GetComponent<Rigidbody2D>().velocity = moveDir.normalized * speed;
    }

    public bool IdleFor(FSTNode self)
    {
        waitingSec += tickRate;
        if (waitingSec > waitingSecMAX)
        {
            waitingSec = 0;
            return false;
        }
        return true;
    }
}
