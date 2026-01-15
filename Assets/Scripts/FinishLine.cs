using UnityEngine;
using System.Collections;

public class FinishLine : MonoBehaviour
{
    [Header("End Game Settings")]
    public float danceDelay = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("LEVEL COMPLETED!");
            
            // 1. Oyuncunun kontrollerini kapat
            SwerveController controller = other.GetComponent<SwerveController>();
            if (controller != null)
            {
                controller.enabled = false;
                controller.forwardSpeed = 0; // Hareketi durdur
            }

            // 2. Stack içindeki tüm karakterler için kutlama yap
            StartCoroutine(CelebrateRoutine());
            
            // 3. UI veya Level Manager'ı tetikle (İleride eklenecek)
            // GameManager.Instance.LevelFixed();
        }
    }

    private IEnumerator CelebrateRoutine()
    {
        yield return new WaitForSeconds(danceDelay);

        var allFoods = StackManager.Instance.collectedFoods;
        foreach (var food in allFoods)
        {
            // Eğer animasyon varsa 'Dance' triggerını çalıştır
            Animator anim = food.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Dance");
            }
            
            // Veya basitçe kendi etrafında dönme efekti
            // food.AddComponent<Rotator>(); // Örnek
        }
    }
}
