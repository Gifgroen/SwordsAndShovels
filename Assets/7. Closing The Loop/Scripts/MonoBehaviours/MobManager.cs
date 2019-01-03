using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Collections.Generic;

public class MobManager : MonoBehaviour 
{
    public GameObject[] Mobs;
    public MobWave[] Waves;
    public List<DropTable> dropTables;

    public Events.EventIntegerEvent OnMobKilled;
    public Events.EventIntegerEvent OnWaveCompleted;
    public UnityEvent OnOutOfWaves;
    public UnityEvent OnWaveSpawned;

    private int currentWaveIndex = 0;
    private int activeMobs;

    private Spawnpoint[] spawnpoints;

	// Use this for initialization
	void Start () 
    {
        spawnpoints = FindObjectsOfType<Spawnpoint>();
        SpawnWave();
	}
	
    public void SpawnWave()
    {
        if(Waves.Length -1 < currentWaveIndex)
        {
            StartCoroutine(GameManager.Instance.EndGame());
            OnOutOfWaves.Invoke();
            return;
        }

        if (currentWaveIndex > 0)
        {
            SoundManager.Instance.PlaySoundEffect(SoundEffect.NextWave);
            OnWaveSpawned.Invoke();
        }
        activeMobs = Waves[currentWaveIndex].NumberOfMobs;

        for (int i = 0; i <= Waves[currentWaveIndex].NumberOfMobs - 1; i++)
        {
            Spawnpoint spawnpoint = selectRandomSpawnpoint();
            GameObject mob = Instantiate(selectRandomMob(), 
                                         spawnpoint.transform.position, Quaternion.identity);
            mob.GetComponent<NPCController>().waypoints = findClosestWayPoints(mob.transform);

            CharacterStats stats = mob.GetComponent<CharacterStats>();
            MobWave currentWave = Waves[currentWaveIndex];

            stats.SetInitialHealth(currentWave.MobHealth);
            stats.SetInitialResistance(currentWave.MobResistance);
            stats.SetInitialDamage(currentWave.MobDamage);
        }
    }

    public void OnMobDeath(MobType mobType, Vector3 mobPosition)
    {
        SoundManager.Instance.PlaySoundEffect(SoundEffect.MobDeath);
        spawnDrop(mobType, mobPosition);

        MobWave currentWave = Waves[currentWaveIndex];

        activeMobs -= 1;
        OnMobKilled.Invoke(currentWave.PointsPerKill);
        Debug.LogWarningFormat("{0} killed at {1}", mobType, mobPosition);

        if(activeMobs == 0)
        {
            OnWaveCompleted.Invoke(currentWave.WaveValue);
            currentWaveIndex += 1;
            SpawnWave();
        }
    }
	
    private GameObject selectRandomMob()
    {
        int mobIndex = Random.Range(0, Mobs.Length);
        return Mobs[mobIndex];
    }

    private Spawnpoint selectRandomSpawnpoint()
    {
        int pointIndex = Random.Range(0, spawnpoints.Length);
        return spawnpoints[pointIndex];
    }

    private Transform[] findClosestWayPoints(Transform mobTranform)
    {
        Vector3 mobPosition = mobTranform.position;

        Waypoint closestPoint =
            FindObjectsOfType<Waypoint>().OrderBy(
                w => (w.transform.position - mobPosition).sqrMagnitude).First();

        Transform parent = closestPoint.transform.parent;

        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>();

        var transforms =
            from t in allTransforms
            where t != parent
            select t;

        return transforms.ToArray();
   }

    private void spawnDrop(MobType mobType, Vector3 position)
    {
        ItemPickUps_SO item = getDrop(mobType);

        if (item != null)
            Instantiate(item.itemSpawnObject, position, Quaternion.identity);
    }


    private ItemPickUps_SO getDrop(MobType mobType)
    {
        DropTable mobDrops = dropTables.Find(mt => mt.mobType == mobType);

        if (mobDrops == null)
            return null;

        mobDrops.drops.OrderBy(d => d.DropChance);

        foreach(DropDefinition dropDef in mobDrops.drops)
        {
            bool shouldDrop = Random.value < dropDef.DropChance;

            if (shouldDrop)
                return dropDef.Drop;
        }

        return null;
    }
}
