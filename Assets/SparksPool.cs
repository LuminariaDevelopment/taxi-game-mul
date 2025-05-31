using UnityEngine;
using System.Collections.Generic;

public class SparksPool : MonoBehaviour
{
    public static SparksPool Instance;
    [SerializeField] private GameObject sparkPrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<ParticleSystem> pool = new Queue<ParticleSystem>();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(sparkPrefab, transform);
            var ps = go.GetComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            pool.Enqueue(ps);
        }
    }

    public ParticleSystem GetSpark()
    {
        if (pool.Count == 0) return null;
        var ps = pool.Dequeue();
        ps.gameObject.SetActive(true);
        ps.Clear();
        return ps;
    }

    public void ReturnSpark(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        ps.gameObject.SetActive(false);
        pool.Enqueue(ps);
    }
}
