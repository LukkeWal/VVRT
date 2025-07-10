using _Project.Ray_Tracer.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

namespace _Project.Scripts
{
    /// <summary>
    /// A camera controller that mimics the controls and behavior of the Unity editor camera. Adapted from
    /// http://wiki.unity3d.com/index.php?title=MouseOrbitZoom.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        private int activeFingerId = -1;
        private float lastTapTimestamp = -1f;
        private Vector2 lastTapPos;
        const float MaxTapInterval = 0.3f;      // seconds
        const float MaxTapMoveSqr   = 200f*200f;  // pixels²
        [SerializeField]
        private RectTransform inputBlocker;
        public bool InputBlockerHovered { get; set; }

        public Transform Target;
        public float InitialDistance = 5.0f;
        public float MaxDistance = 25.0f;
        public float MinDistance = 0.5f;
        public float YMaxLimit = 90.0f;
        public float YMinLimit = -90.0f;
        public float OrbitSpeed = 4.0f;
        public float PanSpeed = 2.0f;
        public float ZoomSpeed = 1.0f;

        [Serializable]
        public class Event : UnityEvent { }
        public Event onPanChanged, OnOrbitChanged, OnZoomChanged;

        private float xDegrees = 0.0f;
        private float yDegrees = 0.0f;
        private float distance = 0.0f;

        private bool zoom = false;
        private bool orbiting = false;
        private bool panning = false;
        private bool mode = false;

        bool IsTouchOverHandle()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                Ray ray = Camera.main.ScreenPointToRay(t.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Gizmos")))
                {
                    return true;
                }
            }
            return false;
        }

        bool IsDoubleTap(Touch t){
            float now = Time.time;
            Vector2 pos = t.position;
            if (lastTapTimestamp < 0.0f){
                lastTapTimestamp = now;
                lastTapPos = pos;
                return false;
            }
            float dt = now - lastTapTimestamp;
            float dist2 = (pos - lastTapPos).sqrMagnitude;
            if (dt <= MaxTapInterval && dist2 <= MaxTapMoveSqr)
            {
                lastTapTimestamp = now;
                lastTapPos = pos;
                return true;
            }
            lastTapTimestamp = now;
            lastTapPos = pos;
            return false;

        }

        void Start()
        {
            Input.multiTouchEnabled = true;
            Initialize();
#if UNITY_WEBGL
            // When using WebGL the RTCamera was overriding the MainCamera in the ray casting level
            Debug.Log("Using WebGL: explicitly disabling RT Camera since it overrides the main camera");
            Camera myCamera = GetComponent<Camera>();
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam != myCamera)
                {
                    if (cam.name == "RT Camera"){
                        Debug.Log("disabling camera: " + cam.name);
                        cam.enabled = false;
                    }
                }
            }

            myCamera.enabled = true;
