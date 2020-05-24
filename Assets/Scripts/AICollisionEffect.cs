using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICollisionEffect : MonoBehaviour
{
    [SerializeField] private TimeController timeController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timeController.timeLeft = timeController.timeLeft - Mathf.RoundToInt(Time.deltaTime);
        }
    }
}
