using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;

public class HeroController : MonoBehaviour
{
    private static readonly int SpeedKey = Animator.StringToHash("Speed");
    private static readonly int StompKey = Animator.StringToHash("Stomp");
    private static readonly int AttackKey = Animator.StringToHash("Attack");
    
    public Aoe aoeStompAttack;

    public UnityEvent onHeroInitialised
    {
        get { return stats.characterDefinition.onHeroInitialized; }
    }
    
    private Animator animator;
    private NavMeshAgent agent;
    private CharacterStats stats;

    private GameObject attackTarget;

    private GameManager gameManager;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<CharacterStats>();

        gameManager = GameObject.FindWithTag(GameManager.Tag).GetComponent<GameManager>();
    }

    private void Start()
    {
        stats.characterDefinition.onLevelUp.AddListener(gameManager.OnHeroLeveledUp);
        stats.characterDefinition.onHeroDamaged.AddListener(gameManager.OnHeroDamaged);
        stats.characterDefinition.onHeroGainedHealth.AddListener(gameManager.OnHeroGainedHealth);
        stats.characterDefinition.onHeroDeath.AddListener(gameManager.OnHeroDied);
        stats.characterDefinition.onHeroInitialized.AddListener(gameManager.OnHeroInit);

        stats.characterDefinition.onHeroInitialized.Invoke();
    }

    private void Update()
    {
        animator.SetFloat(SpeedKey, agent.velocity.magnitude);
    }

    public void SetDestination(Vector3 destination)
    {
        StopAllCoroutines();
        agent.isStopped = false;
        agent.destination = destination;
    }

    public void DoStomp(Vector3 destination)
    {
        StopAllCoroutines();
        agent.isStopped = false;
        StartCoroutine(GoToTargetAndStomp(destination));
    }

    private IEnumerator GoToTargetAndStomp(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > aoeStompAttack.Range)
        {
            agent.destination = destination;
            yield return null;
        }
        agent.isStopped = true;
        animator.SetTrigger(StompKey);
    }

    public void AttackTarget(GameObject target)
    {
        if (stats.GetCurrentWeapon() == null)
        {
            return;
        }
        StopAllCoroutines();

        agent.isStopped = false;
        attackTarget = target;
        StartCoroutine(PursueAndAttackTarget());
    }

    private IEnumerator PursueAndAttackTarget()
    {
        agent.isStopped = false;
        var weapon = stats.GetCurrentWeapon();

        while (Vector3.Distance(transform.position, attackTarget.transform.position) > weapon.Range)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;

        transform.LookAt(attackTarget.transform);
        animator.SetTrigger(AttackKey);
    }

    public void Hit()
    {
        // Have our weapon attack the attack target
        if (attackTarget != null)
        {
            stats.GetCurrentWeapon().ExecuteAttack(gameObject, attackTarget);
        }
    }

    public void Stomp()
    {
        var o = gameObject;
        aoeStompAttack.Fire(o, o.transform.position, LayerMask.NameToLayer("PlayerSpells"));
    }

    public int GetCurrentHealth()
    {
        return stats.characterDefinition.currentHealth;
    }

    public int GetMaxHealth()
    {
        return stats.characterDefinition.maxHealth;
    }

    public int GetCurrentLevel()
    {
        return stats.characterDefinition.charLevel;
    }

    public int GetCurrentXp()
    {
        return stats.characterDefinition.charExperience;
    }

    #region Callbacks

    public void OnMobDeath(int pointVal)
    {
        stats.IncreaseXp(pointVal);
    }

    public void OnWaveComplete(int pointVal)
    {
        stats.IncreaseXp(pointVal);
    }

    public void OnOutOfWaves()
    {
        Debug.LogWarning("No more waves. you Win!");
    }

    #endregion
}
