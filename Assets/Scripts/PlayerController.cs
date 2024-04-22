using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour{
    [SerializeField] private float followSpeed = 5;
    
    [SerializeField] private LineRenderer brushPrefab;
    [SerializeField] private GameObject drawPanel;
    
    private GraphicRaycaster _uiRaycaster;
    private Canvas           _canvas;
    private EventSystem      _eventSystem;
    
    private RectTransform _drawPanelRectTransform;
    
    private PointerEventData _pointerEventData;
    
    private Vector2 _previousDrawPoint;
    private LineRenderer _currentBrush;

    private SplineFollower _follower;
    
    private Vector2 _drawPanelLastPosition;
    
    private void Awake(){
        _uiRaycaster = FindObjectOfType<GraphicRaycaster>();
        _canvas      = FindObjectOfType<Canvas>();
        _eventSystem = FindObjectOfType<EventSystem>();
        
        _drawPanelRectTransform = drawPanel.GetComponent<RectTransform>();
    }
    
    private void Start(){
        _follower = GetComponent<SplineFollower>();
    }
    
    private void Update(){
        Drawing();
    
        if (Input.GetKeyDown(KeyCode.Space)){
            _follower.followSpeed = followSpeed;
        }
        if (Input.GetKeyDown(KeyCode.C)){
            _follower.followSpeed = 0;
        }
    }
    
    private void LateUpdate(){
    /*
        if (!_currentBrush){
            return;
        }
        Vector2 drawPanelDelta = drawPanel.transform.position - _drawPanelLastPosition;
        for (int i = 0; i < _currentBrush.positionCount; i++){
            _currentBrush.SetPosition(i, _currentBrush.GetPosition(i) + drawPanelDelta);
        }
        _drawPanelLastPosition = drawPanel.transform.position;
        */
    }
    
    private void Drawing(){
        if (Input.GetKeyDown(KeyCode.Mouse0)){
            _currentBrush = Instantiate(brushPrefab, drawPanel.transform);
            _currentBrush.positionCount = 0;
            Vector2 touchPosition = DrawPanelWorldTouchPos(out bool success);
            if (success){
                AddDrawPoint(touchPosition);
                _previousDrawPoint = touchPosition;
            }
        }
        if (Input.GetKey(KeyCode.Mouse0)){
            Vector2 touchPosition = DrawPanelWorldTouchPos(out bool success);            
            if (success && (_previousDrawPoint == Vector2.zero || Vector2.Distance(touchPosition, _previousDrawPoint) >= 0.02f)){
                AddDrawPoint(touchPosition);
                _previousDrawPoint = touchPosition;
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0)){
            Destroy(_currentBrush.gameObject);
            _currentBrush = null;
            _previousDrawPoint = Vector2.zero;
        }
    }
    
    private void AddDrawPoint(Vector2 point){
        _currentBrush.positionCount++;
        _currentBrush.SetPosition(_currentBrush.positionCount-1, point);
    }
    
    private Vector2 DrawPanelWorldTouchPos(out bool success){
        _pointerEventData = new PointerEventData(_eventSystem);
        _pointerEventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        _uiRaycaster.Raycast(_pointerEventData, results);
        
        for (int i = 0; i < results.Count; i++){
            if (results[i].gameObject.GetComponent<DrawPanel>()){
                Debug.Log(results[i].screenPosition);
                float scaleFactor = _canvas.scaleFactor;
            
                success = true;
                Vector2 halfScreenWidth = new Vector2(Screen.width * 0.5f, 0);
                return (results[i].screenPosition - halfScreenWidth) / scaleFactor - new Vector2(0, _drawPanelRectTransform.sizeDelta.y * 0.5f);// - new Vector2(_drawPanelRectTransform.sizeDelta.x * 0.5f, _drawPanelRectTransform.sizeDelta.y * 0.5f);// - new Vector2(0, Screen.;// * 2;// + results[i].worldNormal * 0.05f;
            }
        }
        
        success = false;
        return Vector2.zero;
        //return Camera.main.ScreenToWorldPoint(Input.mousePosition);// + drawPanel.transform.position;// + drawPanel.transform.localPosition;
    }
}
