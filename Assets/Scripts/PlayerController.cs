using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;

    [Header("Animation")]
    // We need a reference to the Nurse's Animator
    public Animator characterAnimator;

    private CharacterController controller;
    private Camera playerCamera;
    private float verticalRotation = 0f;

    public float interactDistance = 3f;
    public UIManager uiManager;
    public ToastManager toastManager;
    private HotspotInteractable currentHotspot;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
// Only check for hotspots and allow pressing 'E' if the UI is NOT open
        if (!uiManager.isUIOpen)
        {
            CheckForHotspot();

            if (currentHotspot != null && Input.GetKeyDown(KeyCode.E))
            {
                Interact(currentHotspot);
            }
        }
        else
        {
            // If the UI is open, make sure we aren't showing a leftover "Press E" toast
            currentHotspot = null;
            //toastManager.HideToast();
        }
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        float horizontal = 0f; // A/D
        float vertical   = 0f;   // W/S


        // ONLY read keyboard input if the UI is closed
        if (!uiManager.isUIOpen)
        {
            horizontal = Input.GetAxis("Horizontal"); // A/D
            vertical = Input.GetAxis("Vertical");   // W/S
        }

        // Move relative to where the player is facing
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // If we attached an animator, check if the player is pressing keys
        if (characterAnimator != null)
        {
            // If horizontal or vertical input is greater than a tiny amount, we are moving
            bool isMoving = (Mathf.Abs(horizontal) > 0.0001f || Mathf.Abs(vertical) > 0.0001f);
            
            // Send that true/false value to our Mixamo Animator flowchart!
            characterAnimator.SetBool("isWalking", isMoving);
        }
    }

    void HandleMouseLook()
    {

        if(uiManager.isUIOpen) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the whole player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Tilt the camera up/down (clamped so you can't flip upside down)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

     private void CheckForHotspot()
    {
        currentHotspot = null;

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            currentHotspot = hit.collider.GetComponent<HotspotInteractable>();

            if (currentHotspot != null)
            {
                toastManager.ShowToast("Press E to interact",0.1f);
            }
        }
    }

    private void Interact(HotspotInteractable hotspot)
    {
        switch (hotspot.hotspotType)
        {
            case HotspotType.EHR:
                uiManager.OpenEHR();
                break;

            case HotspotType.Monitor:
                uiManager.OpenVitals();
                break;

            case HotspotType.Patient:
                uiManager.OpenPatient();
                break;

            case HotspotType.Ventilator:
                uiManager.OpenVentilator();
                break;

            case HotspotType.CallSystem:
                uiManager.OpenCallSystem();
                break;
        }
    }
}