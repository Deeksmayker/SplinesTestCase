using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;
using static Utils;

public class PlayerController : MonoBehaviour{
    [SerializeField] private float followSpeed = 5;
    [SerializeField] private float dudesAreaWidth = 10;
    [SerializeField] private float dudesAreaHeight = 10;
    
    [SerializeField] private LineRenderer brushPrefab;
    [SerializeField] private GameObject drawPanel;
    
    private float _brushHeightPointLimit, _brushWidthPointLimit;
    
    private GraphicRaycaster _uiRaycaster;
    private Canvas           _canvas;
    private EventSystem      _eventSystem;
    
    private RectTransform _drawPanelRectTransform;
    
    private PointerEventData _pointerEventData;
    
    private Vector2 _previousDrawPoint;
    private LineRenderer _currentBrush;

    private SplineFollower _follower;
    
    private List<Dude> _dudes = new();
    
    private void Awake(){
        _uiRaycaster = FindObjectOfType<GraphicRaycaster>();
        _canvas      = FindObjectOfType<Canvas>();
        _eventSystem = FindObjectOfType<EventSystem>();
        
        _drawPanelRectTransform = drawPanel.GetComponent<RectTransform>();
    }
    
    private void Start(){
        _follower = GetComponent<SplineFollower>();
        
        _dudes = GetComponentsInChildren<Dude>().ToList();
        
        _brushHeightPointLimit = _drawPanelRectTransform.sizeDelta.y * 0.5f;
        _brushWidthPointLimit = _drawPanelRectTransform.sizeDelta.x * 0.5f;
    }
    
    private void Update(){
        Drawing();
    
        if (Input.GetKeyDown(KeyCode.Space)){
            _follower.followSpeed = followSpeed;
        }
        if (Input.GetKeyDown(KeyCode.C)){
            _follower.followSpeed = 0;
        }
        
        UpdateDudes();
    }
    
    private void UpdateDudes(){
        for (int i = 0; i < _dudes.Count; i++){
            Dude dude = _dudes[i];
            
            if (dude == null){
                _dudes.RemoveAt(i);
                continue;
            }
            
            if (dude.deadMan){
                dude.transform.localScale = Vector3.Lerp(dude.transform.localScale, Vector3.zero, Time.deltaTime);
                continue;
            }
            
            CalculateDudeCollisions(ref dude);
            
            dude.transform.localPosition = Vector3.Lerp(dude.transform.localPosition, dude.targetLocalPosition, Time.deltaTime * 10);
        }
    }
    
    private void CalculateDudeCollisions(ref Dude dude){
        if (CheckSphere(dude.transform.position + dude.transform.up * dude.capsuleCollider.height * 0.5f, dude.capsuleCollider.radius, Layers.Obstacle)){
            KillDude(ref dude);
        }
    }
    
    public void KillDude(ref Dude dude){
        if (dude.deadMan){
            return;
        }
        
        dude.deathParticles.Play();
        dude.deadMan = true;
        var rb = dude.gameObject.AddComponent<Rigidbody>();
        rb.AddForce(-transform.forward * 3000);
        Destroy(dude.gameObject, 3);
    }
    
    private void RearrangeDudes(LineRenderer brush){
        if (_dudes.Count <= 0){
            return;
        }
    
        float pointsStep = (float)brush.positionCount / (float)_dudes.Count;
        for (int i = 0; i < _dudes.Count; i++){
            if (_dudes[i].deadMan){
                continue;
            }
        
            float targetLocalX =  Mathf.Lerp(-dudesAreaWidth * 0.5f,
                                             dudesAreaWidth * 0.5f,
                                             Mathf.InverseLerp(-_brushWidthPointLimit, _brushWidthPointLimit, brush.GetPosition((int)(i * pointsStep)).x));
            float targetLocalZ =  Mathf.Lerp(-dudesAreaHeight * 0.5f,
                                             dudesAreaHeight * 0.5f,
                                             Mathf.InverseLerp(-_brushHeightPointLimit, _brushHeightPointLimit, brush.GetPosition((int)(i * pointsStep)).y));
                                             
             _dudes[i].targetLocalPosition = new Vector3(targetLocalX, 0, targetLocalZ);
        }
    }
    
    private void Drawing(){
        if (Input.GetKeyDown(KeyCode.Mouse0)){
            Vector2 touchPosition = DrawPanelWorldTouchPos(out bool success);
            if (success){
                _currentBrush = Instantiate(brushPrefab, drawPanel.transform);
                _currentBrush.positionCount = 1;
                _currentBrush.SetPosition(0, touchPosition);
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
        if (Input.GetKeyUp(KeyCode.Mouse0) && _currentBrush){
            RearrangeDudes(_currentBrush);
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
                float scaleFactor = _canvas.scaleFactor;
            
                success = true;
                Vector2 halfScreenWidth = new Vector2(Screen.width * 0.5f, 0);
                return (results[i].screenPosition - halfScreenWidth) / scaleFactor - new Vector2(0, _drawPanelRectTransform.sizeDelta.y * 0.5f);
            }
        }
        
        success = false;
        return Vector2.zero;
    }
    
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
		Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(Vector3.zero, new Vector3(dudesAreaWidth, 2, dudesAreaHeight));
    }
}
