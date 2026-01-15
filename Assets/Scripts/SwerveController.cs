using UnityEngine;
using UnityEngine.InputSystem; // Yeni kütüphaneyi ekledik

public class SwerveController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 8f;

    
    public float swerveSensitivity = 0.5f;
    public float maxSwerveAmount = 4.5f;

    private float _lastTouchX;
    private float _swerveDelta;

    void Update()
    {
        // 1. İleri Hareket
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // 2. Yeni Input System ile Swerve Hesaplama
        HandleInput();

        // 3. Pozisyon Uygulama
        ApplySwerve();
    }

    private void HandleInput()
    {
        // Mouse veya Dokunmatik fark etmeksizin çalışır
        if (Pointer.current == null) return;

        Vector2 pointerPos = Pointer.current.position.ReadValue();

        if (Pointer.current.press.wasPressedThisFrame)
        {
            _lastTouchX = pointerPos.x;
        }
        else if (Pointer.current.press.isPressed)
        {
            _swerveDelta = (pointerPos.x - _lastTouchX) / Screen.width;
            _lastTouchX = pointerPos.x;
        }
        else
        {
            _swerveDelta = 0;
        }
    }

    private void ApplySwerve()
    {
        float newX = transform.position.x + (_swerveDelta * swerveSensitivity * 100f);
        newX = Mathf.Clamp(newX, -maxSwerveAmount, maxSwerveAmount);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            StackManager.Instance.AddFood(other.gameObject);
        }
    }
}