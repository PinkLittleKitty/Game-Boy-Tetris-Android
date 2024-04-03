using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnAnimFinished : MonoBehaviour
{
    [SerializeField] bool destroyRoot;
    void Start()
    {
        float destroyTime = GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;

        Destroy(destroyRoot ? transform.root.gameObject : gameObject, destroyTime);
    }
}
