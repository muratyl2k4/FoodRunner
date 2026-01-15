using UnityEngine;

public class Food : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            // Eğer engele çarparsa, StackManager'a haber ver ve bu pizzadan sonrasını sil
            StackManager.Instance.RemoveFoodFromIndex(this.gameObject);
        }
    }
}