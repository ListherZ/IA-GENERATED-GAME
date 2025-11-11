using UnityEngine;

namespace SphereTrials
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -10f);
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private float lookSpeed = 12f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPos = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

            Quaternion lookRot = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, lookSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
