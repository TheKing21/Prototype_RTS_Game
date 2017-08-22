using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBounds : MonoBehaviour {

    public Camera CameraGameObject;

    public float BoundLeft
    {
        get
        {
            return _areaBound.min.x + _horizExtend;
        }
    }

    public float BoundRight
    {
        get
        {
            return _areaBound.max.x - _horizExtend;
        }
    }

    public float BoundBottom
    {
        get
        {
            return _areaBound.min.y + _vertExtend;
        }
    }

    public float BoundTop
    {
        get
        {
            return _areaBound.max.y - _vertExtend;
        }
    }

    private BoxCollider2D _boxColliderBounds;
    private float _vertExtend;
    private float _horizExtend;
    private Bounds _areaBound;

	private void Start ()
    {
        _boxColliderBounds = gameObject.GetComponent<BoxCollider2D>();
    }

	private void LateUpdate ()
    {
        _vertExtend = CameraGameObject.orthographicSize;
        _horizExtend = _vertExtend * Screen.width / Screen.height;
        _areaBound = _boxColliderBounds.bounds;

        Vector3 cameraPosition = CameraGameObject.transform.position;

        CameraGameObject.transform.position = new Vector3(Mathf.Clamp(cameraPosition.x, BoundLeft, BoundRight),
                                                 Mathf.Clamp(cameraPosition.y, BoundBottom, BoundTop),
                                                 cameraPosition.z);
    }
}
