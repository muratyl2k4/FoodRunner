using UnityEngine;
using TMPro;

public enum GateType { Addition, Multiplication, Subtraction } // Çıkarma işlemini de ekledik

public class GateManager : MonoBehaviour
{
    public GateType gateType;
    public int gateValue;
    
    [Header("References")]
    public TextMeshPro gateText;
    public MeshRenderer panelRenderer; // Şeffaf olan Quad objen

    private bool _isUsed = false;
    private MaterialPropertyBlock _propBlock;

    // Editör üzerinde bir değer değiştiğinde (Play'e basmadan) çalışır
    private void OnValidate()
    {
        UpdateGateVisuals();
    }

    private void Start()
    {
        UpdateGateVisuals();
    }

    public void UpdateGateVisuals()
    {
        if (gateText == null || panelRenderer == null) return;

        // 1. Yazıyı Güncelle
        string prefix = gateType switch
        {
            GateType.Addition => "+",
            GateType.Multiplication => "x",
            GateType.Subtraction => "-",
            _ => ""
        };
        gateText.text = prefix + gateValue.ToString();

        // 2. Rengi Güncelle (Pozitifse Yeşil, Negatifse Kırmızı)
        // Çarpma kapıları genellikle her zaman pozitiftir (Yeşil)
        bool isPositive = gateType == GateType.Multiplication || (gateType == GateType.Addition && gateValue >= 0);
        Color targetColor = isPositive ? Color.green : Color.red;
        targetColor.a = 0.4f; // Şeffaflık seviyesi

        // 3. Performanslı Renk Atama (MaterialPropertyBlock)
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        panelRenderer.GetPropertyBlock(_propBlock);
        
        // URP Unlit shader kullanıyorsan ana renk ismi "_BaseColor"dur. 
        // Standart shader ise "_Color" kullanabilirsin.
        _propBlock.SetColor("_BaseColor", targetColor); 
        panelRenderer.SetPropertyBlock(_propBlock);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isUsed && other.CompareTag("Player"))
        {
            _isUsed = true;
            StackManager.Instance.OnGateHit(gateType, gateValue);
            
            // Kapıyı tamamen yok etmek yerine sadece collider'ı kapatmak 
            // görselin birden kaybolup oyuncuyu rahatsız etmesini engeller.
            GetComponent<Collider>().enabled = false; 
            gameObject.SetActive(false); 
        }
    }
      

} 
