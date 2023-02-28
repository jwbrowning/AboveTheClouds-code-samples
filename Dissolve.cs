using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team3.Events;

// script to handle an object that will dissolve away when we're done with it
// based on manipulating the dissolve shader attribute
public class Dissolve : MonoBehaviour
{
    public float dissolveSpeed = .1f;
    private float dissolveAmount = 0f;
    [SerializeField] private string eventName = "Dissolve";

    void Start()
    {
        EventsPublisher.Instance.SubscribeToEvent(eventName, HandleDissolve);
    }

    void OnDestroy()
    {
        EventsPublisher.Instance.UnsubscribeToEvent(eventName, HandleDissolve);
    }

    private void HandleDissolve(object sender, object data)
    {
        if ((GameObject)data == gameObject && dissolveAmount == 0)
        {
            StartCoroutine(DissolveChildren());
        }
    }

    private IEnumerator DissolveChildren()
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        while (dissolveAmount < 1f)
        {
            dissolveAmount += dissolveSpeed * Time.deltaTime;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_DissolveAmount", dissolveAmount);
            }
            yield return null;
        }
        //Debug.Log("Island: " + transform.position);

        // IslandGeneration.Instance.RemoveIsland(transform.position);
        Destroy(gameObject);
    }
}
