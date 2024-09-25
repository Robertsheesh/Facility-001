using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeverManager : MonoBehaviour
{
    public List<LeverControl> controlRoomLevers;
    public List<LeverControl> machineLevers;

    public Light doorLight1;
    public Light doorLight2;
    public Light machineLight1;
    public Light machineLight2;
    public Light machineLight3;
    public GameObject waterObject;
    public CameraShake cameraShake;


    public AudioSource machineSound;
    public AudioSource emergencyAudioSource;
    public AudioSource machineOkSound;
    public AudioSource waterSound;

    public MachineStatePanel machineStatePanel;  // For the power slider

    public float waterRiseRate = 0.4f;
    public float maxWaterHeight = 10.6f;
    private float initialWaterY;

    // The fuel mechanism
    public float fuelMaxTime = 50f;
    public float currentFuelTime;
    public float emergencyCountdown = 10f;  // Time before emergency starts after fuel runs out
    private bool powerDepleting = false;    // Track if the power is depleting

    // The pressure mechanism
    public float maxPressure = 100f;  // Max pressure value
    public float currentPressure = 0f;  // Tracks the current pressure
    public float pressureIncreaseRate = 0.1f;  // Rate at which pressure increases per second
    public float pressureThreshold = 100f;  // Pressure threshold that will trigger fuel loss

    public Slider pressureSlider;  // UI slider to display pressure
    private bool pressureTooHigh = false;  // Flag to track if pressure is too high


    private bool gameStarted = false;
    private bool patternMatched = false;
    private bool inEmergency = false;
    private bool isFlashing = false;

    public float initialDelay = 30f;
    public float explorationDelay = 30f;
    public float emergencyDuration = 20f;
    private float timer;
    private bool gameLost = false;


    private float timeLimit = 30f;

    void Start()
    {
        StartGame();
        RandomizeMachineLevers();
        timer = initialDelay;
        initialWaterY = waterObject.transform.position.y;
        currentFuelTime = fuelMaxTime;

        // You can assign the cameraShake variable in the Inspector or find it programmatically like this:
        if (cameraShake == null)
        {
            cameraShake = Camera.main.GetComponent<CameraShake>();
        }
  
    }

    void Update()
    {
        if (!gameStarted) return;

        if (inEmergency)
        {
            // Handle emergency water level rise
            timer -= Time.deltaTime;

            Vector3 newPosition = waterObject.transform.position;
            newPosition.y = Mathf.Clamp(newPosition.y + waterRiseRate * Time.deltaTime, initialWaterY, initialWaterY + maxWaterHeight);
            waterObject.transform.position = newPosition;

            if (timer <= 0)
            {
                EndGame();
            }
        }
        else
        {
            // **Fuel Depletion First**
            if (currentFuelTime > 0)  // Fuel is not yet empty
            {
                currentFuelTime -= Time.deltaTime;

                // Make sure that the power slider does NOT decrease during this time
                if (currentFuelTime <= 0)
                {
                    Debug.Log("Fuel ran out, starting power depletion.");
                    currentFuelTime = 0; // Prevent negative fuel
                    StartPowerDepletion();  // Start depleting power now that fuel is out
                }
            }
            else if (powerDepleting)  // Only deplete power when fuel is out
            {
                // **Power Slider Depletion** - Only now start decreasing power
                machineStatePanel.DecreasePower(Time.deltaTime * 0.8f);  // Slow power depletion

                // Check if the power is fully depleted
                if (machineStatePanel.IsPowerEmpty())
                {
                    Debug.Log("Power ran out, starting emergency.");
                    StartEmergency();  // Start emergency once power is gone
                }
            }

            // **Pressure System**
            if (currentPressure < maxPressure)
            {
                // Increase pressure over time
                currentPressure += pressureIncreaseRate * Time.deltaTime;

                // Check if pressure has exceeded the threshold
                if (currentPressure >= pressureThreshold && !pressureTooHigh)
                {
                    Debug.Log("Pressure too high! Fuel is depleted.");
                    currentPressure = maxPressure;  // Cap the pressure
                    pressureTooHigh = true;  // Set flag to avoid repeated fuel loss

                    // Immediately deplete fuel and start power depletion
                    currentFuelTime = 0;
                    StartPowerDepletion();
                }

                // Update pressure slider (if you have a UI element for it)
                UpdatePressureSlider();
            }

            // Time limit for the round
            timeLimit -= Time.deltaTime;
            if (timeLimit <= 0 && !gameLost)
            {
                gameLost = true;
                IncreaseWaterLevel();
            }
        }
    }


    public void StartPowerDepletion()
    {
        powerDepleting = true;  // Allow power to start decreasing
        machineStatePanel.StartPowerDepletion();  // Reset and start the power slider
    }

    private void UpdatePressureSlider()
    {
        if (pressureSlider != null)
        {
            pressureSlider.value = currentPressure / maxPressure;  // Set slider value based on current pressure
        }
    }

    public void ReleasePressure()
    {
        currentPressure -= 30f;  // Reduce pressure by 30 (or any value)
        currentPressure = Mathf.Clamp(currentPressure, 0f, maxPressure);  // Clamp the pressure to prevent negative values

        pressureTooHigh = false;  // Reset the high pressure flag so the player can avoid fuel loss again
        Debug.Log("Pressure released by valve.");

        UpdatePressureSlider();  // Update the UI
    }

    public void UpdateLeverState(LeverControl lever)
    {
        Debug.Log("Lever state updated for lever: " + lever.name);
        if (gameStarted)
        {
            CheckLeversMatch();
        }
    }

    void RandomizeMachineLevers()
    {
        foreach (var lever in machineLevers)
        {
            lever.leverState = Random.Range(1, 4);
            lever.leverAnimator.SetInteger("LeverState", lever.leverState);
        }
    }

    void CheckLeversMatch()
    {
        patternMatched = true;

        for (int i = 0; i < controlRoomLevers.Count; i++)
        {
            if (controlRoomLevers[i].leverState != machineLevers[i].leverState)
            {
                patternMatched = false;
                break;
            }
        }

        if (patternMatched)
        {
            HandleSuccess();
        }
    }

    void StartEmergency()
    {
        inEmergency = true;
        timer = emergencyDuration;
        cameraShake.TriggerShake(0.5f);

        StopMachineSound();
        PlayEmergencySound();
        StopmachineOkSound();
        StartWaterSound();

        if (!isFlashing)
        {
            StartCoroutine(FlashLights());
            StartCoroutine(MachineLightsFlash());
        }

        RandomizeMachineLevers();
    }

    void EndEmergency()
    {
        inEmergency = false;
        doorLight1.color = Color.green;
        doorLight2.color = Color.green;
        cameraShake.TriggerShake(0.5f);

        if (isFlashing)
        {
            StopCoroutine(FlashLights());
            StartCoroutine(MachineLightsFlash());
            isFlashing = false;
        }

        StopEmergencySound();
        PlaymachineOkSound();
        StopWaterSound();

        Invoke("StartMachineSound", 5);

        // Update this line to ResetPower instead of ResetTimer
        if (machineStatePanel != null)
        {
            machineStatePanel.ResetPower();  // Reset power after emergency ends
        }

        if (currentFuelTime <= 0)
        {
            machineStatePanel.StartPowerDepletion();  // Continue power depletion if fuel is empty
        }
    }


    IEnumerator FlashLights()
    {
        isFlashing = true;

        while (inEmergency)
        {
            doorLight1.color = Color.red;
            doorLight2.color = Color.red;

            yield return new WaitForSeconds(0.5f);

            doorLight1.color = Color.black;
            doorLight2.color = Color.black;

            yield return new WaitForSeconds(0.5f);
        }

        doorLight1.color = Color.green;
        doorLight2.color = Color.green;
    }

    IEnumerator MachineLightsFlash()
    {
        isFlashing = true;

        while (inEmergency)
        {
            machineLight1.color = Color.red;
            machineLight2.color = Color.red;
            machineLight3.color = Color.red;

            yield return new WaitForSeconds(0.5f);

            machineLight1.color = Color.black;
            machineLight2.color = Color.black;
            machineLight3.color = Color.black;

            yield return new WaitForSeconds(0.5f);
        }

        machineLight1.color = Color.white;
        machineLight2.color = Color.white;
        machineLight3.color = Color.white;
    }

    void IncreaseWaterLevel()
    {
        if (waterObject != null)
        {
            Vector3 newPosition = waterObject.transform.position;
            newPosition.y = Mathf.Clamp(newPosition.y + waterRiseRate * Time.deltaTime, initialWaterY, initialWaterY + maxWaterHeight);
            waterObject.transform.position = newPosition;
        }
    }

    void ResetWaterLevel()
    {
        if (waterObject != null)
        {
            waterObject.transform.position = new Vector3(waterObject.transform.position.x, initialWaterY, waterObject.transform.position.z);
        }
    }

    public void RefuelMachine()
    {
        currentFuelTime = fuelMaxTime;
        timer = emergencyCountdown;
        inEmergency = false;
        powerDepleting = false;  // Stop power depletion when refueled
    }

    void StartMachineSound()
    {
        if (machineSound != null)
        {
            machineSound.Play();
        }
    }

    void StopMachineSound()
    {
        if (machineSound != null)
        {
            machineSound.Stop();
        }
    }

    void PlayEmergencySound()
    {
        if (emergencyAudioSource != null)
        {
            emergencyAudioSource.Play();
        }
    }

    void StopEmergencySound()
    {
        if (emergencyAudioSource != null)
        {
            emergencyAudioSource.Stop();
        }
    }

    void PlaymachineOkSound()
    {
        if (machineOkSound != null)
        {
            machineOkSound.Play();
        }
    }

    void StopmachineOkSound()
    {
        if (machineOkSound != null)
        {
            machineOkSound.Stop();
        }
    }

    void StartWaterSound()
    {
        if (waterSound != null)
        {
            waterSound.Play();
        }
    }

    void StopWaterSound()
    {
        if (waterSound != null)
        {
            waterSound.Stop();
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        timer = initialDelay + explorationDelay;
        timeLimit = 30f;
        gameLost = false;
    }

    public void EndGame()
    {
        Debug.Log("Emergency state continues. Game over.");
        IncreaseWaterLevel(); // Ensure water continues to rise on failure
        // Handle other game over mechanics here, like showing a game over screen
    }

    void HandleSuccess()
    {
        EndEmergency();
        ResetWaterLevel();
        gameLost = false;
        timeLimit = 30f;
        timer = initialDelay;
        Invoke("StartEmergency", explorationDelay);
    }
}
