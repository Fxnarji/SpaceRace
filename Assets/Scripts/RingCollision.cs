  using System;
using UnityEngine;

public class RingCollision : MonoBehaviour
{
    private Renderer _renderer;
    private bool _active;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    //I hate collision. I make something always wrong :D
    // Works with Interface
    void OnTriggerEnter(Collider other)
    {
        if (_active) return;

        IRingCounter counter = other.GetComponentInParent<IRingCounter>();

        if (counter == null) return;

        _active = true;
        counter.CollectRing();
        _renderer.material.SetInt("_Active", 1);
    }
}
