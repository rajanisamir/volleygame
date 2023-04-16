using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target = null;
    [SerializeField] float smoothSpeed = 0.125f;
    [SerializeField] Vector3 offset = new Vector3(0f, 2.25f, -1.5f);

    private Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
    }

    public void CenterOnTarget()
    {
        transform.position = transform.position + offset;
    }
}
