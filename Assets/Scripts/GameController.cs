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

    public GameObject gameOverText;
    public GameObject victoryText;

    public TMP_Text hintText;

    private float timeLeft;
    private float gameOverLockout;
    private bool updatedGameOverText = false;

    private bool gameOver = false;

    private List<AInteractable> interactableObjects = new List<AInteractable>();

    private float totalGameTime = 0f;
    private bool hint2shown = false;
    private bool hint3shown = false;
    private bool hint3Ashown = false;
    private bool hint4shown = false;
    private bool hint5shown = false;

    private bool aphidSpawned = false;
    private bool aphidSmooshed = false;

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
            gameOverLockout -= Time.deltaTime;
            if (gameOverLockout < 0f && !updatedGameOverText)
            {
                updatedGameOverText = true;
                gameOverText.SetActive(true);
                victoryText.SetActive(true);
            }

            if (Keyboard.current.cKey.isPressed)
            {
                SceneManager.LoadScene("Credits");
            }
            else if (Keyboard.current.anyKey.isPressed && gameOverLockout < 0f)
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

        totalGameTime += Time.deltaTime;

        if (totalGameTime > 5f && !hint2shown)
        {
            hint2shown = true;
            hintText.text = "Gather water and nutrients for the tree!";
        }

        if (totalGameTime > 8f && !hint3shown && hint2shown && ladybugController.IsCarryingItem())
        {
            hint3shown = true;
            hintText.text = "Bring the item back to the roots of the tree!";
        }

        if (!hint3Ashown && !hint4shown && hint3shown && !ladybugController.IsCarryingItem())
        {
            hint3Ashown = true;
            hintText.text = "Keep supplying the tree!";
        }

        if (!hint4shown && hint3shown && aphidSpawned)
        {
            hint4shown = true;
            hintText.text = "An aphid wants to chew the roots of the tree.  Stop it!";
        }

        if (!hint5shown && hint4shown && aphidSmooshed)
        {
            hint5shown = true;
            hintText.text = "Keep defending the tree and providing its needs!";
        }

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
                        aphidSpawned = true;
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

                if (interactable is AphidController)
                {
                    aphidSmooshed = true;
                }

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
        gameOverLockout = 3;
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        gameOverLockout = 3;
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