#endif
        }

        void OnEnable() { Initialize(); }

        /// <summary>
        /// Initialize this camera controller. Will rotate the camera to look at the provided <see cref="Target"/>. If no
        /// target is provided a new target will be placed <see cref="InitialDistance"/> in front of the camera.
        /// </summary>
        public void Initialize()
        {
            // If there is no target, create a target at the given initial distance from the camera's current viewpoint.
            if (!Target)
            {
                GameObject go = new GameObject("Camera Target");
                go.transform.position = transform.position + (transform.forward * InitialDistance);
                Target = go.transform;
            }

            // Store the distance to the target and camera rotation.
            distance = Vector3.Distance(transform.position, Target.position);
            xDegrees = transform.rotation.eulerAngles.y;
            yDegrees = transform.rotation.eulerAngles.x;

            // Calculate the initial position based on our rotation and the distance to the target.
            transform.position = Target.position - (transform.rotation * Vector3.forward * distance);
        }

        public void SetCursor()
        {
            if (zoom || panning || orbiting)
                return;
            GlobalManager.Get().SetCursor(CursorType.ModeCursor);
        }

        public void ResetCursor()
        {
            if (zoom || panning || orbiting)
                return;

            GlobalManager.Get().ResetCursor();
        }

        private void DisableBlocker()
        {
            GlobalManager globalSettings = GlobalManager.Get();
            if (mode)
            {
                globalSettings.SetCursor(CursorType.ModeCursor);
                return;
            }
            globalSettings.ResetCursor();
            inputBlocker.gameObject.SetActive(false);
            InputBlockerHovered = false;
        }

        private void PanningUpdate()
        {
            if (activeFingerId != -1){
                if(TouchPanningUpdate()){
                    return;
                } else{
                    panning = false;
                    DisableBlocker();
                    activeFingerId = -1;
                    //Debug.Log("Stopped panning through touch");
                    return;
                }
            }
            float xDistance = 0.0f;
            float yDistance = 0.0f;

            // Grab the rotation of the camera so we can move in a pseudo local XY space.
            if (Input.GetMouseButton(2))
            {
                xDistance = -Input.GetAxis("Mouse X") * 0.01f;
                yDistance = -Input.GetAxis("Mouse Y") * 0.01f;
            }


            // And we pan with the arrow keys
            if (Input.GetKey(KeyCode.LeftArrow))
                xDistance -= Time.deltaTime * 0.5f;
            if (Input.GetKey(KeyCode.RightArrow))
                xDistance += Time.deltaTime * 0.5f;
            if (Input.GetKey(KeyCode.UpArrow))
                yDistance += Time.deltaTime * 0.5f;
            if (Input.GetKey(KeyCode.DownArrow))
                yDistance -= Time.deltaTime * 0.5f;

            if (yDistance != 0f || xDistance != 0f)
                onPanChanged?.Invoke();

            // we pan by way of transforming the target in screen space.
            // Grab the rotation of the camera so we can move in a pseudo local XY space.
            Target.rotation = transform.rotation;
            Target.Translate(Vector3.right * (xDistance * PanSpeed * distance));
            Target.Translate(Vector3.up * (yDistance * PanSpeed * distance));

            if (!(Input.GetMouseButton(2) ||
                  Input.GetKey(KeyCode.LeftArrow) || 
                  Input.GetKey(KeyCode.RightArrow) ||
                  Input.GetKey(KeyCode.UpArrow) || 
                  Input.GetKey(KeyCode.DownArrow) ||
                  (Input.touchCount == 1)))
            {
                panning = false;
                DisableBlocker();
            }
        }

        private void OrbitingUpdate()
        {
            if(activeFingerId != -1){
                if(TouchOrbitUpdate()){
                    return;
                } else{
                    orbiting = false;
                    activeFingerId = -1;
                    DisableBlocker();
                }
            }

            float xDistance = 0.0f;
            float yDistance = 0.0f;

            if (Input.GetMouseButton(0))
            {
                xDistance += Input.GetAxis("Mouse X") * OrbitSpeed;
                yDistance -= Input.GetAxis("Mouse Y") * OrbitSpeed;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
                xDistance += Time.deltaTime * 20.0f * OrbitSpeed;
            if (Input.GetKey(KeyCode.RightArrow))
                xDistance -= Time.deltaTime * 20.0f * OrbitSpeed;
            if (Input.GetKey(KeyCode.UpArrow))
                yDistance += Time.deltaTime * 20.0f * OrbitSpeed;
            if (Input.GetKey(KeyCode.DownArrow))
                yDistance -= Time.deltaTime * 20.0f * OrbitSpeed;

            if (yDistance != 0f || xDistance != 0f)
                OnOrbitChanged?.Invoke();

            xDegrees += xDistance;
            yDegrees += yDistance;

            // Clamp the vertical axis for the orbit.
            yDegrees = ClampAngle(yDegrees, YMinLimit, YMaxLimit);

            // Set desired camera rotation.
            Quaternion desiredRotation = Quaternion.Euler(yDegrees, xDegrees, 0);

            // Linearly interpolate camera rotation to desired rotation. I have no idea why, but just setting the
            // rotation to desired does not work (its not normalization). Lerp seems to produce a quaternion with the
            // signs of the coordinates flipped.
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, 1.0f);

            if (!(Input.GetMouseButton(0) ||
                  Input.GetKey(KeyCode.LeftArrow) || 
                  Input.GetKey(KeyCode.RightArrow) ||
                  Input.GetKey(KeyCode.UpArrow) || 
                  Input.GetKey(KeyCode.DownArrow)))
            {
                orbiting = false;
                DisableBlocker();
            }

        }

        private void OnlyOneInputPicker()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftCommand) || IsTouchOverHandle())
            {
                inputBlocker.gameObject.SetActive(true);
                mode = true;
            }

            // If the user is zooming, orbiting or panning we calculate their position

            if (zoom)
            {
                distance += Input.GetAxis("Mouse Y") * ZoomSpeed * 0.125f * Mathf.Abs(distance);

                if (!Input.GetMouseButton(1))
                {
                    zoom = false;
                    DisableBlocker();
                }

                return;
            }

            if (orbiting)
            {
                OrbitingUpdate();
                return;
            }

            if (panning)
            {
                PanningUpdate();
                return;
            }

            if (mode && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand) || IsTouchOverHandle()))
            {
                inputBlocker.gameObject.SetActive(false);
                GlobalManager.Get().ResetCursor();
                InputBlockerHovered = false;
                mode = false;
            }

            if (EventSystem.current.IsPointerOverGameObject() && !InputBlockerHovered)
                return;

            // If they are not already in zoom, orbit or pan, we check if they want to start zoom, orbit or pan

            // If scrollWheel is used change zoom. This one is not exclusive.
            distance -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed * Mathf.Abs(distance);

            // Invoke zoom changed
            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                OnZoomChanged.Invoke();

            // If the left control is pressed and.... 
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand))
            {

                // The right mouse button we activate zoom.
                if (Input.GetMouseButtonDown(1))
                {
                    zoom = true;
                    GlobalManager.Get().SetCursor(CursorType.ZoomCursor);
                    return;
                }

                // The left mouse button or Arrow keys we activate orbiting.
                if (Input.GetMouseButtonDown(0) || 
                    Input.GetKeyDown(KeyCode.LeftArrow) ||
                    Input.GetKeyDown(KeyCode.RightArrow) ||
                    Input.GetKeyDown(KeyCode.UpArrow) || 
                    Input.GetKeyDown(KeyCode.DownArrow))
                {
                    orbiting = true;
                    GlobalManager.Get().SetCursor(CursorType.RotateCursor);
                    return;
                }
            }

            // If the middle mouse is pressed, the arrow keys, we activate panning.
            if (Input.GetMouseButtonDown(2) || 
                Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.UpArrow) || 
                Input.GetKeyDown(KeyCode.DownArrow))
                {
                    inputBlocker.gameObject.SetActive(true);
                    panning = true;
                    GlobalManager.Get().SetCursor(CursorType.GrabCursor);
                    return;
                }

            // We now get to touchscreen controls
            // If we have exactly 2 touches we zoom
            if (Input.touchCount == 2){
                PinchToZoom();
                return;
            }

            // If we have exactly 1 touch
            if (Input.touchCount == 1){
                Touch t = Input.GetTouch(0); // collect information about the touch
                bool doubleTapDetected = IsDoubleTap(t);
                // clicks on game objects are excluded
                if(EventSystem.current.IsPointerOverGameObject(t.fingerId) || IsTouchOverHandle())
                    return;
                // if it is a double tap we orbit
                Debug.Log("doubleTapDetected " + doubleTapDetected + " t.phase: "+ t.phase);
                if (doubleTapDetected)
                {
                    orbiting = true;
                    activeFingerId = t.fingerId;
                    inputBlocker.gameObject.SetActive(true);
                    GlobalManager.Get().SetCursor(CursorType.RotateCursor);
                    return;
                }
                // if it is not a double tap we pan
                if(Input.touchCount == 1 && t.phase == TouchPhase.Began){
                    inputBlocker.gameObject.SetActive(true);
                    panning = true;
                    activeFingerId = t.fingerId;
                    GlobalManager.Get().SetCursor(CursorType.GrabCursor);
                    //Debug.Log("Starting panning through touch");
                    return;
                }
            }
        }

        private bool flytocam = false;

        public void FlyToRTCamera()
        {
            flytocam = true;
        }

        private void FlyToRTCameraStep()
        {
            Transform RTCamTransform = RTSceneManager.Get().Scene.Camera.transform;

            transform.position = Vector3.Lerp(transform.position, RTCamTransform.position, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, RTCamTransform.rotation, 0.1f);
        }

        private void FixedUpdate()
        {
            if (flytocam)
            {
                Transform RTCamTransform = RTSceneManager.Get().Scene.Camera.transform;
                if (transform.position == RTCamTransform.position && transform.eulerAngles == RTCamTransform.eulerAngles)
                {
                    flytocam = false;
                    distance = Vector3.Distance(Target.position, RTCamTransform.position);
                    xDegrees = transform.rotation.eulerAngles.y;
                    yDegrees = transform.rotation.eulerAngles.x;
                    Target.position = transform.position + (transform.rotation * Vector3.forward * distance);
                }
                else
                    FlyToRTCameraStep();
            }
        }

        private void PinchToZoom()
        {
            // grab the two touches
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // their positions last frame
            Vector2 prevPos0 = t0.position - t0.deltaPosition;
            Vector2 prevPos1 = t1.position - t1.deltaPosition;

            // compute previous vs. current distance between touches
            float prevDeltaMag = (prevPos0 - prevPos1).magnitude;
            float currDeltaMag = (t0.position  - t1.position ).magnitude;

            // difference = how much the pinch has grown (positive = pinch close = zoom out)
            float deltaMagDiff = prevDeltaMag - currDeltaMag;

            // apply to your camera distance (tune the factor if needed)
            distance += deltaMagDiff * ZoomSpeed * 0.01f;
            OnZoomChanged?.Invoke();

            // clamp immediately so you don’t over-zoom
            distance = Mathf.Clamp(distance, MinDistance, MaxDistance);

            // early out so you don’t also pan/orbit
            transform.position = Target.position - (transform.rotation * Vector3.forward * distance);
            return;
        }

        private bool TouchPanningUpdate()
        {
            float xDistance = 0.0f;
            float yDistance = 0.0f;

            if (Input.touchCount == 1) {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved) {
                    // deltaPosition is in screen pixels → scale to world-space pan
                    xDistance = -t.deltaPosition.x * 0.0001f;
                    yDistance = -t.deltaPosition.y * 0.0001f;
                }
            } else {
                return false;
            }

            if (yDistance != 0f || xDistance != 0f)
                onPanChanged?.Invoke();

            // we pan by way of transforming the target in screen space.
            // Grab the rotation of the camera so we can move in a pseudo local XY space.
            Target.rotation = transform.rotation;
            Target.Translate(Vector3.right * (xDistance * PanSpeed * distance));
            Target.Translate(Vector3.up * (yDistance * PanSpeed * distance));
            return true;
        }

        private bool TouchOrbitUpdate()
        {
            // find the touch by ID
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId == activeFingerId)
                {
                    if (t.phase == TouchPhase.Moved)
                    {
                        float xDelta =  t.deltaPosition.x * OrbitSpeed * Time.deltaTime;
                        float yDelta = -t.deltaPosition.y * OrbitSpeed * Time.deltaTime;
                        
                        xDegrees += xDelta;
                        yDegrees += yDelta;
                        yDegrees = ClampAngle(yDegrees, YMinLimit, YMaxLimit);

                        Quaternion desired = Quaternion.Euler(yDegrees, xDegrees, 0);
                        transform.rotation = Quaternion.Lerp(transform.rotation, desired, 1.0f);

                        OnOrbitChanged?.Invoke();
                    }
                    // when the user lifts that finger, end orbit
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        void Update()
        {
            if (flytocam)
                return;

            // If we are over the ui we don't allow the user to start any of these actions.
            OnlyOneInputPicker();

            // TODO Allow for free zoom movement not clamped and not as a function of distance just a fixed step amount.
            // TODO Add function to focus on objects and have zoom like it is now.
            // check if distance is still within it's bounds
            distance = Mathf.Clamp(distance, MinDistance, MaxDistance);

            // Update the position based on our rotation and the distance to the target.
            transform.position = Target.position - (transform.rotation * Vector3.forward * distance);

        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
