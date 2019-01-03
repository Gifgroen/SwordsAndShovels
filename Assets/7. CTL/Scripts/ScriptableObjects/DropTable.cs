using UnityEngine;

[CreateAssetMenu(fileName = "newDropTable", menuName = "New Drop Table")]
public class DropTable : ScriptableObject 
{
    public MobType mobType;
    public DropDefinition[] drops;
}
