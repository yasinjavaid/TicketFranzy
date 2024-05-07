using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using MoreLinq;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class ArcadeNPCBehaviour : MonoBehaviour
{
    public struct NPCPlayInfo
    {
        public Vector3 position; public string machineID;
        public NPCPlayInfo(Vector3 position, string machineID)
        {
            this.position = position;
            this.machineID = machineID;
        }
    }

    public static Dictionary<string, NPCPlayInfo> NPCPositions = new Dictionary<string, NPCPlayInfo>();

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private ArcadeMachine myMachine = null;
    public string UniqueID => uniqueID ??= gameObject.GetFullName();
    private string uniqueID;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (ScenesTime.SceneUnloadedTimes.TryGetValue(SceneManager.GetActiveScene().buildIndex, out DateTime sceneUnloadedTime) &&
           (DateTime.Now - sceneUnloadedTime) < TimeSpan.FromSeconds(30f) && NPCPositions.TryGetValue(UniqueID, out NPCPlayInfo value))
        {
            transform.position = value.position;
            myMachine = ArcadeMachine.AllMachines[value.machineID];
        }
        else PlayGameOrPatrol();
    }

    private void PlayGameOrPatrol()
    {
        if (Random.Range(0, 2) == 1) GoToEmptyMachine();
        else PatrolInWaitingArea();
    }

    private void GoToEmptyMachine()
    {
        IEnumerable<ArcadeMachine> availableMachines = ArcadeMachine.AllMachines.Values.Where(x => !x.InUse && !x.Spotted);
        if (availableMachines.Any())
        {
            myMachine = availableMachines.RandomSubset(1).First();
            myMachine.Spotted = true;

            StartCoroutine(CheckForDestinationRoutine(myMachine.WaitingPosition, PlayOnMachine));
        }
        else PatrolInWaitingArea();
    }

    IEnumerator CheckForDestinationRoutine(Vector3 destinationPos, Action onComplete)
    {
        navMeshAgent.enabled = true;

        navMeshAgent.SetDestination(destinationPos);

        while (Vector3.Distance(destinationPos, transform.position) > 1f)
        {
            animator.SetFloat("Walking", navMeshAgent.speed);
            yield return null;
        }

        navMeshAgent.enabled = false;

        animator.SetFloat("Walking", 0f);

        onComplete?.Invoke();
    }

    private void PlayOnMachine()
    {
        if (myMachine.InUse)
        {
            myMachine.Spotted = false;
            PatrolInWaitingArea();
        }
        else
        {
            transform.position = myMachine.SittingPosition;
            transform.LookAt(new Vector3(myMachine.transform.position.x, transform.position.y, myMachine.transform.position.z));

            animator.SetTrigger("InsertToken");
            myMachine.UseMachine();

            StartCoroutine(PlayOnMachineForTime());
        }
    }

    private void PatrolInWaitingArea() => StartCoroutine(CheckForDestinationRoutine(ArcadePatrolArea.GetRandomPosition(), StartWaiting));

    private void StartWaiting() => StartCoroutine(PlayGameAfterWaitingRoutine());

    IEnumerator PlayGameAfterWaitingRoutine()
    {
        yield return new WaitForSeconds(Random.Range(10f, 30f));
        PlayGameOrPatrol();
    }

    IEnumerator PlayOnMachineForTime()
    {
        yield return new WaitForSeconds(Random.Range(5f, 20f));
        myMachine.LeaveMachine();

        transform.position = myMachine.WaitingPosition;

        myMachine = null;

        PlayGameOrPatrol();
    }

    private void OnDisable()
    {
        if (myMachine == null) NPCPositions.Remove(UniqueID);
        else NPCPositions[UniqueID] = new NPCPlayInfo(transform.position, myMachine.UniqueID);
    }
}
