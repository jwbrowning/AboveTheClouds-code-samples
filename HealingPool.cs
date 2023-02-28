using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team3.Events;

// event-driven script for the healing pool, which heals the player as they stand in it
public class HealingPool : MonoBehaviour
{
    [SerializeField] private float healSpeed = 100;
    private bool playerInPool = false;

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 3 && col.gameObject.name == "Player")
        {
            col.transform.Find("HealthParticles").gameObject.SetActive(true);
            col.transform.Find("HealthParticles").GetComponent<ParticleSystem>().Play();
            playerInPool = true;
            StartCoroutine(HealPlayer());
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == 3 && col.gameObject.name == "Player")
        {
            col.transform.Find("HealthParticles").gameObject.SetActive(false);
            playerInPool = false;
        }
    }

    private IEnumerator HealPlayer()
    {
        while (playerInPool)
        {
            EventsPublisher.Instance.PublishEvent("HealPlayer", null, healSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
