using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable
{
    private Controls m_controls;

    #region Editor fields
    // ===================================

    [Header ("===== Components =====")]
    [SerializeField] private Rigidbody2D m_body;

    [Space (10)]
    [SerializeField] private GameObject bulletPrefab;

    [Space (10)]
    [Header ("===== Variables =====")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;

    // ===================================
    #endregion

    private Vector2 moveVector;
    private float rotateVector; // Naming for consistency

    private bool isAlive;

    private GameObject bullet;
    private Vector3 respawnPosition;

    void Awake(){
        m_controls = new Controls();

        respawnPosition = transform.position;
        GameManager.OnGameRestart += Respawn;
        Respawn();
    }

    void OnEnable(){
        m_controls.Enable();

        m_controls.Player.Movement.performed += OnMovePerformed;
        m_controls.Player.Movement.canceled += OnMovePerformed;
        m_controls.Player.Rotation.performed += OnRotationPerformed;
        m_controls.Player.Rotation.canceled += OnRotationPerformed;
        m_controls.Player.Shoot.performed += OnShootPerformed;
    }

    void OnDisable(){
        m_controls.Disable();

        m_controls.Player.Movement.performed -= OnMovePerformed;
        m_controls.Player.Movement.canceled -= OnMovePerformed;
        m_controls.Player.Rotation.performed -= OnRotationPerformed;
        m_controls.Player.Rotation.canceled -= OnRotationPerformed;
        m_controls.Player.Shoot.performed -= OnShootPerformed;
    }

    void OnMovePerformed(InputAction.CallbackContext ctx){
        moveVector = ctx.ReadValue<Vector2>();
    }

    void OnRotationPerformed(InputAction.CallbackContext ctx){
        rotateVector = ctx.ReadValue<float>();
    }

    void OnShootPerformed(InputAction.CallbackContext ctx){
        if (bullet == null){
            bullet = Instantiate(bulletPrefab, transform.GetChild(0).position, transform.GetChild(0).rotation);
            bullet.GetComponent<BulletBehaviour>().Init(GetComponent<CircleCollider2D>());
        }
    }

    public void GetDamage()
    {
        m_body.velocity = Vector2.zero;
        GameManager.Instance.AgentScored(false);
        isAlive = false;
    }

    void Respawn(){
        //code
        m_body.SetRotation(0);
        transform.position = respawnPosition;
        isAlive = true;
    }

    void FixedUpdate(){
        if (GameManager.Instance.isGameActive){
            m_body.velocity = moveSpeed * ((Vector2)transform.up * moveVector.y + (Vector2)transform.right * moveVector.x);
            m_body.MoveRotation(m_body.rotation + rotateVector * rotationSpeed * Time.fixedDeltaTime);
        }
        else{
            m_body.velocity = Vector2.zero;
            if (!isAlive){
                m_body.MoveRotation(m_body.rotation + 25);
                return;
            }
        }
    }
}
