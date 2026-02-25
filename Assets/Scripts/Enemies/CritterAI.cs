using HeroesOfCrimson.Utils;
using UnityEngine;

public class CritterAI : MonoBehaviour
{
    private BaseNPCBehaviour _npcBehaviour;

    [SerializeField] private float wanderDuration = 0.4f;
    [SerializeField] private float wanderPause = 1.5f;
    [SerializeField] private float wanderSpeedMultiplier = 0.4f;

    private Vector2 _wanderDirection;
    private float _wanderTimer;
    private float _wanderPauseTimer;

    void Start()
    {
        _npcBehaviour = GetComponent<BaseNPCBehaviour>();
        PickNewWanderDirection();
    }

    void FixedUpdate()
    {
        Wander();
    }

    private void Wander()
    {
        if (_wanderPauseTimer > 0f)
        {
            _wanderPauseTimer -= Time.deltaTime;
            return;
        }

        _wanderTimer -= Time.deltaTime;

        if (_wanderTimer <= 0f)
        {
            PickNewWanderDirection();
            return;
        }

        _npcBehaviour.Move(_wanderDirection * wanderSpeedMultiplier);
    }

    private void PickNewWanderDirection()
    {
        _wanderDirection = Random.insideUnitCircle.normalized;
        _wanderTimer = wanderDuration;
        _wanderPauseTimer = wanderPause;
    }
}