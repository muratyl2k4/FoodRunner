using UnityEngine;

public class RotatingSaw : BaseObstacle
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up; // Varsayılan olarak Y ekseninde döner
    public float rotationSpeed = 180f;

    [Header("Migration Settings (Optional)")]
    public bool isMoving = false; // Hareket etsin mi?
    public float moveSpeed = 2f;
    public float moveRange = 2f; // Ne kadar uzağa gitsin

    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        // 1. Dönme Hareketi
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        // 2. Hareket (Devriye) Mantığı
        if (isMoving)
        {
            // Sinüs dalgası -1 ile 1 arasında gider gelir, bunu range ile çarpıyoruz
            float offset = Mathf.Sin(Time.time * moveSpeed) * moveRange;
            
            // Sadece X ekseninde hareket ettiriyoruz (Yana doğru)
            // İstersen bunu Z veya Y için de modifiye edebilirsin
            transform.position = new Vector3(initialPosition.x + offset, transform.position.y, transform.position.z);
        }
    }
}
