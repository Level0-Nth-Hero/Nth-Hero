using UnityEngine;

public class PlayerMove : MonoBehaviour , IDamageable
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator anim;

    [Header("에너지")]
    public int maxEnergy = 3;
    public int currentEnergy;

    public float maxHp { get; private set; } = 100;
    public float currentHp { get; private set; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //anim = GetComponent<Animator>();
        
        currentHp = maxHp;
    }

    void Start()
    {
         UIManager.Instance.UpdateHP(currentHp, maxHp, true);
         UIManager.Instance.UpdateHP(100, 100, false); 
    }

    public void RefillEnergy()
    {
        currentEnergy = maxEnergy;
        UIManager.Instance.UpdateEnergy(currentEnergy, maxEnergy);
    }

    public bool TryUseEnergy(int cost)
    {
        if (currentEnergy >= cost)
        {
            currentEnergy -= cost;
            UIManager.Instance.UpdateEnergy(currentEnergy, maxEnergy);
            return true;
        }
        else
        {
            Debug.Log("에너지가 부족합니다!");
            return false;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        anim.SetTrigger("Damaged");
        if (currentHp < 0) currentHp = 0;
        UIManager.Instance.UpdateHP(currentHp, maxHp, true);
    }

}