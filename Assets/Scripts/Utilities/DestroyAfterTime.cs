using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private bool destroyRootObject;
    void Start()
    {
        Destroy(destroyRootObject ? transform.root.gameObject : gameObject, time);
    }
}
