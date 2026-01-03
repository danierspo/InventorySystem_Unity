using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager instance;

    // Simple implementation: set from the editor
    public int health;
    public int maxHealth;

    // Simple implementation: set from the editor
    public int mana;
    public int maxMana;

    // Simple implementation: set from the editor
    public int stamina;
    public int maxStamina;

    // Simple implementation: set from the editor
    public Image HealthBar;
    public Image ManaBar;
    public Image StaminaBar;

    public void Awake()
    {
        instance = this;
        HealthBar.fillAmount = 1.0f * health / maxHealth;
        ManaBar.fillAmount = 1.0f * mana / maxMana;
        StaminaBar.fillAmount = 1.0f * stamina / maxStamina;
    }

    public void UpdateHealthBar()
    {
        HealthBar.fillAmount = 1.0f * health / maxHealth;
    }

    public void UpdateManaBar()
    {
        ManaBar.fillAmount = 1.0f * mana / maxMana;
    }

    public void UpdateStaminaBar()
    {
        StaminaBar.fillAmount = 1.0f * stamina / maxStamina;
    }
}
