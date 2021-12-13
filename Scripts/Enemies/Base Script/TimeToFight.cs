using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;


public class TimeToFight : MonoBehaviour
{
    public enum State
    {
        Chase,
        Attack,
        Idle,
        Wander
    }

    public static State state = State.Idle;
    public Vector3 chosenDest;
    public GameObject invis;
    private GameObject checkedheight;
    RaycastHit hit;

    Ray downRay;
    Ray upRay;
    public TimeToFight instance;

    private void Awake()
    {
        instance = this;
    }

    Alk alk;

    private void Start()
    {
        if (gameObject.GetComponent<Alk>() != null)
        {
            alk = gameObject.GetComponent<Alk>();
        }
    }

    public void ChoseChasingWhatStateToAttackPlayer(NavMeshAgent agent, GameObject playerPos, GameObject Gself, Animator anim, bool MultiAttack, float inRange, float inView, float AttackRange, bool isBlocking, bool HasSeenPLayer)
    {
        switch (state)
        {
            case State.Chase:
                ChasePlayer(playerPos, Gself, anim, agent, AttackRange, inRange, inView, HasSeenPLayer);
                break;
            case State.Attack:
                AttackPlayer(playerPos, anim, agent, Gself, MultiAttack, inRange, inView, AttackRange, isBlocking, HasSeenPLayer);
                break;
            case State.Idle:
                IdlePlayer(playerPos, anim, agent, Gself, inRange, inView, HasSeenPLayer);
                break;
            case State.Wander:
                WanderPlayer(playerPos, anim, agent, Gself, HasSeenPLayer);
                break;
        }
    }

    private float distDif;
    public int random = 0; // random Attacks
    
