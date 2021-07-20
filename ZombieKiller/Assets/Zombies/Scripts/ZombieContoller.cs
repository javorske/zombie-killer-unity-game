using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieContoller : MonoBehaviour
{
    public GameObject target;
    public AudioSource[] splatsSounds;
    public AudioSource scream;
    public float walkingSpeed;
    public float runningSpeed;
    public GameObject ragdoll;
    public float damageAmount = 5;
    public int lives = 4;
    public int shotsTaken = 0;

    Animator anim;
    NavMeshAgent agent;
    public enum STATE { IDLE, WANDER, ATTACK, CHASE, DEAD };
    public STATE state = STATE.IDLE;
    void Start()
    {
        GameStats.gameOver = false;
        anim = this.GetComponent<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
    }

    void TurnOffTriggers()
    {
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isDead", false);
    }

    float DistanceToPlayer()
    {
        if (GameStats.gameOver) return Mathf.Infinity;
        return Vector3.Distance(target.transform.position, this.transform.position);
    }
    bool CanSeePlayer()
    {
        if (DistanceToPlayer() < 17)
        {
            if (1 == Random.Range(0, 2))
                scream.Play();
            return true;
        }
        return false;
    }

    bool ForgetPlayer()
    {
        if (DistanceToPlayer() > 20)
        {
            return true;
        }
        return false;
    }
    public void KillZombies()
    {
            TurnOffTriggers();
            anim.SetBool("isDead", true);
            state = STATE.DEAD;
    }

    public void DamagePlayer()
    {
        if (target != null)
        {
            target.GetComponent<FPController>().TakeHit(damageAmount);
            PlaySplatsAudio();
        }
    }
    void PlaySplatsAudio()
    {
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, splatsSounds.Length);
        audioSource = splatsSounds[n];
        audioSource.Play();
        splatsSounds[n] = splatsSounds[0];
        splatsSounds[0] = audioSource;
    }
    void Update()
    {
        
            target = GameObject.FindWithTag("Player");
        switch (state)
        {
            case STATE.IDLE:

                if (CanSeePlayer()) state = STATE.CHASE;
                else if (Random.Range(0, 3000) < 5)
                {
                    state = STATE.WANDER;
                }
                break;

            case STATE.WANDER:
                if (!agent.hasPath)
                {
                    float newX = this.transform.position.x + Random.Range(-5, 5);
                    float newZ = this.transform.position.z + Random.Range(-5, 5);
                    float newY = Terrain.activeTerrain.SampleHeight(new Vector3(newX, 0, newZ));
                    Vector3 destination = new Vector3(newX, newY, newZ);
                    agent.SetDestination(destination);
                    agent.stoppingDistance = 0;
                    TurnOffTriggers();
                    agent.speed = walkingSpeed;
                    anim.SetBool("isWalking", true);

                }
                if (CanSeePlayer()) state = STATE.CHASE;
                else if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.IDLE;
                    TurnOffTriggers();
                    agent.ResetPath();
                }
                break;

            case STATE.ATTACK:
                if (GameStats.gameOver)
                {
                    TurnOffTriggers();
                    state = STATE.WANDER;
                    break;
                }
                TurnOffTriggers();
                anim.SetBool("isAttacking", true);
                Vector3 playerPos = GameObject.FindWithTag("Player").transform.position;
                Vector3 zombiePos = this.transform.position;
                Vector3 delta = new Vector3(playerPos.x - zombiePos.x, 0, playerPos.z - zombiePos.z);
                Quaternion rotation = Quaternion.LookRotation(delta);
                this.transform.rotation = rotation;
                if (DistanceToPlayer() > agent.stoppingDistance + 2)
                {
                    state = STATE.CHASE;
                }
                break;

            case STATE.CHASE:
                if (GameStats.gameOver)
                {
                    TurnOffTriggers();
                    state = STATE.WANDER;
                    break;
                }
                agent.SetDestination(target.transform.position);
                agent.stoppingDistance = 3;
                TurnOffTriggers();
                agent.speed = runningSpeed;
                anim.SetBool("isRunning", true);

                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    state = STATE.ATTACK;
                }
                if (ForgetPlayer())
                {
                    state = STATE.WANDER;
                    agent.ResetPath();
                }
                break;

            case STATE.DEAD:
                Destroy(agent);
                AudioSource[] sounds = this.GetComponents<AudioSource>();
                foreach (AudioSource sound in sounds)
                {
                    sound.volume = 0;
                }
                this.GetComponent<Sink>().StartSink();
                break;

            default:
                break;
        }
    }
}
