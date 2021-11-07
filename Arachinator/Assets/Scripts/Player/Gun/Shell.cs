using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Shell : MonoBehaviour
{
   [SerializeField] float forceMin;
   [SerializeField] float forceMax;
   [SerializeField] float lifeTime = 4;
   [SerializeField] float fadeTime = 2;
   [SerializeField] AudioClip dropAudioClip;

   Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Floor"))
        {
            var audioSource = GetComponent<AudioSource>();
            audioSource.pitch = Random.Range(1f, 2f);
            audioSource.PlayOneShot(dropAudioClip);
        }
    }

    void OnEnable()
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

        rb.velocity = Vector3.zero;
        ObjectPooling.GiveBack(Pools.Shell, gameObject);
    }
}
