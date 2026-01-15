using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class StackManager : MonoBehaviour
{
    public static StackManager Instance;

    public List<GameObject> collectedFoods = new List<GameObject>();
    public float distanceBetweenObjects = 0.5f;

    [Header("Organik Takip Ayarları")]
    public float followSpeedX = 20f; 
    public float followSpeedY = 40f; 

    [Header("Yaylanma (Spring) Ayarları")]
    [Tooltip("Sallanma Şiddeti (150-250 ideal)")]
    public float stiffness = 180f; 
    [Tooltip("Sönümleme: Düşükse çok sallanır, yüksekse hemen durur (0.4 - 0.7 ideal)")]
    public float damping = 0.5f; 
    [Tooltip("Harekete tepki şiddeti")]
    public float movementImpact = 1.5f;

    // Sallanma verilerini tutan listeler (Hata almamak için isimler senkronize edildi)
    private List<float> rotVelocities = new List<float>();
    private List<float> currentZAround = new List<float>();

    [Header("Gate Settings")]
    public GameObject foodPrefab;
    public int spawnBatchSize = 4;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // İlk objeyi (Player) listelere ekle
        if (collectedFoods.Count == 0)
        {
            collectedFoods.Add(gameObject);
            rotVelocities.Add(0f);
            currentZAround.Add(0f);
        }
    }

    void Update()
    {
        ApplySpringPhysics();
    }

    private void ApplySpringPhysics()
    {
        // 1. indeksten başla (0 oyuncunun kendisi)
        for (int i = 1; i < collectedFoods.Count; i++)
        {
            GameObject currentFood = collectedFoods[i];
            GameObject prevFood = collectedFoods[i - 1];

            // 1. POZİSYON TAKİBİ
            Vector3 targetPos = prevFood.transform.position;
            targetPos.y += distanceBetweenObjects;

            float nx = Mathf.Lerp(currentFood.transform.position.x, prevFood.transform.position.x, followSpeedX * Time.deltaTime);
            float ny = Mathf.Lerp(currentFood.transform.position.y, targetPos.y, followSpeedY * Time.deltaTime);
            currentFood.transform.position = new Vector3(nx, ny, targetPos.z);

            // 2. YAYLANMA (BOZUK PARA EFEKTİ)
            // Sağa sola gidiş miktarını hesapla
            float movementDeltaX = (currentFood.transform.position.x - prevFood.transform.position.x);
            
            // Yay Kuvveti Formülü
            float force = -stiffness * currentZAround[i]; // Merkeze çekme
            force -= (movementDeltaX * movementImpact * stiffness); // Hareket itkisi

            // İvme -> Hız -> Konum geçişi
            rotVelocities[i] += force * Time.deltaTime;
            rotVelocities[i] = Mathf.Lerp(rotVelocities[i], 0, damping * Time.deltaTime * 10f);
            currentZAround[i] += rotVelocities[i] * Time.deltaTime;

            // Kısıtlama ve Uygulama
            currentZAround[i] = Mathf.Clamp(currentZAround[i], -50f, 50f);
            currentFood.transform.rotation = Quaternion.Euler(0, 0, currentZAround[i]);
        }
    }

public void AddFood(GameObject food)
{
    collectedFoods.Add(food);
    rotVelocities.Add(0f);
    currentZAround.Add(0f);

    food.transform.parent = null; 
    SetupPhysics(food);
    
    // --- ÖNEMLİ: Prefabın orijinal scale değerini alıyoruz ---
    // Eğer yemeğin hala büyük geliyorsa, Project ekranındaki Prefab'ın scale'ini kontrol et!
    Vector3 originalPrefabScale = food.transform.localScale; 
    
    food.transform.localScale = Vector3.zero;
    StartCoroutine(PopAnimation(food.transform, originalPrefabScale));
}

