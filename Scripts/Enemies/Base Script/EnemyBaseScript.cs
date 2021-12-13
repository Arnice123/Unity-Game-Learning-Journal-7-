using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy
{
    public float health;
    public float speed;
    public NavMeshAgent agent;
    public float inRange;
    public float inView;    

    public Enemy(float health, float speed, NavMeshAgent agent, float inRange, float inView)
    {
        this.health = health;
        this.speed = speed;
        this.agent = agent;
        this.inRange = inRange;
        this.inView = inView;        
    }
}

public class EnemyBaseScript : MonoBehaviour
{
    public float health;
    private float MaxHealth;
    public float speed;
    public float inRange;
    public float inView;
    public float AttackRange;
    public NavMeshAgent agent;
    public Enemy enemy;
    public GameObject self;
    public bool MultiAttack = false;      
    public bool isBlockingAndItHitSheild = false;      
    public bool isBlocking = false;
    public GameObject HealthBarUI;
    public Slider slider;

    private bool hasSeenPlayer = false;

    public static float SelectedDifficulty;
        
    private Animator anim;    
    
    private Weapon weaponGettingHitBy;    
    private TimeToFight TtF;    

    private void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
        MaxHealth = health;
    }
            
    void Start()
    {
        enemy = new Enemy(health, speed, agent, inRange, inView);
        TtF = gameObject.GetComponent<TimeToFight>();
        agent = gameObject.GetComponent<NavMeshAgent>();

        slider.value = CalculateHealth();

        if (SceneManager.sceneCount == 2)
        {
            SelectedDifficulty = GameObject.FindGameObjectWithTag("ButtonController").GetComponent<Difficulty>().difficulty;
            SceneManager.UnloadSceneAsync("DifficultyChoosing");
        }
        else
        {
            SelectedDifficulty = Chosen_dif.SDif;
        }                        

        health *= SelectedDifficulty;
        speed *= SelectedDifficulty;
        inRange *= SelectedDifficulty;
        inView *= SelectedDifficulty;

        

        StartCoroutine(GetGood());
    }

    IEnumerator GetGood()
    {        
        TtF.ChoseChasingWhatStateToAttackPlayer(agent, Player_pos.player, self, anim, MultiAttack, inRange, inView, AttackRange, isBlocking, hasSeenPlayer);
        
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(GetGood());
    }

    public void Update()
    {
        if (health < 0)
        {
            health = 0;
            StartCoroutine(JustDies());
        }

        if (health < MaxHealth)
        {
            HealthBarUI.SetActive(true);
        }

        slider.transform.LookAt(Player_pos.player.transform);

        if (TimeToFight.state == TimeToFight.State.Idle && health < MaxHealth)
        {            
            Invoke("Heal", 1.0f);
        }

        slider.value = CalculateHealth();
    }

    public float Heal()
    {
        health = health + 1;
        return health;        
    }

    float CalculateHealth()
    {
        return health / MaxHealth;
    }

    IEnumerator JustDies()
    {
        anim.SetBool("isDead", true);
        
        yield return new WaitForSeconds(4.5f);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Weapon")
        {
            weaponGettingHitBy = new Weapon(col.gameObject, col.gameObject.GetComponent<WeaponBaseScript>().damage);
            if (isBlockingAndItHitSheild)
            {
                return;
            }
            health = TakeDamage.TakeDmg(health, weaponGettingHitBy);
            slider.value = CalculateHealth();
        }
    }
}

