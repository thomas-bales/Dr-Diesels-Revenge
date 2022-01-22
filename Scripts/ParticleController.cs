using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public void DestroyParticles(float delay = 0)
    {
        StartCoroutine(DestroyParticlesDelay(delay));
    }

    IEnumerator DestroyParticlesDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
