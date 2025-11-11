using UnityEngine;

namespace SphereTrials
{
    public class GoalPoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.PlayerReachedGoal();
            }
        }
    }
}
