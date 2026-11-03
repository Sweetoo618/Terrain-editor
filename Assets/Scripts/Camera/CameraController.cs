using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 100.0f;
    private float _baseMoveSpeed = 100.0f;
    public float rotationSpeed = 300.0f;
    public float verticalSpeed = 100.0f;
    public float smoothTime = 0.1f;
    public bool enableSmoothing = true;
    public bool enableSpeedAdjustment = true;

    [Header("Performance Settings")]
    [Tooltip("距离检测更新间隔（秒）")]
    public float distanceCheckInterval = 0.5f;

    private Vector3 _velocity;
    private Vector3 _targetPosition;
    private float _yRotation;
    private float _xRotation;
    private Quaternion _targetRotation;
    private float _lastDistanceCheckTime;
    private float _currentDistanceToGround = 100f;
    private float _currentSpeedMultiplier = 1f;

    public void SetMoveSpeed(float speed)
    {
        _baseMoveSpeed = Mathf.Max(10.0f, speed);
        moveSpeed = _baseMoveSpeed;
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(50.0f, speed);
    }

    public void SetVerticalSpeed(float speed)
    {
        verticalSpeed = Mathf.Max(10.0f, speed);
    }

    public float GetBaseMoveSpeed()
    {
        return _baseMoveSpeed;
    }

    void Start()
    {
        _targetPosition = transform.position;
        Vector3 angles = transform.eulerAngles;
        _yRotation = angles.y;
        _xRotation = angles.x;
        _targetRotation = transform.rotation;
    }

    void Update()
    {
        HandleMovementInput();
        HandleRotation();

        if (enableSmoothing)
        {
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, smoothTime);
        }
        else
        {
            transform.position = _targetPosition;
        }
    }

    void HandleMovementInput()
    {
        float forwardInput = Input.GetAxis("Vertical");
        if (Mathf.Abs(forwardInput) > 0.1f)
        {
            _targetPosition += transform.forward * forwardInput * moveSpeed * Time.deltaTime;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            _targetPosition += transform.right * horizontalInput * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            _targetPosition += Vector3.up * verticalSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            _targetPosition += Vector3.down * verticalSpeed * Time.deltaTime;
        }

        if (enableSpeedAdjustment)
        {
            AdjustSpeedBasedOnHeight();
        }
    }

    void AdjustSpeedBasedOnHeight()
    {
        if (Time.time - _lastDistanceCheckTime > distanceCheckInterval)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1000f))
            {
                _currentDistanceToGround = hit.distance;
            }
            _lastDistanceCheckTime = Time.time;

            _currentSpeedMultiplier = Mathf.Clamp(_currentDistanceToGround / 50f, 0.5f, 2.0f);
        }

        moveSpeed = Mathf.Lerp(moveSpeed, _baseMoveSpeed * _currentSpeedMultiplier, Time.deltaTime * 2f);
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (Mathf.Abs(mouseX) > 0.1f || Mathf.Abs(mouseY) > 0.1f)
            {
                _yRotation += mouseX * rotationSpeed * Time.deltaTime;
                _xRotation -= mouseY * rotationSpeed * Time.deltaTime;
                _xRotation = Mathf.Clamp(_xRotation, -90, 90);
                _targetRotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            }
        }

        if (enableSmoothing)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, smoothTime * 10f);
        }
        else
        {
            transform.rotation = _targetRotation;
        }
    }
}