    public void AttackPlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, bool MultiAttack, float inRange, float inView, float AttackRange, bool isBlocking, bool HasSeenPLayer)
    {
        distDif = Vector3.Distance(self.transform.position, playerPos.transform.position);

        if (distDif >= AttackRange)
        {
            agent.SetDestination(playerPos.transform.position);
            //anim.SetBool("walk", false);
        }

        else if (distDif <= AttackRange)
        {
            anim.SetBool("walk", false);
        }

        if (MultiAttack)
        {
            MultAttack(playerPos, anim, agent, self, inRange, inView);            
        }
        else
        {
            anim.SetBool("Attack2", false);
            anim.SetBool("Attack3", false);
            StartCoroutine(PlayAnim(anim, "Attack"));
        }
        
        if (!PlayerPos.FindPlayerPos(AttackRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            anim.SetBool("Attack", false);
            anim.SetBool("Attack2", false);
            anim.SetBool("Attack3", false);
            state = State.Chase;
        }
    }

    public void MultAttack(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, float inRange, float inView)
    {      
        if (random == 0) random = Random.Range(1, 5);

        if (random == 1)
        {
            StartCoroutine(PlayAnim(anim, "Attack"));
            random = 0;            
            return;
        }

        if (random == 2)
        {
            if (HasParameter("Attack2", anim))
            {
                StartCoroutine(PlayAnim(anim, "Attack2"));
                random = 0;
                return;
            }

            StartCoroutine(PlayAnim(anim, "Attack"));
            random = 0;
            return;
        }

        if (random == 3)
        {
            if (alk != null)
            {
                Debug.Log("This runs");
                
                alk.particle.gameObject.SetActive(true);
                alk.particle.Play();
                StartCoroutine(PlayAnim(anim, "Attack3"));
                alk.particle.Stop();
                alk.particle.gameObject.SetActive(false);
                random = 0;
                return;
            }

            if (HasParameter("Attack3", anim))
            {
                StartCoroutine(PlayAnim(anim, "Attack3"));
                random = 0;
                return;
            }

            StartCoroutine(PlayAnim(anim, "Attack"));
            random = 0;
            return;
        }

        if (random == 4)
        {
            BlockPlayer(playerPos, anim, agent, inRange, inView, self);
            random = 0;
            return;
        }

        StartCoroutine(PlayAnim(anim, "Attack"));
        random = 0;
        return;

    }

    public static bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    public void ChasePlayer(GameObject playerPos, GameObject self, Animator anim, NavMeshAgent agent, float AttackRange, float inRange, float inView, bool HasSeenPLayer)
    {
        agent.SetDestination(playerPos.transform.position);
        //StartCoroutine(PlayAnim(anim, "walk"));
        anim.SetBool("walk", true);

        distDif = Vector3.Distance(playerPos.transform.position, self.transform.position);
        if (PlayerPos.FindPlayerPos(AttackRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            state = State.Attack;
        }

        if (!PlayerPos.FindPlayerPos(inRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            state = State.Idle;
        }
    }


    public void BlockPlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, float inRange, float inView, GameObject self)
    {
        if (HasParameter("Block", anim))
            StartCoroutine(PlayAnim(anim, "Block"));

        if (HasParameter("BlockAttack", anim))
            StartCoroutine(PlayAnim(anim, "BlockAttack"));

        if (HasParameter("Attack", anim))
            StartCoroutine(PlayAnim(anim, "Attack"));

        state = State.Attack;
    }

    private Coroutine coroutine;
    private Coroutine wand;
    public void IdlePlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, float inRange, float inView, bool HasSeenPLayer)
    {
        if (PlayerPos.FindPlayerPos(inRange, playerPos.transform, inView, self, HasSeenPLayer))
        {
            state = State.Chase;
        }

        if (coroutine == null)
        {
            //instance.StartCoroutine(LookAroundArea(self));
            coroutine = StartCoroutine(LookAroundArea(self));
        }

        if (wand == null)
        {
            //instance.StartCoroutine(RandomlyWander());         
            wand = StartCoroutine(RandomlyWander());
        }
    }

    IEnumerator RandomlyWander()
    {
        yield return new WaitForSeconds(1.0f);
        if (10 >= Random.Range(1, 100)) // DECREASE CHANCES ----------------------------------------------
        {
            state = State.Wander;
        }
        wand = null;
    }

    IEnumerator LookAroundArea(GameObject self)
    {
        if (1 >= Random.Range(1, 1000))
        {
            // Cache the start, left, and right extremes of our rotation.
            Quaternion start = self.transform.rotation;
            Quaternion left = start * Quaternion.Euler(0, -45, 0);
            Quaternion right = start * Quaternion.Euler(0, 45, 0);

            // Yield control to the Rotate coroutine to execute
            // each turn in sequence, and resume here after each
            // invocation of Rotate finishes its work.

            yield return Rotate(self.transform, start, left, 1.0f);
            yield return Rotate(self.transform, left, right, 2.0f);
            yield return Rotate(self.transform, right, start, 1.0f);
        }
        coroutine = null;
    }

    NavMeshHit hite;
    private bool coru = false;
    Coroutine cro = null;
    public void WanderPlayer(GameObject playerPos, Animator anim, NavMeshAgent agent, GameObject self, bool HasSeenPLayer)
    {
        agent.isStopped = false;
        if (coru == false)
        {
            coru = true;
            agent.SetDestination(GetDest(self));            
            
        }
        anim.SetBool("walk", true);

        if ((Vector3.Distance(agent.transform.position, agent.destination) <= 0.5f || Vector3.Distance(agent.transform.position, agent.destination) <= -0.5f)
                || (Mathf.Approximately(self.transform.position.x, agent.destination.x) && Mathf.Approximately(self.transform.position.z, agent.destination.z)))
        {
            state = State.Idle;
            if (cro == null)
                cro = StartCoroutine(PlayAnim(anim, "walk"));
            agent.ResetPath();
            coru = false;
        }
    }

    float StopOtherAnim(Animator anim)
    {
        float animatorLength;
        animatorLength = anim.GetCurrentAnimatorStateInfo(0).length + anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        return animatorLength;
    }

    IEnumerator PlayAnim(Animator anim, string booleanName)
    {
        anim.SetBool(booleanName, true);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        anim.SetBool(booleanName, false);
        cro = null;
    }

    private GameObject[] enemies;
    private GameObject closestEnemy;

    private Vector3 GetDest(GameObject self)
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float distDif = 0;
        float curDistdif = 0;

        if (enemies != null)
            chosenDest = CloseToEnemy(distDif, curDistdif, self);
        else
            chosenDest = DefaultFind(self);
               
        return chosenDest;
    }

    public Vector3 CloseToEnemy(float distDif, float curDistdif, GameObject self)
    {
        closestEnemy = enemies[0];
        foreach (GameObject g in enemies)
        {
            distDif = Vector3.Distance(this.transform.position, g.transform.position);
            curDistdif = Vector3.Distance(this.transform.position, closestEnemy.transform.position);
            if (distDif < curDistdif)
            {
                closestEnemy = g;
            }
        }
        curDistdif = Vector3.Distance(this.transform.position, closestEnemy.transform.position);

        if (curDistdif >= 60)
        {
            chosenDest = DefaultFind(self);
            return chosenDest;
        }

        float AngleDif = Vector3.Angle(self.transform.position, closestEnemy.transform.position);

        Quaternion start = self.transform.rotation;
        Quaternion turnAmount = start * Quaternion.Euler(0, AngleDif, 0);

        Rotate(self.transform, start, turnAmount, 1.0f);

        chosenDest = new Vector3(closestEnemy.transform.position.x + Random.Range(-30, 30), closestEnemy.transform.position.y, closestEnemy.transform.position.z + Random.Range(-30, 30));

        while (Vector3.Distance(chosenDest, closestEnemy.transform.position) >= 5.0f && Vector3.Distance(chosenDest, closestEnemy.transform.position) <= -5)
        {
            chosenDest = new Vector3(closestEnemy.transform.position.x + Random.Range(-30, 30), closestEnemy.transform.position.y, closestEnemy.transform.position.z + Random.Range(-30, 30));
        }

        return chosenDest;
    }

    public Vector3 DefaultFind(GameObject self)
    {
        chosenDest = new Vector3(self.transform.position.x + Random.Range(-30, 30), self.transform.position.y, self.transform.position.z + Random.Range(-30, 30));
        checkedheight = Instantiate(invis, chosenDest, Quaternion.identity);

        downRay = new Ray(checkedheight.transform.position, -transform.up);
        upRay = new Ray(checkedheight.transform.position, transform.up);

        if (Physics.Raycast(upRay, out hit) || Physics.Raycast(downRay, out hit))
        {
            if (hit.collider != null && hit.collider == CompareTag("Terrain"))
            {
                chosenDest.y = hit.point.y;
                //chosenDest.y = hit.collider.transform.position.y;
            }
        }

        return chosenDest;
    }

    IEnumerator Rotate(Transform self, Quaternion from, Quaternion to, float duration)
    {

        for (float t = 0; t < 1f; t += Time.deltaTime / duration)
        {
            // Rotate to match our current progress between from and to.
            self.rotation = Quaternion.Slerp(from, to, t);
            // Wait one frame before looping again.
            yield return null;
        }

        // Ensure we finish exactly at the destination orientation.
        self.rotation = to;
    }

}
