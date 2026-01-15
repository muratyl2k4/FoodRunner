using UnityEngine;

public class SwingingHammer : BaseObstacle
{
    [Header("Swing Settings")]
    public float swingSpeed = 2f;
    public float swingAngle = 75f; // Sağa sola kaç derece açılsın
    public float startDelay = 0f; // Balyozlar senkronize olmasın diye gecikme
    
    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.rotation;
    }

    private void Update()
    {
        // Sinüs dalgası ile -1 ile 1 arasında gidip geliyoruz
        // startDelay sayesinde her balyoz farklı zamanda sallanabilir
        float angle = Mathf.Sin((Time.time + startDelay) * swingSpeed) * swingAngle;

        // Z ekseninde (veya balyozun asılı olduğu eksende) sallıyoruz
        // Eğer balyozun pivotu (merkezi) en üst noktadaysa, sarkaç gibi sallanır.
        // Pivot ortadaysa tahterevalli gibi sallanır. Modeli buna göre ayarlamalısın.
        transform.rotation = startRotation * Quaternion.Euler(0, 0, angle);
    }
}
