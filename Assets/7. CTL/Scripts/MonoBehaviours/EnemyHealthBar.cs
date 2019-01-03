using UnityEngine;

public class EnemyHealthBar : MonoBehaviour 
{
    private Vector3 localScale;
    private CharacterStats enemyStats;

    private void Awake()
    {
        enemyStats = transform.parent.GetComponent<CharacterStats>();
    }

    // Use this for initialization
    void Start () 
    {
        localScale = transform.localScale;	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(enemyStats != null)
        {
            localScale.x = (float)enemyStats.characterDefinition.currentHealth /
                enemyStats.characterDefinition.maxHealth;

            transform.localScale = localScale;
        }

        transform.LookAt(Camera.main.transform);
		
	}
}
