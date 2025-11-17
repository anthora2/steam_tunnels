using Mirror;
using TMPro;
using UnityEngine;

public class GameClockNetworked : NetworkBehaviour
{
    [Header("Clock UI")]
    [SerializeField] private TextMeshProUGUI clockText;

    [Header("Time Settings")]
    [SerializeField] private float secondsPerTick = 95f;

    private float timer;

    [SyncVar(hook = nameof(OnClockStringChanged))]
    private string clockString = "9:00 PM";

    // In-game time
    private int hour = 9;
    private int minute = 0;
    private bool isPM = true;

    void Start()
    {
        // Only the APPEARANCE changes on clients
        clockText.text = clockString;
    }

    void Update()
    {
        if (!isServer) return;  // ONLY THE SERVER RUNS THE CLOCK

        timer += Time.deltaTime;

        if (timer >= secondsPerTick)
        {
            timer = 0f;
            AdvanceClock();
        }
    }

    [Server]
    private void AdvanceClock()
    {
        minute += 15;

        if (minute >= 60)
        {
            minute = 0;
            hour++;

            if (hour == 12)
                isPM = !isPM;

            if (hour > 12)
                hour = 1;
        }

        // Update SyncVar — Mirror auto sends this to all clients
        clockString = FormatClockString();

        if (hour == 4 && minute == 0 && !isPM)
        {
            Debug.Log("Night is over!");
        }
    }

    private string FormatClockString()
    {
        string suffix = isPM ? "PM" : "AM";
        return $"{hour}:{minute:00} {suffix}";
    }

    // This runs ON CLIENTS when SyncVar updates
    private void OnClockStringChanged(string oldValue, string newValue)
    {
        clockText.text = newValue;
    }
}
