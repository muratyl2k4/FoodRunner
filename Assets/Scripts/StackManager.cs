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
        
        // İlk objeyi (PlayerHolder) bul ve listeye ekle
        // ARTIK KENDİNİ EKLEMİYOR, SAHNEDEKİ PLAYER'I BULUYOR!
        if (collectedFoods.Count == 0)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                collectedFoods.Add(player);
                rotVelocities.Add(0f);
                currentZAround.Add(0f);
            }
            else
            {
                Debug.LogError("StackManager: 'Player' tag'ine sahip obje bulunamadı! Lütfen PlayerHolder'a 'Player' tagi verin.");
            }
        }
    }

    [Header("Game Settings")]
    public int initialStackSize = 3; // Başlangıçta kaç pizza olsun?

    private void Start()
    {
        // Başlangıç stack'ini oluştur
        StartCoroutine(InitializeStack());
    }

    private IEnumerator InitializeStack()
    {
        // PlayerHolder'ın sahneye yerleşmesi için 1 kare bekle
        yield return null;

        for (int i = 0; i < initialStackSize; i++)
        {
            CreateNewFood();
            yield return null; // Hepsini aynı karede değil, sırayla ekleyelim ki pozisyonları karışmasın
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
            // GÜVENLİK KONTROLÜ: Eğer listedeki obje yok olmuşsa (null ise) işlemi atla
            // Bu durum, Destroy gerçekleştiği frame ile listenin güncellendiği an arasındaki milisaniyelik farklarda oluşabilir.
            if (collectedFoods[i] == null || collectedFoods[i - 1] == null) 
            {
               continue; 
            }

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
            
            // Eğer listeler senkronizasyonunu kaybettiyse (Index Out of Range hatası almamak için)
            if (i >= currentZAround.Count || i >= rotVelocities.Count) continue;

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
        if (collectedFoods.Count == 0 || collectedFoods[0] == null) return;

        GameObject lastFood = collectedFoods[collectedFoods.Count - 1];
        
        // Eğer sahnede silinmiş bir objeye erişmeye çalışıyorsak hata vermesin diye kontrol
        if (lastFood == null)
        {
             // Eğer son obje bir şekilde yoksa, listeyi temizle veya pas geç
             return; 
        }

        GameObject newFood = Instantiate(foodPrefab);
        Vector3 lastPos = lastFood.transform.position;
        
        // Yeni objeyi, son objenin Y ekseninde "distanceBetweenObjects" kadar üzerine koy
        // Bu sayede kule mantığı devam eder
        newFood.transform.position = new Vector3(lastPos.x, lastPos.y + distanceBetweenObjects, lastPos.z);
        
        // Yeni objenin parent'ını null yap ki manager'ın altına girmesin
        newFood.transform.parent = null;
                
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

    private float lastDamageTime = 0f;
    public float damageCooldown = 0.5f; // Yarım saniye koruma süresi

    public void RemoveFoodFromIndex(GameObject hitFood, DamageBehavior behavior = DamageBehavior.SacrificeBottom)
    {
        // Eğer son hasardan bu yana yeterince süre geçmediyse işlem yapma
        if (Time.time < lastDamageTime + damageCooldown) return;

        lastDamageTime = Time.time;
        
        int index = collectedFoods.IndexOf(hitFood);

        if (index == -1) return;
        
        // --- SENARYO 1: KESME (Balyoz vb.) ---
        if (behavior == DamageBehavior.SliceFromHit)
        {
            // Eğer Lider (0) veya 1. elemana vurulduysa, Stack'i komple silmeyelim, yine alttan eksiltelim (Affedici olsun)
            if (index <= 1)
            {
                // Fallback -> Alttan eksilt
                HandleBottomSacrifice();
            }
            else
            {
                // Vurulan yerden YUKARISINI komple kes at
                // Döngü tersten çalışmalı (Sondan başa doğru sil ki index kaymasın)
                int countToRemove = collectedFoods.Count - index;
                for (int i = 0; i < countToRemove; i++)
                {
                    int lastIndex = collectedFoods.Count - 1;
                    GameObject foodToRemove = collectedFoods[lastIndex];

                    collectedFoods.RemoveAt(lastIndex);
                    rotVelocities.RemoveAt(lastIndex);
                    currentZAround.RemoveAt(lastIndex);

                    DropFoodPhysics(foodToRemove);
                }
                
                // Kalanlar için animasyon (Gerekirse)
                // StartCoroutine(StackDropEffect()); // Kesilince düşmesine gerek yok, zaten üstü gitti.
            }
            return;
        }

        // --- SENARYO 2: ALTTAN EKSİLTME (Testere vb.) ---
        if (behavior == DamageBehavior.SacrificeBottom)
        {
           HandleBottomSacrifice();
        }
    }

    private void HandleBottomSacrifice()
    {
        // Lideri ASLA yok etmiyoruz (Kamera ve Kontrol onda kalsın).
        // Onun yerine kuyruğun başındaki ilk elemanı (Index 1) feda ediyoruz.
        if (collectedFoods.Count > 1)
        {
            // Feda edilecek obje: Listenin 1. sırasındaki (Liderin arkasındaki)
            GameObject foodToSacrifice = collectedFoods[1];

            // Listelerden çıkar
            collectedFoods.RemoveAt(1);
            rotVelocities.RemoveAt(1);
            currentZAround.RemoveAt(1);

            // Fiziksel olarak fırlat ve yok et
            DropFoodPhysics(foodToSacrifice);

            // Kalanlar aşağı düşsün (Snap Efekti)
            StartCoroutine(StackDropEffect());
        }
        else
        {
            // Eğer kuyrukta hiç kimse kalmadıysa -> Oyun Biter
            Debug.Log("Game Over or Stack Empty!");
        }
    }

    private IEnumerator StackDropEffect()
    {
        // Kısa süreliğine Y takip hızını çok arttır veya özel animasyon yap
        // Ama en güzeli: Manuel olarak hafif bir "Hoplatma" veya "Squash" yapmaktır.
        // Şimdilik Lerp hızını manipüle edelim.
        
        float originalSpeed = followSpeedY;
        followSpeedY = 100f; // Çok hızlı düşüş (Snap hissi için)

        // Hatta hepsine küçük bir "Squash" (Ezilme) scale animasyonu verelim
        /*
        foreach (var food in collectedFoods)
        {
            if(food != null) StartCoroutine(PopAnimation(food.transform, food.transform.localScale));
        }
        */

        yield return new WaitForSeconds(0.2f); // 0.2 saniye hızlı düşsün
        
        followSpeedY = originalSpeed; // Normale dön
    }

    private void DropFoodPhysics(GameObject food)
    {
        // Collider'ı ve Varsa Çocuklardaki Colliderları tamamen kapat
        Collider[] cols = food.GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            col.enabled = false;
        }

        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            // Rastgele bir yöne fırlasın (Yukarı doğru değil, yana doğru fırlatalım ki yolu tıkamasın)
            rb.AddForce(new Vector3(UnityEngine.Random.Range(-5, 5), 2, UnityEngine.Random.Range(-5, 5)), ForceMode.Impulse);
            rb.AddTorque(UnityEngine.Random.insideUnitSphere * 10f, ForceMode.Impulse);
        }
        
        Destroy(food, 2f);
    }
}