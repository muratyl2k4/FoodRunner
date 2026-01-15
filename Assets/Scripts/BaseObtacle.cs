using UnityEngine;


public abstract class BaseObstacle : MonoBehaviour
{
    public int damageAmount = 1; // Kaç karakter öldüreceği

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Member")) // Kalabalıktaki her bir birime çarptığında
        {
            HandleCollision(other.gameObject);
        }
    }

    protected virtual void HandleCollision(GameObject member)
    {
        // StackManager'a bu karakterin öldüğünü bildir
        StackManager.Instance.RemoveFoodFromIndex(member);
        
        // Cartoon efekti: Karakteri patlat (vfx) ve yok et
        Destroy(member);
    }
}