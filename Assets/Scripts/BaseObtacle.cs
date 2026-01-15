using UnityEngine;


public enum DamageBehavior
{
    SacrificeBottom, // Alttan eksilt (Ground Saw gibi)
    SliceFromHit     // Çarptığı yerden yukarısını kes (Balyoz gibi)
}

public abstract class BaseObstacle : MonoBehaviour
{
    public int damageAmount = 1; 
    public DamageBehavior behavior = DamageBehavior.SacrificeBottom; // Varsayılan: Alttan eksilt

    private void OnTriggerEnter(Collider other)
    {
        // Player (Lider) ve Food (Takipçiler) için kontrol
        if (other.CompareTag("Food") || other.CompareTag("Player")) 
        {
            HandleCollision(other.gameObject);
        }
    }

    protected virtual void HandleCollision(GameObject member)
    {
        // StackManager'a hasar türünü de gönderiyoruz
        StackManager.Instance.RemoveFoodFromIndex(member, behavior);
    }
}