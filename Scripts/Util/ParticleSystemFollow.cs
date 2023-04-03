using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemFollow : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 offset;

    private void LateUpdate()
    {
        transform.position = playerTransform.position + offset;
    }
}
