using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        public static Pose GetWorldPose(this Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }

        public static void SetWorldPose(this Transform transform, Pose pose)
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
        }

        public static Vector3 ForwardRestrictPitch(this Transform transform)
        {
            var forward = transform.forward;
            return new Vector3(forward.x, 0.0f, forward.z).normalized;
        }
    }
}