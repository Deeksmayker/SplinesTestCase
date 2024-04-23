using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject drawTextPanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Text gemCountTextMesh;
    [SerializeField] private Material dudeMaterial;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem newDudeParticles;
    [SerializeField] private ParticleSystem bloodParticles;
    [SerializeField] private ParticleSystem[] winParticles;
    
    private float _brushHeightPointLimit, _brushWidthPointLimit;
    
    private int _gemCount;
    
    private int _aliveCount;
    
    private bool _gameStarted;
    private bool _win;
    private bool _dead;
    private float _winTime;
    
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
        gemCountTextMesh.text = "0";
        _uiRaycaster = FindObjectOfType<GraphicRaycaster>();
        _canvas      = FindObjectOfType<Canvas>();
        _eventSystem = FindObjectOfType<EventSystem>();
        
        _drawPanelRectTransform = drawPanel.GetComponent<RectTransform>();
    }
    
    private void Start(){
        _follower = GetComponent<SplineFollower>();
        
        _dudes = GetComponentsInChildren<Dude>().ToList();
        _aliveCount = _dudes.Count;
        
        _brushHeightPointLimit = _drawPanelRectTransform.sizeDelta.y * 0.5f;
        _brushWidthPointLimit = _drawPanelRectTransform.sizeDelta.x * 0.5f;
        
        _follower.onEndReached += Win;   
    }
    
    private void Update(){
        Drawing();
        
        if (!_gameStarted){
            _follower.followSpeed = 0;
            float cosValue = Cos(Time.time * 3) * 1f + 0.5f;
            float sinValue = Sin(Time.time * 2) * 40;
            drawTextPanel.transform.localScale = new Vector3(cosValue * Sign(cosValue), cosValue * Sign(cosValue) * 0.5f, 1);
            drawTextPanel.transform.localEulerAngles = new Vector3(0, 0, sinValue);
        } else if (!_dead){
            _follower.followSpeed = followSpeed;
        }
    
        if (_gameStarted && !_dead){
            UpdateDudes();
        }
    }
    
    private void UpdateDudes(){
        if (_dead){
            return;
        }
    
        if (_win){
            _winTime += Time.deltaTime;
        }
    
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
            if (!_win){
                dude.animator.SetBool("Running", true);
            }
            CalculateDudeCollisions(ref dude);
            
            dude.transform.localPosition = Vector3.Lerp(dude.transform.localPosition, dude.targetLocalPosition, Time.deltaTime * 3);
            float rotationMultiplier = 1;
            if (_win){
                rotationMultiplier = -1;
            }
            dude.transform.forward = transform.right * rotationMultiplier;
            
            if (_win){
                if (_winTime >= 6){
                    dude.transform.Rotate(new Vector3(0, 40, 0) * Time.deltaTime);
                    dude.transform.localScale = Vector3.Lerp(dude.transform.localScale, Vector3.zero, Time.deltaTime * 4);
                }
            }
        }
    }
    
    private void Win(double aboba){
        if (_win){
            return;
        }
    
        _win = true;
        
        float spacing = 4;
        int row = 0;
        
        for (int i = 0; i < winParticles.Length; i++){
            winParticles[i].gameObject.SetActive(true);
            winParticles[i].Play();
        }
        
        for (int i = 0; i < _dudes.Count; i++){
            //_dudes[i].transform.forward = -transform.right;
            _dudes[i].animator.SetBool("Win", true);
            float targetLocalZ = dudesAreaHeight - row * spacing;
            float targetLocalX = -dudesAreaWidth * 0.5f + i * spacing - row * dudesAreaWidth;
            _dudes[i].targetLocalPosition = new Vector3(targetLocalX, 0, targetLocalZ);
            
            if (i * spacing - row * dudesAreaWidth >= dudesAreaWidth - spacing){
                row++;
            }
        }
    }
    
    private void CalculateDudeCollisions(ref Dude dude){
        if (CheckSphere(dude.transform.position + dude.transform.up * dude.capsuleCollider.height * 0.5f, dude.capsuleCollider.radius, Layers.Obstacle)){
            KillDude(ref dude);
        }
        
        //@OPTIMIZATION required
        Collider[] inactiveDudes = OverlapSphere(dude.transform.position + dude.transform.up * dude.capsuleCollider.height * 0.5f, dude.capsuleCollider.radius, Layers.InactiveDude);
        for (int i = 0; i < inactiveDudes.Length; i++){
            if (inactiveDudes[i].TryGetComponent<Dude>(out var inactiveDude)){
                inactiveDude.gameObject.layer = LayerMask.NameToLayer("Dude");   
                inactiveDude.transform.SetParent(transform, true);
                //inactiveDude.transform.localPosition = dude.transform.localPosition;
                inactiveDude.targetLocalPosition = dude.targetLocalPosition + dude.transform.forward * 2;
                //inactiveDude.SetMaterial(dudeMaterial);
                
                var particles = Instantiate(newDudeParticles, inactiveDude.transform);
                particles.transform.localPosition = Vector3.up * 5;
                _dudes.Add(inactiveDude);
                _aliveCount++;
            }
        }
        
    }
    
    public void KillDude(ref Dude dude){
        if (dude.deadMan){
            return;
        }
        
        dude.animator.SetBool("Death", true);
        Instantiate(bloodParticles, dude.transform.position, Quaternion.identity);
        dude.deathParticles.Play();
        dude.deadMan = true;
        var rb = dude.gameObject.AddComponent<Rigidbody>();
        rb.AddForce(-transform.forward * 3000);
        Destroy(dude.gameObject, 3);
        
        _aliveCount--;
        if (_aliveCount <= 0){
            deathPanel.SetActive(true);
            _dead = true;
            _follower.followSpeed = 0;
        }
    }
    
    private void RearrangeDudes(LineRenderer brush){
        if (_dudes.Count <= 0 || _win){
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
                SpawnBrush(touchPosition);
                AddDrawPoint(touchPosition);
                _previousDrawPoint = touchPosition;
            }
        }
        if (Input.GetKey(KeyCode.Mouse0)){
            Vector2 touchPosition = DrawPanelWorldTouchPos(out bool success);            
            if (success && (_previousDrawPoint == Vector2.zero || Vector2.Distance(touchPosition, _previousDrawPoint) >= 0.02f)){
                if (!_currentBrush){
                    SpawnBrush(touchPosition);              
                }
                AddDrawPoint(touchPosition);
                _previousDrawPoint = touchPosition;
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0) && _currentBrush){
            if (!_gameStarted){
                _gameStarted = true;
                drawTextPanel.SetActive(false);            
            }
            RearrangeDudes(_currentBrush);
            Destroy(_currentBrush.gameObject);
            _currentBrush = null;
            _previousDrawPoint = Vector2.zero;
        }
    }
    
    private void SpawnBrush(Vector2 position){
        _currentBrush = Instantiate(brushPrefab, drawPanel.transform);
        _currentBrush.positionCount = 1;
        _currentBrush.SetPosition(0, position);
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
    
    public void AddMoney(){
        _gemCount++;
        gemCountTextMesh.text = _gemCount.ToString();
    }
    
    public void RestartLevel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
		Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(Vector3.zero, new Vector3(dudesAreaWidth, 2, dudesAreaHeight));
    }
}
