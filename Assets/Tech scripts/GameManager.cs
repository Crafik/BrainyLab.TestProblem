using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void GameRestartEvent();
    public static event GameRestartEvent OnGameRestart;

    public static GameManager Instance { get; private set;}

    [Header ("===== Scene objects references =====")]
    [SerializeField] private TextMeshProUGUI scoreTable;


    [HideInInspector] public bool isGameActive;
    private float scoringTimer;

    private int playerScore;
    private int enemyScore;

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
        }
        else{
            Instance = this;
        }
    }

    void Start(){
        isGameActive = true;
        playerScore = 0;
        enemyScore = 0;
        SetScore();
    }

    public void AgentScored(bool isPlayer){
        isGameActive = false;
        if (isPlayer){
            playerScore += 1;
        }
        else{
            enemyScore += 1;
        }
        scoringTimer = 2f;
    }

    private void SetScore(){
        scoreTable.text = playerScore + ":" + enemyScore;
    }

    void Update(){
        if (scoringTimer > 0){
            scoringTimer -= Time.deltaTime;
        }
        if (scoringTimer < 0 && !isGameActive){
            isGameActive = true;
            GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
            foreach (GameObject bullet in bullets){
                Destroy(bullet);
            }
            SetScore();
            OnGameRestart();
        }
    }
}
