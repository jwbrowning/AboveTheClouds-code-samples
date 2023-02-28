using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;

namespace Team3.Player
{

    // Script to manage the camera targeting system, reminicient of z targeting in Zelda games
    public class TargetingManager : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera defaultCamera;
        [SerializeField] private CinemachineVirtualCamera targetingCamera;
        [SerializeField] private Transform enemyTarget;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float sphereRadius = 30;
        [SerializeField] private float tooClose = 5;
        [SerializeField] public Material outlineMaterial;
        private PlayerStateManager stateManager;
        private GameObject currentEnemy = null;
        private bool targeting = false;

        void Awake()
        {
            stateManager = GetComponent<PlayerStateManager>();
            if (cameraTransform == null) cameraTransform = Camera.main.transform;
            if (defaultCamera == null) defaultCamera = GameObject.Find("DefaultCamera").GetComponent<CinemachineVirtualCamera>();
            if (targetingCamera == null) targetingCamera = GameObject.Find("TargetingCamera").GetComponent<CinemachineVirtualCamera>();

            Events.EventsPublisher.Instance.SubscribeToEvent("EnterTargetingState", StartTargeting);
            Events.EventsPublisher.Instance.SubscribeToEvent("ExitTargetingState", StopTargeting);
            Events.EventsPublisher.Instance.SubscribeToEvent("Target", HandleTargetEvent);
            Events.EventsPublisher.Instance.SubscribeToEvent("DeadEntity", CheckDeadEnemy);
        }

        private void CheckDeadEnemy(object sender, object data)
        {
            if (((GameObject)data) == currentEnemy)
            {
                stateManager.StopTargeting();
            }
        }

        private void StartTargeting(object sender, object data)
        {
            targetingCamera.Priority = 10;
            defaultCamera.Priority = 0;
            targeting = true;
            StartCoroutine(Target());
        }

        private void StopTargeting(object sender, object data)
        {
            targetingCamera.Priority = 0;
            defaultCamera.Priority = 10;
            targeting = false;
            DisableOutline(currentEnemy);
            currentEnemy = null;
        }

        private void HandleTargetEvent(object sender, object data)
        {
            if (targeting)
            {
                TargetClosestEnemy();
            }
            else
            {
                stateManager.StartTargeting();
            }
        }

        private void EnableOutline(GameObject enemy)
        {
            if (currentEnemy != null)
            {
                foreach (var skinnedMeshRenderer in currentEnemy.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var currentMaterials = skinnedMeshRenderer.materials;
                    List<Material> newMaterials = new List<Material>();
                    foreach (Material material in currentMaterials)
                    {
                        newMaterials.Add(material);
                    }
                    newMaterials.Add(outlineMaterial);
                    skinnedMeshRenderer.materials = newMaterials.ToArray();
                }
            }
        }

        private void DisableOutline(GameObject enemy)
        {
            if (currentEnemy != null)
            {
                foreach (var skinnedMeshRenderer in currentEnemy.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var currentMaterials = skinnedMeshRenderer.materials;
                    List<Material> newMaterials = new List<Material>();
                    foreach (Material material in currentMaterials)
                    {
                        if (!material.name.Contains(outlineMaterial.name))
                        {
                            newMaterials.Add(material);
                            // Debug.Log("OL: " + material.name + " " + outlineMaterial.name);
                        }
                    }
                    skinnedMeshRenderer.materials = newMaterials.ToArray();
                    // Debug.Log("OUTLINE: " + currentMaterials.Contains(outlineMaterial));
                }
            }
        }

        private IEnumerator Target()
        {
            currentEnemy = null;
            TargetClosestEnemy();
            while (targeting && currentEnemy != null)
            {
                cameraTarget.transform.rotation = cameraTransform.transform.rotation;
                if (Vector3.Distance(currentEnemy.transform.position, transform.position) > sphereRadius)
                {
                    stateManager.StopTargeting();
                    break;
                }
                enemyTarget.position = currentEnemy.transform.position;
                if (Vector3.Distance(enemyTarget.position, transform.position) < tooClose)
                {
                    Vector3 temp = transform.position + (enemyTarget.position - transform.position).normalized * tooClose;
                    temp.y = enemyTarget.position.y;
                    enemyTarget.position = temp;
                }
                Debug.DrawRay(transform.position, enemyTarget.position - transform.position, Color.blue);
                yield return null;
            }
        }

        private void TargetClosestEnemy()
        {
            DisableOutline(currentEnemy);
            var colliders = Physics.OverlapSphere(transform.position, sphereRadius, layerMask).ToList();
            colliders.Sort((a, b) => { return Vector3.Distance(transform.position, a.transform.position).CompareTo(Vector3.Distance(transform.position, b.transform.position)); });
            if (colliders.Count > 0)
            {
                if (colliders[0].gameObject == currentEnemy)
                {
                    if (colliders.Count > 1)
                    {
                        currentEnemy = colliders[1].gameObject;
                    }
                    else 
                    {
                        currentEnemy = null;
                    }
                }
                else 
                {
                    currentEnemy = colliders[0].gameObject;
                }
            }
            else
            {
                currentEnemy = null;
            }
            if (currentEnemy == null)
            {
                stateManager.StopTargeting();
            }
            EnableOutline(currentEnemy);
        }
    }
}
