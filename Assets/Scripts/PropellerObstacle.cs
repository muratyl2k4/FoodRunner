using UnityEngine;

public class PropellerObstacle : BaseObstacle
{
    [Header("Propeller Settings")]
    public Vector3 rotationAxis = Vector3.forward; // Pervane genelde Z ekseninde döner (karşıdan bakınca)
    public float rotationSpeed = 300f;
    
    [Header("Intermittent Settings (Optional)")]
    public bool isIntermittent = false; // Dur-Kalk yapsın mı?
    public float activeTime = 2f;
    public float idleTime = 1f;

    private float timer;
    private bool isSpinning = true;

    private void Update()
    {
        if (isIntermittent)
        {
            timer += Time.deltaTime;
            if (isSpinning && timer > activeTime)
            {
                isSpinning = false;
                timer = 0;
            }
            else if (!isSpinning && timer > idleTime)
            {
                isSpinning = true;
                timer = 0;
            }
        }

        if (isSpinning)
        {
            // Hızlanma ve yavaşlama efekti (Lerp) eklenebilir ama şimdilik sabit hız yeterli
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        }
    }
}
