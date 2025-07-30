using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floaty : MonoBehaviour
{
    public float amplitude = 0.25f, frequency = 1f;
    Vector3 start;
    void Awake() => start = transform.localPosition;
    void Update()
    {
        transform.localPosition = start + Vector3.up * Mathf.Sin(Time.time * frequency) * amplitude;
    }
}