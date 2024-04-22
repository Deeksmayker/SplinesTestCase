using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour{
    [SerializeField] private float followSpeed = 5;
    
    [SerializeField] private LineRenderer brushPrefab;
    [SerializeField] private GameObject drawPanel;
    
    GraphicRaycaster _uiRaycaster;
    EventSystem _eventSystem;
    
    PointerEventData _pointerEventData;
    
    private Vector3 _previousDrawPoint;
    private LineRenderer _currentBrush;

    private SplineFollower _follower;
    
    private void Awake(){
        _uiRaycaster = FindObjectOfType<GraphicRaycaster>();
        _eventSystem = FindObjectOfType<EventSystem>();
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
    
    private void Drawing(){
        if (Input.GetKeyDown(KeyCode.Mouse0)){
            _currentBrush = Instantiate(brushPrefab);
            _currentBrush.positionCount = 0;
            Vector3 touchPosition = DrawPanelWorldTouchPos(out bool success);
            if (success){
                AddDrawPoint(touchPosition);
                _previousDrawPoint = touchPosition;
            }
        }
        if (Input.GetKey(KeyCode.Mouse0)){
            Vector3 touchPosition = DrawPanelWorldTouchPos(out bool success);            
            if (success && (_previousDrawPoint == Vector3.zero || Vector3.Distance(touchPosition, _previousDrawPoint) >= 0.02f)){
                AddDrawPoint(touchPosition);
                _previousDrawPoint = touchPosition;
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0)){
            Destroy(_currentBrush.gameObject);
            _currentBrush = null;
            _previousDrawPoint = Vector3.zero;
        }
    }
    
    private void AddDrawPoint(Vector3 point){
        _currentBrush.positionCount++;
        _currentBrush.SetPosition(_currentBrush.positionCount-1, point);
    }
    
    private Vector3 DrawPanelWorldTouchPos(out bool success){
        _pointerEventData = new PointerEventData(_eventSystem);
        _pointerEventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        _uiRaycaster.Raycast(_pointerEventData, results);
        
        for (int i = 0; i < results.Count; i++){
            if (results[i].gameObject.GetComponent<DrawPanel>()){
                success = true;
                return results[i].worldPosition + results[i].worldNormal * 0.01f;
            }
        }
        
        success = false;
        return Vector3.zero;
        //return Camera.main.ScreenToWorldPoint(Input.mousePosition);// + drawPanel.transform.position;// + drawPanel.transform.localPosition;
    }
}
