using Arthas.Client;
using UnityEngine;


/// <summary>
/// 2D角色控制器
/// </summary>
public class CharacterController2D : MonoBehaviour
{
    public LayerMask groundMask;
    private bool isServer;
    public const float groundCheckHeight = 7.5f;
    public Transform CheckPoint { get; private set; }
    public Vector3 checkDirection = new Vector3(2f, 4.5f);
    private bool isGround = false;

    public void Initialize(bool isServer, Transform checkPoint = null)
    {
        CheckPoint = checkPoint ?? transform;
        this.isServer = isServer;
    }

    public void FixedUpdate()
    {
#if !UNITY_EDITOR
        if (!isServer)
            return;
#endif
        var hit = Physics2D.Raycast(transform.position + Vector3.up,
            Vector3.down,
            1f,
            groundMask);
        if (isGround = !hit.collider)
        {
            transform.position += Physics.gravity * Time.fixedDeltaTime;
        }
    }

    public void Move(Vector3 move, float speed = 1f)
    {
        var sign = Mathf.Sign(move.x);
        var frontHit = Physics2D.Raycast(CheckPoint.position,
            new Vector2(checkDirection.x * sign, checkDirection.y),
            groundCheckHeight,
            groundMask);
#if UNITY_EDITOR
        Debug.DrawRay(CheckPoint.position,
            new Vector2(checkDirection.x * sign, checkDirection.y),
            Color.red,
            1f);
#endif
        if (frontHit.collider)
            return;
        var downHit = Physics2D.CircleCast(transform.position + Vector3.up * 3f,
           0.1f,
           Vector3.down,
           groundCheckHeight,
           groundMask);
#if UNITY_EDITOR
        Debug.DrawLine(transform.position + Vector3.up * 3f,
            transform.position + Vector3.down * (groundCheckHeight - 1),
            Color.green,
            1f);
#endif
        if (downHit.collider)
        {
            var moveDir = Vector3.ProjectOnPlane(new Vector3(Mathf.Sign(move.x), 0), downHit.normal);
            if (moveDir.x != 0)
            {
                var angle = Mathf.Atan(moveDir.y / moveDir.x) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, 0, angle);
            }
            transform.localScale = new Vector3(sign, 1, 1);
            transform.position = new Vector3(transform.position.x + sign * speed, downHit.point.y);
        }
    }

#if UNITY_EDITOR
    public void OnGUI()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            Move(Vector3.left);
        if (Input.GetKey(KeyCode.RightArrow))
            Move(Vector3.right);
    }
#endif
}