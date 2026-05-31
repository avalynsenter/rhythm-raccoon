using UnityEngine;

public class NullPowerup : Powerup
{
    public override void ApplyEffect()
    {
        Debug.Log("applied nullPowerup!");
    }
}