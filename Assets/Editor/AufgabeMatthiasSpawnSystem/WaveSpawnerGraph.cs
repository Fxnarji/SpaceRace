using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave Graph", menuName = "Wave Spawner")]
public class WaveSpawnerGraph : ScriptableObject
{
    public List<WaveSpawnerNode> nodes = new List<WaveSpawnerNode>();
    public string startNodeGuid;
}