private IEnumerator PopAnimation(Transform t, Vector3 targetScale)
{
    float elapsed = 0;
    float duration = 0.2f; // Animasyon süresi

    while (elapsed < duration)
    {
        if (t == null) yield break;
        elapsed += Time.deltaTime;
        float percent = elapsed / duration;

        // "Pıt" Efekti Formülü: 0 -> 1.2 (punch) -> 1.0 (target)
        // Animasyon eğrisi: Önce hızla büyür, hedefi biraz geçer, sonra yerine oturur.
        float curve = Mathf.Sin(percent * Mathf.PI * 1.2f); 
        t.localScale = Vector3.Lerp(Vector3.zero, targetScale * 1.2f, curve);
        
        yield return null;
    }
    
    if (t != null) t.localScale = targetScale; // Tam boyutta sabitle
}

    private void SetupPhysics(GameObject food)
    {
        if (food.TryGetComponent<Collider>(out Collider col)) col.isTrigger = true;
        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb == null) rb = food.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.None;
    }



    public void OnGateHit(GateType type, int value)
    {
        int amountToAdd = 0;
        if (type == GateType.Addition) amountToAdd = value;
        else if (type == GateType.Multiplication) amountToAdd = (collectedFoods.Count * value) - collectedFoods.Count;
        else if (type == GateType.Subtraction) amountToAdd = -value;
        
        
        if (amountToAdd > 0) StartCoroutine(AddFoodsSequentially(amountToAdd));
        else if (amountToAdd < 0) StartCoroutine(SubstractFoodsSequentially(-amountToAdd));

    }

    public IEnumerator AddFoodsSequentially(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            CreateNewFood();
            if (i % spawnBatchSize == 0) yield return null;
        }
    }

    public IEnumerator SubstractFoodsSequentially(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SubstractFood(1);
            if (i % spawnBatchSize == 0) yield return null;
        }
    }

    private void CreateNewFood()
    {
        GameObject newFood = Instantiate(foodPrefab);
        Vector3 lastPos = collectedFoods[collectedFoods.Count - 1].transform.position;
        // if (collectedFoods.Count == 1)
        // else 
        //     newFood.transform.position = new Vector3(lastPos.x, lastPos.y + foodPrefab.transform.localScale.y /gameObject.transform.localScale.y, lastPos.z);
        newFood.transform.position = new Vector3(lastPos.x, lastPos.y + distanceBetweenObjects, lastPos.z);
        
                
        AddFood(newFood);
    }
    //TODO OPTIMIZE EDILSIN SUBSTRACT ILE REMOVEFODUFROMINDEX BENZER IS YAPIYOR
    private void SubstractFood(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (collectedFoods.Count > 1)
            {
                int lastIndex = collectedFoods.Count - 1;
                GameObject foodToRemove = collectedFoods[lastIndex];

                // Tüm listelerden aynı anda temizle 
                collectedFoods.RemoveAt(lastIndex);
                rotVelocities.RemoveAt(lastIndex);
                currentZAround.RemoveAt(lastIndex);

                DropFoodPhysics(foodToRemove);
            }
        }
    }

    public void RemoveFoodFromIndex(GameObject hitFood)
    {
        int index = collectedFoods.IndexOf(hitFood);
        if (index > 0)
        {
            int countToRemove = collectedFoods.Count - index;
            for (int i = 0; i < countToRemove; i++)
            {
                int lastIndex = collectedFoods.Count - 1;
                GameObject foodToRemove = collectedFoods[lastIndex];

                // Tüm listelerden aynı anda temizle
                collectedFoods.RemoveAt(lastIndex);
                rotVelocities.RemoveAt(lastIndex);
                currentZAround.RemoveAt(lastIndex);

                DropFoodPhysics(foodToRemove);
            }
        }
    }

    private void DropFoodPhysics(GameObject food)
    {
        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(new Vector3(UnityEngine.Random.Range(-3, 3), 5, -3), ForceMode.Impulse);
            rb.AddTorque(new Vector3(UnityEngine.Random.Range(-10, 10), 0, UnityEngine.Random.Range(-10, 10)), ForceMode.Impulse);
        }
        if (food.TryGetComponent<Collider>(out Collider col)) col.isTrigger = false;
        Destroy(food, 2f);
    }
}