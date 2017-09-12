using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

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
        CursorOnEdge = 0,               // The camera move when the cursor is close to the edge of the screen.
        ClickAndDrag = 1,               // The camera move in the direction (or backward) the cursor move when the user pressed on the right button (can choose left button).
        MoveWithArrowKeys = 2,          // The camera move when the user use the arrow keys.
        FollowTarget = 3                // The camera move with the target (ex. player).
    }

    #region Public variables

    // Variables for the configuration of the mode CursorOnEdge.
    public float PourcentScreenEdgeWidth = 0.015625f;
    public float PourcentScreenEdgeHeight = 0.0f;
    public bool IsPourcentScreenEdgeRespectRatio = true;
    public float CameraCursorEdgeSpeed = 20.0f;                             // Max speed of the camera in this mode. Closer the cursor will be to the edge of the screen, faster the camera will move.
    public bool IsLimitEdgeDectectionToExcludeUI = false;                   // Indicate if we limit the detection of the cursor on the edge of the screen to exclude the UI.
    // Limits of the detection of the cursor on the edge of the screen. Use only if IsLimitCameraExcludeUI is true.
    public float EdgeDectectionLimitsLeft = 0.0f;
    public float EdgeDectectionLimitsTop = 0.0f;
    public float EdgeDetectionLimitsRight = 0.0f;
    public float EdgeDetectionLimitsBottom = 0.0f;
    public bool IsCameraMoveWhenCursorOutScreen;                            // Indicate if we allow the camera to move if the cursor is out of the screen.
    public bool IsSpeedProgressive = true;                                  // Indicate if the camera speed is slower when the cursor is not completly on the edge.


    // Variables for the configuration of the mode ClickAndDrag.
    public bool IsUseRightButtonToDragCamera = true;                        // The user use the right button to move the camera. If false, we'll use the left button.


    // Variables for the configuration of the mode MoveWithArrowKeys.
    public float CameraMoveSpeedArrows = 20.0f;


    // Variables for the configuration of the Zoom.
    public bool IsAllowZoom = true;
    public float CameraZoomSpeed = 8.0f;
    public float CameraZoomSmoothSpeed = 10.0f;
    public float CameraZoomMinOrtho = 3.5f;
    public float CameraZoomMaxOrtho = 11.5f;

    public bool IsCameraFollowDelai = true;
    public float CameraFollowSpeed = 0.2f;
    public GameObject CameraFollowTarget = null;

    // Other public variables.
    public List<enmModeMoveCamera> LstModesCamera = new List<enmModeMoveCamera>() { enmModeMoveCamera.CursorOnEdge, enmModeMoveCamera.ClickAndDrag, enmModeMoveCamera.MoveWithArrowKeys };
    public bool LockCursor = true;                                          // Confined the cursor to the view.
    public bool IsAfficheDebug = true;                                      // Display the debug infos?

    public bool IsLimitCamera = true;
    public BoxCollider2D BoundsLimitCamera;                                 // BoxCollider that indicate the limits of the camera. We correct the position of the camera in the LateUpdate method if the camera is outside of the BoxCollider.

    #endregion

    #region Privates variables

    // Zoom
    private float _cameraTargetOrtho;
    private float _cameraOriginalOrtho;

    // DragAndMove mode.
    private Vector3 Origin; // place where mouse is first pressed
    private Vector3 Difference; // change in position of mouse relative to origin

    // Screen
    private int _screenWidth;
    private int _screenHeight;
    private Camera _camera;

    // Variables to restraint the movement of the camera.
    private float _horizExtend = 0.0f;
    private float _vertExtend = 0.0f;
    private Bounds _areaBounds;
    private float _limitBoundLeft = 0.0f;
    private float _limitBoundTop = 0.0f;
    private float _limitBoundRight = 0.0f;
    private float _limitBoundBottom = 0.0f;

    // Debug
    private Vector3 _mousePosition = Vector3.zero;
    private GUIStyle _styleTexteDebug;
    private Texture2D _textureDebug;
    private double idUpdate = 0.0d;

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

        if (BoundsLimitCamera != null)
        {
            _areaBounds = BoundsLimitCamera.bounds;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.LeftAlt))
            Cursor.lockState = CursorLockMode.None;

        calculateBounds();
    }

    private void FixedUpdate()
    {
        if (LstModesCamera.Contains(enmModeMoveCamera.FollowTarget))
            moveCamera_FollowTarget();
    }

    /// <summary>
    /// Camera movements should be in the LateUpdate event.
    /// </summary>
    private void LateUpdate()
    {
        // Debug...
        idUpdate++;
        _mousePosition = Input.mousePosition;


        if (_camera != null)
        {
            if (LstModesCamera.Contains(enmModeMoveCamera.CursorOnEdge))
                moveCamera_MouseOnScreenEdge();
            if (LstModesCamera.Contains(enmModeMoveCamera.ClickAndDrag))
                moveCamera_MouseClickAndDrag();
            if (LstModesCamera.Contains(enmModeMoveCamera.MoveWithArrowKeys))
                moveCamera_ArrowKeys();

            if (IsAllowZoom)
                checkForZoom();

            // Restrint the mouvement of the camera to stay in the game!
            if (IsLimitCamera && BoundsLimitCamera != null)
            {
                _camera.transform.position = new Vector3(Mathf.Clamp(_camera.transform.position.x, _limitBoundLeft, _limitBoundRight),
                                                         Mathf.Clamp(_camera.transform.position.y, _limitBoundBottom, _limitBoundTop),
                                                         _camera.transform.position.z);
            }
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
            GUI.Box(new Rect(5, 65, 400, 300), GUIContent.none);

            GUI.Label(new Rect(10, 70, 100, 30), "Pos Curseur: " + _mousePosition.ToString(), _styleTexteDebug);
            GUI.Label(new Rect(10, 100, 100, 30), "Pos Camera: " + gameObject.transform.position.ToString(), _styleTexteDebug);
            GUI.Label(new Rect(10, 130, 100, 30), "Width: " + _screenWidth.ToString(), _styleTexteDebug);
            GUI.Label(new Rect(10, 160, 100, 30), "Height: " + _screenHeight.ToString(), _styleTexteDebug);
            //GUI.Label(new Rect(10, 190, 100, 30), "IsMouseDragStart: " + _isMouseDragStart.ToString(), _styleTexteDebug);
            //GUI.Label(new Rect(10, 220, 100, 30), "IsMouseDragEnd: " + _isMouseDragEnd.ToString(), _styleTexteDebug);
            //GUI.Label(new Rect(10, 250, 100, 30), "IsDragComplete: " + _isDragComplete.ToString(), _styleTexteDebug);
            //GUI.Label(new Rect(10, 280, 100, 30), "IsDraggingCamera: " + _isDraggingCamera.ToString(), _styleTexteDebug);
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
        float offset = 3.0f;

        float edgeDectectionWidth = PourcentScreenEdgeWidth * _screenWidth;
        float edgeDectectionHeight = PourcentScreenEdgeHeight * _screenHeight;
        if (IsPourcentScreenEdgeRespectRatio)
            edgeDectectionHeight = edgeDectectionWidth * _screenHeight / _screenWidth;

        // Move to the right
        min = _screenWidth - edgeDectectionWidth - (IsLimitEdgeDectectionToExcludeUI ? EdgeDetectionLimitsRight : 0.0f);
        max = _screenWidth - (IsLimitEdgeDectectionToExcludeUI ? EdgeDetectionLimitsRight : 0.0f) + offset;
        if (Input.mousePosition.x > min && gameObject.transform.position.x < _limitBoundRight)
        {
            speed = calculerCameraSpeed(min, max, CameraCursorEdgeSpeed, Input.mousePosition.x, false);
            if (speed > 0.0f)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x + speed * Time.deltaTime, gameObject.transform.position.y, gameObject.transform.position.z);
        }

        // Move to the left
        min = 0.0f + (IsLimitEdgeDectectionToExcludeUI ? EdgeDectectionLimitsLeft : 0.0f) - offset;
        max = edgeDectectionWidth + (IsLimitEdgeDectectionToExcludeUI ? EdgeDectectionLimitsLeft : 0.0f);
        if (Input.mousePosition.x < max && gameObject.transform.position.x > _limitBoundLeft)
        {
            speed = calculerCameraSpeed(min, max, CameraCursorEdgeSpeed, Input.mousePosition.x, true);
            if (speed > 0.0f)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x - speed * Time.deltaTime, gameObject.transform.position.y, gameObject.transform.position.z);
        }

        // Move up
        min = _screenHeight - edgeDectectionHeight - (IsLimitEdgeDectectionToExcludeUI ? EdgeDectectionLimitsTop : 0.0f);
        max = _screenHeight - (IsLimitEdgeDectectionToExcludeUI ? EdgeDectectionLimitsTop : 0.0f) + offset;
        if (Input.mousePosition.y > min && gameObject.transform.position.y < _limitBoundTop)
        {
            speed = calculerCameraSpeed(min, max, CameraCursorEdgeSpeed, Input.mousePosition.y, false);
            if (speed > 0.0f)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + speed * Time.deltaTime, gameObject.transform.position.z);
        }

        // Move down
        min = 0.0f + (IsLimitEdgeDectectionToExcludeUI ? EdgeDetectionLimitsBottom : 0.0f) - offset;
        max = edgeDectectionHeight + (IsLimitEdgeDectectionToExcludeUI ? EdgeDetectionLimitsBottom : 0.0f);
        if (Input.mousePosition.y < max && gameObject.transform.position.y > _limitBoundBottom)
        {
            speed = calculerCameraSpeed(min, max, CameraCursorEdgeSpeed, Input.mousePosition.y, true);
            if (speed > 0.0f)
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - speed * Time.deltaTime, gameObject.transform.position.z);
        }
    }

    /// <summary>
    /// Move the camera when the user old the right button down and move.
    ///   -> Config to use the left or right button.
    /// </summary>
    private void moveCamera_MouseClickAndDrag()
    {
        int button = (IsUseRightButtonToDragCamera ? 1 : 0);

        if (Input.GetMouseButtonDown(button))
        {
            Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(button))
        {
            Difference = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position;
            transform.position = Origin - Difference;
        }
    }

    /// <summary>
    /// Move the camera in the direction of the arrow when the user pressed on an arrow key.
    /// </summary>
    private void moveCamera_ArrowKeys()
    {
        if (Input.GetKey(KeyCode.RightArrow) && gameObject.transform.position.x < _limitBoundRight)
        {
            transform.Translate(new Vector3(CameraMoveSpeedArrows * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow) && gameObject.transform.position.x > _limitBoundLeft)
        {
            transform.Translate(new Vector3(-CameraMoveSpeedArrows * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.UpArrow) && gameObject.transform.position.y < _limitBoundTop)
        {
            transform.Translate(new Vector3(0, CameraMoveSpeedArrows * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.DownArrow) && gameObject.transform.position.y > _limitBoundBottom)
        {
            transform.Translate(new Vector3(0, -CameraMoveSpeedArrows * Time.deltaTime, 0));
        }
    }

    private void moveCamera_FollowTarget()
    {
        if (CameraFollowTarget != null)
        {
            float cameraSpeed = (IsCameraFollowDelai ? CameraFollowSpeed : 99.0f);  // If we don't want any delay, we take a high speed.

            Vector3 targetPosition = new Vector3(CameraFollowTarget.transform.position.x, CameraFollowTarget.transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
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
        if (!IsCameraMoveWhenCursorOutScreen)
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

        // Here, we are int the detection edge zone.
        // If we dont want a progressive speed (faster when closer to the edge), we return full speed.
        if (!IsSpeedProgressive)
            return speedMax;


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

    private void calculateBounds()
    {
        _vertExtend = _camera.orthographicSize;
        _horizExtend = _vertExtend * _screenWidth / _screenHeight;

        _limitBoundLeft = _areaBounds.min.x + _horizExtend;
        _limitBoundTop = _areaBounds.max.y - _vertExtend;
        _limitBoundRight = _areaBounds.max.x - _horizExtend;
        _limitBoundBottom = _areaBounds.min.y + _vertExtend;
    }

    #endregion
}
