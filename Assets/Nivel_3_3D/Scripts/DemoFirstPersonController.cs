using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScifiOffice {
    public class DemoFirstPersonController : MonoBehaviour {

        Rigidbody rb;
        CapsuleCollider col;
        bool isCrouching;

        [SerializeField] public Transform playerBody;

        public enum ControlType { android, keyboard, keyboardMouse };
        public ControlType controlType;

        [Header("Movement")]
        public float speed = 3f;
        public float accelerationRate = 12f, crouchFactor = 0.5f, decelerationFactor = 1f;
        public float mouseSensitivity = 50f;
        [SerializeField] private float groundCheckHeight = 1f;
        [SerializeField] private float groundCheckDistance = 4f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float customGravity = 15f;
        private float groundOffset;
        private float verticalVelocity;
        private bool isGrounded = true;
        private bool jumpRequested;

        float xRot = 0f;
        float horizontalMovement;
        float verticalMovement;

        [Header("HUD")]
        public GameObject canvas;      


        private void Start() {
            if (playerBody == null) {
                playerBody = transform.root;
            }

            rb = playerBody.GetComponent<Rigidbody>();
            col = playerBody.GetComponent<CapsuleCollider>();

            if (rb == null || col == null) {
                Debug.LogError(
                    "DemoFirstPersonController necesita un Rigidbody y un CapsuleCollider " +
                    "en el objeto asignado como Player Body.",
                    this
                );
                enabled = false;
                return;
            }

            // La altura se ajusta siguiendo rampas, sin usar gravedad.
            rb.useGravity = false;
            rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
            rb.linearVelocity = Vector3.zero;
            groundOffset = rb.position.y - col.bounds.min.y;
            
            if(controlType == ControlType.keyboardMouse)
                Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        void Update() {
            
            Look();

            if (Input.GetKeyDown(KeyCode.Space)) {
                jumpRequested = true;
            }

            if (controlType == ControlType.android)
            {
                //Show mobile controls
                SetMobileCanvasActive(true);
            }
            else
            {
                //Do not show mobile controls when using keyboard controls
                Crouch();
                SetMobileCanvasActive(false);
            }



        }

        private void FixedUpdate() {
            Walk();
        }

        private void SetMobileCanvasActive(bool active) {
            // El canvas solo es necesario para los controles de Android.
            // Si no fue asignado en el Inspector, evitamos una excepción.
            if (canvas != null) {
                canvas.SetActive(active);
            }
        }

        private void OnDisable() {
            if (Cursor.lockState == CursorLockMode.Locked) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void Look() {
            float mouseX = 0;
            float mouseY = 0;

            switch(controlType) {
                case ControlType.android:
                    mouseX = horizontalMovement * Time.deltaTime * mouseSensitivity;
                    break;

                case ControlType.keyboard:
                    //Get changes to look left and right only. Player cannot look up and down.
                    mouseX = Input.GetAxis("Horizontal") * mouseSensitivity * Time.deltaTime;
                    mouseY = 0;
                    break;

                default:
                case ControlType.keyboardMouse:
                    //Use mouse to control where to look. Can look in all directions.
                    mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                    mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
                    break;
            }

            //rotate playerBody
            xRot -= mouseY;
            xRot = Mathf.Clamp(xRot, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        void Walk() {
            Vector3 displacement;
            float maxSpeed = speed, maxAcc = accelerationRate;

            // Lower the limits if we are crouching.
            if (isCrouching) {
                maxSpeed *= crouchFactor;
                maxAcc *= crouchFactor;
            }

            //Find displacement based on controlType.
            switch(controlType) {
                case ControlType.android:
                    //Move forward and back only. Horizontal turns.
                    displacement = playerBody.transform.forward * verticalMovement;
                    break;

                case ControlType.keyboard:
                    //Only can move forward and back
                    displacement = playerBody.transform.forward * Input.GetAxis("Vertical");
                    break;

                case ControlType.keyboardMouse:
                default:
                    //Move in 4 directions, this is the default control
                    displacement = playerBody.transform.forward * Input.GetAxis("Vertical") + playerBody.transform.right * Input.GetAxis("Horizontal");
                    break;
            }

            displacement = Vector3.ProjectOnPlane(displacement, Vector3.up);
            float len = displacement.magnitude;
            Vector3 velocity = rb.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

            if(len > 0) {
                horizontalVelocity += displacement / len * Time.fixedDeltaTime * maxAcc;

                // Limita solamente el movimiento horizontal y conserva la gravedad.
                if(horizontalVelocity.magnitude > maxSpeed) {
                    horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                }
            } else {
                // Si no hay entrada, frena solo en horizontal.
                len = horizontalVelocity.magnitude;
                float decelRate = accelerationRate * decelerationFactor * Time.fixedDeltaTime;
                if(len < decelRate) horizontalVelocity = Vector3.zero;
                else {
                    horizontalVelocity -= horizontalVelocity.normalized * decelRate;
                }
            }

            UpdateVerticalMovement(horizontalVelocity);
            rb.linearVelocity = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
        }

        private void UpdateVerticalMovement(Vector3 horizontalVelocity) {
            if (jumpRequested && isGrounded) {
                verticalVelocity = jumpForce;
                isGrounded = false;
            }
            jumpRequested = false;

            Vector3 nextPosition = rb.position + horizontalVelocity * Time.fixedDeltaTime;
            Vector3 rayOrigin = new Vector3(
                nextPosition.x,
                rb.position.y + groundCheckHeight,
                nextPosition.z
            );

            RaycastHit[] hits = Physics.RaycastAll(
                rayOrigin,
                Vector3.down,
                groundCheckDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

            float closestDistance = float.MaxValue;
            float groundY = rb.position.y;

            foreach (RaycastHit hit in hits) {
                if (hit.collider == col || hit.distance >= closestDistance) {
                    continue;
                }

                closestDistance = hit.distance;
                groundY = hit.point.y + groundOffset;
            }

            bool groundFound = closestDistance < float.MaxValue;

            if (isGrounded && groundFound) {
                rb.position = new Vector3(rb.position.x, groundY, rb.position.z);
                verticalVelocity = 0f;
                return;
            }

            verticalVelocity -= customGravity * Time.fixedDeltaTime;
            float nextY = rb.position.y + verticalVelocity * Time.fixedDeltaTime;

            if (groundFound && verticalVelocity <= 0f && nextY <= groundY) {
                rb.position = new Vector3(rb.position.x, groundY, rb.position.z);
                verticalVelocity = 0f;
                isGrounded = true;
            } else if (!groundFound && verticalVelocity <= 0f) {
                // No permite caer al vacío cuando falta un collider de suelo.
                verticalVelocity = 0f;
                isGrounded = true;
            } else {
                rb.position = new Vector3(rb.position.x, nextY, rb.position.z);
            }
        }

        void Crouch() {
            //Crouch when the couch key is being pressed
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift)) {
                col.height = .5f;
                isCrouching = true; 
            } else {
                //Otherwise, player stop crouching
                col.height = 2;
                //if (Input.GetKey(KeyCode.LeftShift)) {
                //    isCrouching = true;
                //    return;
                //}
                isCrouching = false;
            }
        }

        //crouching for android build
        public void MobileCrouch()
        {
            //If player is currently crouching, stop crouching and vice versa
            if(isCrouching)
            {
                col.height = 2;
                isCrouching = false;
            }
            else
            {
                col.height = .5f;
                isCrouching = true;
            }
        }

        //setting movement for android build
        public void MobileWalk(int direction)
        {
            
            if(direction * direction == 1)
            {
                //Moving left and right
                horizontalMovement = direction;
            }
            else if(direction == 3)
            {
                //When none of the button is pressed, stop moving
                horizontalMovement = 0;
                verticalMovement = 0;
            }
            else
            {
                //Moving forward and back
                verticalMovement = direction - 1;
            }
            
            
        }


    }
}
