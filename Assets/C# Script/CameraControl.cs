using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    /* L'utilisateur peut bouger la caméra de 3 façons:
     *   1) Approcher le curseur du bord de l'écran.
     *   2) Maintenir le bouton droit de la souris enfoncé (ou le bouton gauche -> configurable) et bouger dans la direction souhaité (ou opposé -> configurable).
     *   3) Appuyer sur les touches fléchés.
     * On peut choisir plusieurs de ces modes.
     * 
     * J'ai essayé pour le fun si ça serait envisageable de rotate la caméra sur l'aze des z. Très mauvaise idée... puisque ça change les directions de la caméra (ex. gauche devient haut).
     * J'ai donc revert cette feature.
     * 
     * L'utilisateur peut zoom in/out à l'aide de la mouse wheel.
     * 
     * */

    public enum enmModeMoveCamera
    {
        CursorOnEdge = 0,           // The camera move when the cursor is close to the edge of the screen.
        ClickAndDrag = 1,           // The camera move in the direction (or backward) the cursor move when the user pressed on the right button (can choose left button).
        MoveWithArrowKeys = 2       // The camera move when the user use the arrow keys.
    }

    #region Public variables

    // Variables for the configuration of the mode CursorOnEdge.
    public float NbPixelScreenEdgeScroll = 30.0f;                           // Distance in pixels from the edge of the screen where we begin to move the camera.        TODO - Perfect in all resolutions?
    public float CameraCursorEdgeSpeed = 20.0f;                             // Max speed of the camera in this mode. Closer the cursor will be to the edge of the screen, faster the camera will move.
    public bool IsLimitCameraExcludeUI = false;                             // Indicate if we limit the detection of the cursor on the edge of the screen to exclude the UI.
    // Limits of the detection of the cursor on the edge of the screen. Use only if IsLimitCameraExcludeUI is true.
    public float CameraLimitsLeft = 0.0f;
    public float CameraLimitsTop = 0.0f;
    public float CameraLimitsRight= 0.0f;
    public float CameraLimitsBottom = 0.0f;
    public bool IsCameraMoveCursorOutScreen;                                // Indicate if we allow the camera to move if the cursor is out of the screen.


    // Variables for the configuration of the mode ClickAndDrag.
    public float CameraDragSpeed = 4.0f;                                    // Speed of the camera when the use go in one direction when olding the right/left button of the mouse.
    public bool IsDragMovementInverted = false;                             // Indicate if we want the movement of the camera to be inverted (if the cursor goes to the left, the camera goes to the right).
    public bool IsUseRightButtonToDragCamera = true;                        // The user use the right button to move the camera. If false, we'll use the left button.


    // Variables for the configuration of the mode MoveWithArrowKeys.
    public float CameraMoveSpeedArrows = 20.0f;


    // Variables for the configuration of the Zoom.
    public float CameraZoomSpeed = 8.0f;
    public float CameraZoomSmoothSpeed = 10.0f;
    public float CameraZoomMinOrtho = 3.5f;
    public float CameraZoomMaxOrtho = 11.5f;


    // Other public variables.
    public List<enmModeMoveCamera> LstModesCamera = new List<enmModeMoveCamera>() { enmModeMoveCamera.CursorOnEdge, enmModeMoveCamera.ClickAndDrag, enmModeMoveCamera.MoveWithArrowKeys };
    public CameraBounds CameraBoundsObj;                                    // Limits the movement of the camera to stay in game.
    public bool LockCursor = true;                                          // Confined the cursor to the view.
    public bool IsAfficheDebug = true;                                      // Display the debug infos?

    #endregion

    #region Privates variables

    // Zoom
    private float _cameraTargetOrtho;
    private float _cameraOriginalOrtho;

    // DragAndMove mode.
    private Vector3 _mouseOrigin = Vector3.zero;
    private bool _isDraggingCamera = false;

    // Screen
    private int _screenWidth;
    private int _screenHeight;
    private Camera _camera;

    // Debug
    private Vector3 _mousePosition = Vector3.zero;
    private GUIStyle _styleTexteDebug;
    private Texture2D _textureDebug;

    #endregion

    #region Unity - Events

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        _camera = gameObject.GetComponent<Camera>();

        _cameraTargetOrtho = _camera.orthographicSize;
        _cameraOriginalOrtho = _camera.orthographicSize;

        if (LockCursor)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Camera movements should be in the LateUpdate event.
    /// </summary>
    private void LateUpdate()
    {
        // Debug...
        _mousePosition = Input.mousePosition;

        if (_camera != null)
        {
            if (LstModesCamera.Contains(enmModeMoveCamera.CursorOnEdge))
                moveCamera_MouseOnScreenEdge();
            if (LstModesCamera.Contains(enmModeMoveCamera.ClickAndDrag))
                moveCamera_MouseClickAndDrag();
            if (LstModesCamera.Contains(enmModeMoveCamera.MoveWithArrowKeys))
                moveCamera_ArrowKeys();

            checkForZoom();
        }
    }

    /// <summary>
    /// For debug purpose.
    /// </summary>
    private void OnGUI()
    {
        if (IsAfficheDebug)
        {
            if (_styleTexteDebug == null)
            {
                _styleTexteDebug = new GUIStyle();
                _styleTexteDebug.fontSize = 20;
                _styleTexteDebug.normal.textColor = new Color32(0, 0, 128, 255);
            }

            if (_textureDebug == null)
            {
                Color32 color = new Color32(255, 255, 255, 150);
                _textureDebug = new Texture2D(1, 1);
                _textureDebug.SetPixel(0, 0, color);
                _textureDebug.Apply();
            }

            GUI.skin.box.normal.background = _textureDebug;
            GUI.Box(new Rect(5, 65, 325, 125), GUIContent.none);

            GUI.Label(new Rect(10, 70, 100, 30), "Pos Curseur: " + _mousePosition.ToString(), _styleTexteDebug);
            GUI.Label(new Rect(10, 100, 100, 30), "Pos Camera: " + gameObject.transform.position.ToString(), _styleTexteDebug);
            GUI.Label(new Rect(10, 130, 100, 30), "Width: " + _screenWidth.ToString(), _styleTexteDebug);
            GUI.Label(new Rect(10, 160, 100, 30), "Height: " + _screenHeight.ToString(), _styleTexteDebug);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Reset the zoom of the camera.
    /// </summary>
    public void ResetCameraZoom()
    {
        if (_camera != null)
        {
            _camera.orthographicSize = _cameraOriginalOrtho;
        }
    }

    #endregion

    #region Privates Methods

    /// <summary>
    /// Move the camera when the cursor go near the edge of the screen.
    /// 
    /// Notes:
    ///   -> We restrict the camera movement in CameraBounds. The camera stay in the game area.
    ///   -> We must validate if we can move (restrict in CameraBounds) before moving to avoid flicker.
    /// </summary>
    private void moveCamera_MouseOnScreenEdge()
    {
        float speed = CameraCursorEdgeSpeed;
        float min = 0.0f;
        float max = 0.0f;

        // Move to the right
        min = _screenWidth - NbPixelScreenEdgeScroll - (IsLimitCameraExcludeUI ? CameraLimitsRight : 0.0f);
        max = _screenWidth - (IsLimitCameraExcludeUI ? CameraLimitsRight : 0.0f) + 5.0f;
        if (Input.mousePosition.x > min && gameObject.transform.position.x < CameraBoundsObj.BoundRight)
        {
            speed = calculerCameraSpeed(min, max, CameraCursorEdgeSpeed, Input.mousePosition.x, false);
            if (speed > 0.0f)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x + speed * Time.deltaTime, gameObject.transform.position.y, gameObject.transform.position.z);
        }

        // Move to the left
        min = 0.0f + (IsLimitCameraExcludeUI ? CameraLimitsLeft : 0.0f) - 5.0f;
        max = NbPixelScreenEdgeScroll + (IsLimitCameraExcludeUI ? CameraLimitsLeft : 0.0f);
        if (Input.mousePosition.x < max && gameObject.transform.position.x > CameraBoundsObj.BoundLeft)
        {
            speed = calculerCameraSpeed(min, max, CameraCursorEdgeSpeed, Input.mousePosition.x, true);
            if (speed > 0.0f)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x - speed * Time.deltaTime, gameObject.transform.position.y, gameObject.transform.position.z);
        }

        // Move up
        min = _screenHeight - NbPixelScreenEdgeScroll - (IsLimitCameraExcludeUI ? CameraLimitsTop : 0.0f);
        max = _screenHeight - (IsLimitCameraExcludeUI ? CameraLimitsTop : 0.0f) + 5.0f;
        if (Input.mousePosition.y > min && gameObject.transform.position.y < CameraBoundsObj.BoundTop)
        {
            speed = calculerCameraSpeed(min, max, CameraCursorEdgeSpeed, Input.mousePosition.y, false);
            if (speed > 0.0f)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + speed * Time.deltaTime, gameObject.transform.position.z);
        }

        // Move down
        min = 0.0f + (IsLimitCameraExcludeUI ? CameraLimitsBottom : 0.0f) - 5.0f;
        max = NbPixelScreenEdgeScroll + (IsLimitCameraExcludeUI ? CameraLimitsBottom : 0.0f);
        if (Input.mousePosition.y < max && gameObject.transform.position.y > CameraBoundsObj.BoundBottom)
        {
            speed = calculerCameraSpeed(min, max, CameraCursorEdgeSpeed, Input.mousePosition.y, true);
            if (speed > 0.0f)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - speed * Time.deltaTime, gameObject.transform.position.z);
        }
    }

    /// <summary>
    /// Move the camera when the user old the right button down and move.
    ///   -> Config to use the left or right button.
    ///   -> Config to move in the direction or inversed.
    /// </summary>
    private void moveCamera_MouseClickAndDrag()
    {
        bool isMouseButtonDown = false;

        if (Input.GetMouseButtonDown(1) && IsUseRightButtonToDragCamera)
            isMouseButtonDown = true;
        else if (Input.GetMouseButtonDown(0) && !IsUseRightButtonToDragCamera)
            isMouseButtonDown = true;

        // Begin the movement
        if (isMouseButtonDown)
        {
            _mouseOrigin = Input.mousePosition;
            _isDraggingCamera = true;
        }

        bool isMouseButtonRelease = false;
        if (!Input.GetMouseButton(1) && IsUseRightButtonToDragCamera)
            isMouseButtonRelease = true;
        else if (!Input.GetMouseButton(0) && !IsUseRightButtonToDragCamera)
            isMouseButtonRelease = true;

        // Stop the movement
        if (isMouseButtonRelease)
        {
            _isDraggingCamera = false;
        }

        // Move the camera
        if (_isDraggingCamera)
        {
            Vector3 pos = _camera.ScreenToViewportPoint(Input.mousePosition - _mouseOrigin);

            Vector3 move = new Vector3(pos.x * CameraDragSpeed, pos.y * CameraDragSpeed, 0) * (IsDragMovementInverted ? -1 : 1);

            _camera.transform.Translate(move, Space.Self);
        }
    }

    /// <summary>
    /// Move the camera in the direction of the arrow when the user pressed on an arrow key.
    /// </summary>
    private void moveCamera_ArrowKeys()
    {
        if (Input.GetKey(KeyCode.RightArrow) && gameObject.transform.position.x < CameraBoundsObj.BoundRight)
        {
            transform.Translate(new Vector3(CameraMoveSpeedArrows * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow) && gameObject.transform.position.x > CameraBoundsObj.BoundLeft)
        {
            transform.Translate(new Vector3(-CameraMoveSpeedArrows * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.UpArrow) && gameObject.transform.position.y < CameraBoundsObj.BoundTop)
        {
            transform.Translate(new Vector3(0, CameraMoveSpeedArrows * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.DownArrow) && gameObject.transform.position.y > CameraBoundsObj.BoundBottom)
        {
            transform.Translate(new Vector3(0, -CameraMoveSpeedArrows * Time.deltaTime, 0));
        }
    }

    /// <summary>
    /// Allow the user to Zoom In/Out with the mousewheel.
    /// </summary>
    private void checkForZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
            _cameraTargetOrtho -= scroll * CameraZoomSpeed;
            _cameraTargetOrtho = Mathf.Clamp(_cameraTargetOrtho, CameraZoomMinOrtho, CameraZoomMaxOrtho);
        }

        _camera.orthographicSize = Mathf.MoveTowards(_camera.orthographicSize, _cameraTargetOrtho, CameraZoomSmoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Calculate the speed of camera. Closer to the edge faster the camera will be.
    /// </summary>
    private float calculerCameraSpeed(float distanceMin, float distanceMax, float speedMax, float mousePosition, bool isNegatif)
    {
        if (!IsCameraMoveCursorOutScreen)
        {
            // We move only if we are in the scrollable zone.
            if (mousePosition <= distanceMin || mousePosition >= distanceMax)
                return 0.0f;
        }
        else
        {
            // We can move the camera when we go further (out of screen). Use when not fullscreen...
            if (isNegatif)
            {
                // Left or bottom
                if (mousePosition <= distanceMin)
                    return speedMax;
                else if (mousePosition >= distanceMax)
                    return 0.0f;
            }
            else
            {
                // Right or top
                if (mousePosition <= distanceMin)
                    return 0.0f;
                else if (mousePosition >= distanceMax)
                    return speedMax;
            }
        }
        

        float distanceFromMin = mousePosition - distanceMin;
        float totalDistance = distanceMax - distanceMin;
        float pourcent = distanceFromMin / totalDistance;

        // If we go in the minus, we must reverse the pourcent.
        if (isNegatif)
            pourcent = 1 - pourcent;

        // Tweak
        if (pourcent < 0.50f)
        {
            // We are not so close to the edge. The camera move slowly.
            speedMax = speedMax * 0.5f;
            return speedMax * pourcent;
        }
        else if (pourcent > 0.85f)
        {
            // We are close to the edge. The camera move faster.
            return speedMax;
        }
        else
        {
            // Little progression for the speed.
            return speedMax * pourcent;
        }        
    }

    #endregion
}
