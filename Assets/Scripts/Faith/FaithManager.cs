using UnityEngine;
using Mirror;
using System;

public class FaithManager : NetworkBehaviour
{

    // Any UI script (like FaithBarUI) can subscribe to this
    public static event Action<float, float> OnFaithChanged;

    [SyncVar(hook = nameof(OnCurrentFaithChanged))]
    private float currentFaith = 100f;

    [SyncVar(hook = nameof(OnMaxFaithChanged))]
    private float maxFaith = 100f;

    [Header("Faith Settings")]
    public float passiveDrainPerSecond = 0.005f;

    [Header("Debug Keys (Client Sends Command)")]
    public KeyCode testReduceKey = KeyCode.Minus;    // Press "-" to reduce faith
    public KeyCode testIncreaseKey = KeyCode.Equals; // Press "=" to increase faith

    //Runs on start
    public override void OnStartServer()
    {
        currentFaith = 100f;
        maxFaith = 100f;
        Debug.Log("Server is running");
    }

    //Will run with FPS
    private void Update()
    {
        // Only LOCAL player sends test commands
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(testReduceKey))
                CmdReduceFaith(10f);

            if (Input.GetKeyDown(testIncreaseKey))
                CmdIncreaseFaith(10f);
        }

        // Only server applies passive drain
        if (isServer)
        {
            currentFaith -= passiveDrainPerSecond * Time.deltaTime;
            currentFaith = Mathf.Clamp(currentFaith, 0f, maxFaith);
            Debug.Log(currentFaith);
            Debug.Log("Above is current faith, should be decreasing");
        }
    }


    [Command]
    private void CmdReduceFaith(float amount)
    {
        ReduceFaith(amount);
    }

    [Command]
    private void CmdIncreaseFaith(float amount)
    {
        IncreaseFaith(amount);
    }


    [Server]
    public void ReduceFaith(float amount)
    {
        currentFaith -= amount;
        currentFaith = Mathf.Clamp(currentFaith, 0, maxFaith);
    }

    [Server]
    public void IncreaseFaith(float amount)
    {
        currentFaith += amount;
        currentFaith = Mathf.Clamp(currentFaith, 0, maxFaith);
    }

    [Server]
    public void SetMaxFaith(float value)
    {
        maxFaith = Mathf.Clamp(value, 1f, 100f);
        
        // Reclamp currentFaith to new max
        currentFaith = Mathf.Clamp(currentFaith, 0f, maxFaith);
    }


    private void OnCurrentFaithChanged(float oldValue, float newValue)
    {
        OnFaithChanged?.Invoke(newValue, maxFaith);
    }

    private void OnMaxFaithChanged(float oldValue, float newValue)
    {
        OnFaithChanged?.Invoke(currentFaith, newValue);
    }
}
