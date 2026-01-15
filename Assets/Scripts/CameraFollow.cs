using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Takip edilecek hedef (Otomatik bulunacak)
    public Vector3 offset;   // Kamera ile hedef arasındaki mesafe farkı
    public float smoothSpeed = 5f; // Takip yumuşaklığı

    private void Start()
    {
        // Eğer hedef atanmamışsa, "Player" tag'li objeyi bul
        if (target == null)
        {
            FindTarget();
        }

        // Offset'i otomatik hesaplayalım (Editörde ayarladığın pozisyonu korur)
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate() // Kamera takibi için LateUpdate kullanılır (Titremeyi önler)
    {
        // HER KAREDE LİDERİ KONTROL ET
        // Çünkü StackManager'da lider değişmiş olabilir ama eski lider hala sahnede (DropPhysics ile) duruyor olabilir.
        if (StackManager.Instance != null && StackManager.Instance.collectedFoods.Count > 0)
        {
            Transform realLeader = StackManager.Instance.collectedFoods[0].transform;
            if (target != realLeader)
            {
                target = realLeader;
                // Hedef değiştiği an ışınlan
                if (target != null)
                {
                    transform.position = target.position + offset;
                }
            }
        }
        else if (target == null)
        {
            FindTarget();
            if (target == null) return;
        }

        // İstersek sadece Z ekseninde takip ettirebiliriz (Runner oyunları için genelde böyledir)
        // Ama tam takip için bunu kullanabilirsin:
        Vector3 desiredPosition = target.position + offset;
        
        // Sadece Z ve belki X takibi yapıp, Y yüksekliğini sabit tutmak daha sinematik olabilir.
        // Şimdilik tam takip yapıyoruz.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    private void FindTarget()
    {
        // StackManager'dan o anki lideri alabilirsek en sağlıklısı
        if (StackManager.Instance != null && StackManager.Instance.collectedFoods.Count > 0)
        {
            // Eğer hedef değişiyorsa SNAP yapmamız gerekebilir
            Transform newTarget = StackManager.Instance.collectedFoods[0].transform;
            if (target != newTarget)
            {
                target = newTarget;
            }
        }
        // Veya tag ile bul
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
}
