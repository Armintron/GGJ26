using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Oxygen Settings")]
    public float maxOxygen = 100f;
    public float currentOxygen;
    public event Action<float> OnOxygenChanged;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public event Action<float> OnHealthChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentOxygen = maxOxygen;
        currentHealth = maxHealth;
    }

    public void AddOxygen(float amount)
    {
        currentOxygen = Mathf.Clamp(currentOxygen + amount, 0, maxOxygen);
        OnOxygenChanged?.Invoke(currentOxygen / maxOxygen);
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }
}