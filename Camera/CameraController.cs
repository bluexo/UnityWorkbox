using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Arthas.Client;

public class CameraController : MonoBehaviour
{
    public Camera GameCamera { get; private set; }
    public Transform target;

    public Vector4 Edge { get; private set; }

    [SerializeField]
    private float offset = 0.2f;
    [SerializeField]
    private float smoothTime = 0.2f;
    private Vector3 damp;
    private bool follow;
    [SerializeField]
    private Vector3 followOffset = new Vector3(0, 15f);

    const float CameraSlideMaxDistance = 60f;
    const float CameraMaxZoom = 35;
    const float CameraMinZoom = 15;
    const float ZoomSpeed = 0.05f;

    [Range(0, 1)]
    public float shakeDuration = .3f;
    public float shakeStrength = 2f;
    public int shakeVibrato = 20;
    public float width;
    public float height;
    [SerializeField]
    private bool first = false;

    void Awake()
    {
        GameCamera = GetComponent<Camera>();
        width = Display.main.renderingWidth / GameCamera.orthographicSize / 2;
        height = Display.main.renderingHeight / GameCamera.orthographicSize / 2;
    }

    public void Configure(Vector4 edge)
    {
        Edge = edge;
    }

    public void SetTarget(Transform obj)
    {
        target = obj;
        follow = true;
    }

    public void Shake()
    {
        transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato);
    }

    void LateUpdate()
    {
        if (first)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.touches[0];
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    var go = EventSystem.current.currentSelectedGameObject;
                    if (go && go.GetComponent<Selectable>())
                        return;
                }
            }
            if (Input.touchCount == 1)
                Slide();
            else if (first && Input.touchCount == 2)
                Zoom();
            else
                Follow();
        }
    }

    Vector3 prevTouchPosition;

    /// <summary>
    /// 滑动操作
    /// </summary>
    public void Slide()
    {
        var touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            prevTouchPosition = touch.rawPosition;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            var diffX = -(touch.position.x - prevTouchPosition.x) * Time.deltaTime;
            var diffY = -(touch.position.y - prevTouchPosition.y) * Time.deltaTime;
            var newPos = transform.position + new Vector3(diffX * 2, diffY, 0);
            var x = Mathf.Clamp(newPos.x, Edge.z, Edge.w);
            var y = Mathf.Clamp(newPos.y, Edge.x, Edge.y);
            transform.position = new Vector3(x, y, newPos.z);
            prevTouchPosition = touch.position;
        }
    }

    /// <summary>
    /// 缩放操作
    /// </summary>
    public void Zoom()
    {
        var zero = Input.GetTouch(0);
        var one = Input.GetTouch(1);
        if (EventSystem.current.IsPointerOverGameObject(zero.fingerId)
            /*|| EventSystem.current.IsPointerOverGameObject(one.fingerId)*/)
        {
            var go = EventSystem.current.currentSelectedGameObject;
            if (go && go.GetComponent<Selectable>())
                return;
        }
        var zeroPrevPos = zero.position - zero.deltaPosition;
        var onePrevPos = one.position - one.deltaPosition;
        var prevMag = (zeroPrevPos - onePrevPos).magnitude;
        var mag = (zero.position - one.position).magnitude;
        var diffMag = prevMag - mag;
        GameCamera.orthographicSize += diffMag * ZoomSpeed;
        GameCamera.orthographicSize = Mathf.Clamp(GameCamera.orthographicSize, CameraMinZoom, CameraMaxZoom);
    }

    /// <summary>
    /// 摄像机跟随
    /// </summary>
    public void Follow()
    {
        if (!target || !follow)
            return;
        var diff = transform.position - target.position - followOffset;
        if (diff.magnitude <= offset)
            return;
        var position = Vector3.SmoothDamp(transform.position, target.position + followOffset, ref damp, smoothTime);
        transform.position = new Vector3(Mathf.Clamp(position.x, Edge.z, Edge.w),
           Mathf.Clamp(position.y, Edge.x, Edge.y),
           transform.position.z);
    }
}
