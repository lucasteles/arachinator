using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shell : MonoBehaviour
{

    [SerializeField] Rigidbody rb;
    public float forceMin;
    public float forceMax;

    float lifeTime = 4;
    float fadeTime = 2;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start ()
    {
        var force = Random.Range(forceMin, forceMax);

        rb.AddForce(transform.right * force);
        rb.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }
	
    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifeTime);

        var percent = 0f;
        var fadeSpeed = 1 / fadeTime;

        var mat = GetComponent<Renderer>().material;
        var originalCol = mat.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(originalCol, Color.clear, percent);

            yield return null;
        }

        Destroy(gameObject);
    }
}
