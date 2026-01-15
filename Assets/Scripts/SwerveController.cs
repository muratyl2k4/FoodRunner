using UnityEngine;
using UnityEngine.InputSystem; // New Input System namespace

public class SwerveController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 8f;

    [Header("Control Settings")]
    public float swerveSpeed = 10f; 
    public float maxSwerveAmount = 4.5f;

    private float _lastTouchX;
    private float _swerveDelta;

    private void Start()
    {
        // Script ilk başladığında, eğer parmak zaten basılıysa (Lider değişimi vb.)
        // LastTouchX'i şu anki pozisyona eşitle ki "0"dan fark alıp zıplamasın.
        if (Pointer.current != null && Pointer.current.press.isPressed)
        {
            _lastTouchX = Pointer.current.position.ReadValue().x;
        }
    }

    void Update()
    {
        // 1. İleri Hareket
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // 2. Input Hesaplama
        HandleInput();

        // 3. Pozisyon Uygulama
        ApplySwerve();
    }

    private void HandleInput()
    {
        if (Pointer.current == null) return;

        Vector2 pointerPos = Pointer.current.position.ReadValue();

        if (Pointer.current.press.wasPressedThisFrame)
        {
            _lastTouchX = pointerPos.x;
        }
        else if (Pointer.current.press.isPressed)
        {
            // Ekran genişliğine göre normalize edilmiş farkı al
            _swerveDelta = (pointerPos.x - _lastTouchX) / Screen.width;
            
            // Son pozisyonu güncelle ki bir sonraki karede yine farkı alabilelim
            _lastTouchX = pointerPos.x;
        }
        else
        {
            _swerveDelta = 0;
        }
    }

    private void ApplySwerve()
    {
        // swerveSpeed ile çarparak oyun dünyasındaki hareket miktarını bul
        float swerveAmount = _swerveDelta * swerveSpeed;
        
        float newX = transform.position.x + swerveAmount;
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