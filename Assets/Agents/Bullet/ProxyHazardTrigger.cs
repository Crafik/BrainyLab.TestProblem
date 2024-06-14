using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProxyHazardTrigger : MonoBehaviour
{
    [SerializeField] private BulletBehaviour bullet;
    void OnTriggerEnter2D(Collider2D collision){
        if (collision.CompareTag("Enemy")){
            if (bullet.ProxyCheck(collision)){
                collision.GetComponent<EnemyBehaviour>().Evade(transform.position);
            }
        }
    }
}
