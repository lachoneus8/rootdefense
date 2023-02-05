using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public enum ESpawnType
    {
        WaterDroplet,
        NutrientBag,
        Aphid
    }

    [Serializable]
    public class SpawnTimeRecord
    {
        public ESpawnType spawnType;
        public float timeBeforeFirstSpawn;
        public float spawnDelay;
        public GameObject prefab;

        [HideInInspector]
        public float timeToSpawn = 0f;
    }

    public TMP_Text timerText;
    public float gameTimeSec;

    public List<SpawnTimeRecord> spawnRecords;

    public TreeController treeController;

    public float minSpawnDistance;
    public float maxSpawnDistance;

    public LadybugController ladybugController;

    public float interactionDistance;

    public float minDistanceToCheckRoots;

    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    private float timeLeft;

    private bool gameOver = false;

    private List<AInteractable> interactableObjects = new List<AInteractable>();

    public bool IsGameOver()
    {
        return gameOver;
    }

    // Start is called before the first frame update
    void Start()
    {
        timeLeft = gameTimeSec;

        foreach (var record in spawnRecords)
        {
            record.timeToSpawn = record.timeBeforeFirstSpawn;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver)
        {
            if (Keyboard.current.cKey.isPressed)
            {
                SceneManager.LoadScene("Credits");
            }
            else if (Keyboard.current.anyKey.isPressed)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            return;
        }

        if (treeController.GetHealth() <= 0f)
        {
            gameOver = true;
            ShowGameOver();
            return;
        }

        timeLeft -= Time.deltaTime;

        if (timeLeft < 0)
        {
            timeLeft = 0;
        }

        var minutes = (int)(timeLeft / 60);
        var seconds = (int)(timeLeft % 60);

        timerText.text = minutes + ":" + string.Format("{0:0#}", seconds);

        if (timeLeft == 0)
        {
            gameOver = true;
            ShowVictory();
            return;
        }

        foreach (var spawnRecord in spawnRecords)
        {
            spawnRecord.timeToSpawn -= Time.deltaTime;
            if (spawnRecord.timeToSpawn < 0f)
            {
                spawnRecord.timeToSpawn = spawnRecord.spawnDelay;
                switch (spawnRecord.spawnType)
                {
                    case ESpawnType.WaterDroplet:
                        SpawnPrefab<WaterDroplet>(spawnRecord);
                        break;
                    case ESpawnType.NutrientBag:
                        SpawnPrefab<NutrientBag>(spawnRecord);
                        break;
                    case ESpawnType.Aphid:
                        SpawnPrefab<AphidController>(spawnRecord);
                        break;
                }
                
            }
        }

        var toDelete = new List<AInteractable>();

        foreach (var interactable in interactableObjects)
        {
            var distance = ladybugController.transform.position - interactable.transform.position;
            if (distance.magnitude < interactionDistance)
            {
                bool destroyInteractable;
                interactable.Interact(ladybugController, out destroyInteractable);

                if (destroyInteractable)
                {
                    toDelete.Add(interactable);
                }
            }
        }

        foreach (var deleteMe in toDelete)
        {
            interactableObjects.Remove(deleteMe);
            Destroy(deleteMe.gameObject);
        }

        if (ladybugController.IsCarryingItem())
        {
            var rootParent = treeController.GetClosestRootParent(ladybugController.gameObject, minDistanceToCheckRoots);

            if (rootParent != null)
            {
                var rootObject = rootParent.GetClosestRoot(ladybugController.gameObject);
                var diff = rootObject.transform.position - ladybugController.transform.position;

                if (diff.magnitude < interactionDistance)
                {
                    bool wasWater;
                    ladybugController.RemoveCarriedItem(out wasWater);

                    if (wasWater)
                    {
                        treeController.ApplyWater();
                    }
                    else
                    {
                        treeController.ApplyNutrients();
                    }
                }
            }
        }
    }

    public void RemoveInteractable(AInteractable interactable)
    {
        interactableObjects.Remove(interactable);
    }

    private void ShowVictory()
    {
        victoryPanel.SetActive(true);
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    private void SpawnPrefab<T>(SpawnTimeRecord spawnRecord) where T:AInteractable
    {
        var distance = UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance);
        var direction = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        var spawnPosition = new Vector3(Mathf.Cos(direction) * distance, 0f, Mathf.Sin(direction) * distance);
        var spawnedObject = Instantiate(spawnRecord.prefab, spawnPosition, Quaternion.identity, transform);

        AInteractable interactable = spawnedObject.GetComponent<T>();
        interactableObjects.Add(interactable);
        interactable.gameController = this;
    }
}
