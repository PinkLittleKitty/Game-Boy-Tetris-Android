using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMoveWithSpeed : MonoBehaviour
{
    [SerializeField] private Vector3 direction;
    [SerializeField] private float speed;

    void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);
    }
}
