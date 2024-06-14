using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    [Header ("===== Components =====")]
    [SerializeField] private Rigidbody2D m_body;
    [SerializeField] private CircleCollider2D m_collider;

    [Header ("===== Variables =====")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask layerMask;

    private Vector2 moveVelocity;

    private Collider2D owner;

    void Start(){
        moveVelocity = (Vector2)transform.right;
    }

    public void Init(Collider2D own){
        Physics2D.IgnoreCollision(m_collider, own);
        owner = own;
    }

    public bool ProxyCheck(Collider2D proxy){
        return proxy != owner;
    }

    void OnBecameInvisible(){
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision){
        if (!collision.gameObject.CompareTag("Walls")){
            collision.gameObject.GetComponent<IDamageable>().GetDamage();
            Destroy(gameObject);
        }
        moveVelocity = Vector2.Reflect(moveVelocity, collision.GetContact(0).normal);
    }

    void FixedUpdate(){
        m_body.velocity = moveVelocity * moveSpeed;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveVelocity, Mathf.Infinity, layerMask);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy") && hit.collider != owner){
            if (Vector3.Distance(transform.position, hit.collider.transform.position) < 7f){
                hit.collider.GetComponent<EnemyBehaviour>().Evade(transform.position);
            }
        }
    }
}
