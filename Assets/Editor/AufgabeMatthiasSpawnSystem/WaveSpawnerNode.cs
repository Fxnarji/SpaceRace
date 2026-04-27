using System;
using UnityEngine;

[Serializable]
public class WaveSpawnerNode
{
    public string guid;
    public Rect rect;
    public string nextNodeGuid;
    public string waveName = "New Wave";
    public int enemyCount = 10;
    public float waveTimer = 5f;

    public WaveSpawnerNode(Vector2 position)
    {
        guid = Guid.NewGuid().ToString();
        nextNodeGuid = "";
        rect = new Rect(position.x, position.y, 220, 300);
        waveName = "New Wave";
        waveTimer = 5f;
    }
}

