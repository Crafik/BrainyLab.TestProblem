using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Extensions;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour, IDamageable
{

    #region Editor fields
    // ===================================

    [Header ("===== Components =====")]
    [SerializeField] private NavMeshAgent m_agent;

    [Space (10)]
    [SerializeField] private GameObject bulletPrefab;

    [Space (10)]
    [Header ("===== Variables =====")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask m_layerMask;

    [Space (10)]
    [Header ("===== Outer entities =====")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform hidingSpots;

    // ===================================
    #endregion

    private GameObject bullet;
    private Vector3 respawnPosition;

    private bool isAlive;
    public bool isEvading;

    void Awake(){
        isAlive = true;
        isEvading = false;

        m_agent.updateRotation = false;
        m_agent.updateUpAxis = false;
        m_agent.speed = moveSpeed;

        respawnPosition = transform.position;
        GameManager.OnGameRestart += Respawn;
        Respawn();
    }

    public void GetDamage(){
        GameManager.Instance.AgentScored(true);
        isAlive = false;
    }

    void Respawn(){
        isAlive = true;
        isEvading = false;
        evadingTimer = 0f;
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
        transform.position = respawnPosition;
        currentHidingSpot = hidingSpots.GetChild(Random.Range(0, 3)).transform.position;
        grazePeriod = 3f;
    }

    Vector3 enemyBulletPos;
    public void Evade(Vector3 bulletPos){
        isEvading = true;
        enemyBulletPos = bulletPos;
        evadingTimer = 1.5f;
    }

    float aimingTimer;
    float spottingTimer;
    float evadingTimer;
    float grazePeriod = 3f;
    float scatterShiftTimer = 3f;
    Vector3 currentHidingSpot;
    void FixedUpdate(){
        if (GameManager.Instance.isGameActive){
            Vector3 dirToPlayer = player.transform.position - transform.position;
            float distToPlayer = Vector3.Distance(player.transform.position, transform.position);

            RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, dirToPlayer, Mathf.Infinity, m_layerMask);
            
            Vector3 destination;

            if (isEvading){
                destination = transform.position - (enemyBulletPos - transform.position);
                evadingTimer -= Time.fixedDeltaTime;
                if (evadingTimer < 0){
                    isEvading = false;
                }
            }
            else{
                // Aiming
                if (raycastHit.collider != null && raycastHit.collider.gameObject.CompareTag("Player") && bullet == null){
                    spottingTimer -= Time.fixedDeltaTime;
                    if (spottingTimer < 0){
                        m_agent.SetDestination(transform.position);
                        transform.rotation = Quaternion.LookRotation(transform.forward, dirToPlayer);
                        aimingTimer -= Time.fixedDeltaTime;

                        if (aimingTimer < 0){
                            bullet = Instantiate(bulletPrefab, transform.GetChild(0).position, transform.GetChild(0).rotation);
                            bullet.GetComponent<BulletBehaviour>().Init(GetComponent<CircleCollider2D>());
                            currentHidingSpot = hidingSpots.GetChild(Random.Range(0, 3)).transform.position;
                        }
                        return;
                    }
                }
                else{
                    aimingTimer = 1f;
                    spottingTimer = 0.5f;
                }

                // Pursuit
                if (bullet == null && distToPlayer > 10f){
                    if (grazePeriod < 0){
                        destination = player.transform.position;
                    }
                    else{
                        grazePeriod -= Time.fixedDeltaTime;
                        destination = (player.transform.position + currentHidingSpot) / 2;
                    }
                }
                // Scatter
                else{
                    destination = currentHidingSpot;
                    scatterShiftTimer -= Time.fixedDeltaTime;
                    if (scatterShiftTimer < 0){
                        currentHidingSpot = hidingSpots.GetChild(Random.Range(0, 3)).transform.position;
                        scatterShiftTimer = 1.5f;
                    }
                    if (distToPlayer < 10f){
                        grazePeriod = 1.5f;
                    }
                }
            }

            transform.rotation = Quaternion.LookRotation(transform.forward, m_agent.velocity);
            m_agent.SetDestination(destination);
        }
        else{
            m_agent.SetDestination(transform.position); // a bit hacky, but meh
            if (!isAlive){
                transform.Rotate(new Vector3(0f, 0f, 25f));
                return;
            }
        }
    }
}
