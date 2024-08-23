using Data;
using Extensions;
using UnityEngine;

namespace Behaviours
{
    public class PlayerBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float speed = 5.0f;
        [SerializeField] private float sensitivity = 2.0f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpForce = 8.0f;

        private float _pitch = 0.0f; // For tracking the pitch
        private Vector3 _velocity;
        private bool _isGrounded;

        private void Awake()
        {
            transform.SetWorldPose(GameData.Player.Pose);
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Camera speed
            if (Input.GetKey(KeyCode.E))
                speed += 1.0f;
            if (Input.GetKey(KeyCode.Q))
                speed -= 1.0f;

            UpdateHeadRotations();
            UpdateMovement();
            UpdatePlayerPose();
        }

        private void UpdateHeadRotations()
        {
            var mouseX = Input.GetAxis("Mouse X") * sensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, -89.9f, 89.9f); // Clamping pitch

            head.transform.rotation = Quaternion.Euler(_pitch, head.transform.rotation.eulerAngles.y + mouseX, 0.0f);
        }

        private void UpdateMovement()
        {
            _isGrounded = characterController.isGrounded;

            var direction = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
                direction.z += 1.0f;
            if (Input.GetKey(KeyCode.S))
                direction.z -= 1.0f;
            if (Input.GetKey(KeyCode.D))
                direction.x += 1.0f;
            if (Input.GetKey(KeyCode.A))
                direction.x -= 1.0f;
            if (Input.GetKey(KeyCode.Space) && _isGrounded)
                _velocity.y = jumpForce;

            if (direction.sqrMagnitude > 0.0f)
            {
                direction = head.right * direction.x + Vector3.up * direction.y + head.forward * direction.z;
                direction.Normalize();
            }

            // Apply gravity
            // _velocity.y += gravity * Time.deltaTime;

            // Move the character
            characterController.Move((direction * speed + _velocity) * Time.deltaTime);
        }

        private void UpdatePlayerPose()
        {
            GameData.Player.Pose = head.GetWorldPose();
        }
    }
}
