using UnityEngine;

public class Mine : MonoBehaviour{
    [SerializeField] private ParticleSystem explosionParticles;
    
    private bool _dying;
    
    private CapsuleCollider _capsuleCollider;
    
    private void Awake(){
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }
    
    private void Update(){
        if (_dying){
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 2);
            return;
        }
    
        if (Physics.CheckCapsule(transform.position - transform.up * _capsuleCollider.height * 0.5f, transform.position + transform.up * _capsuleCollider.height * 0.5f, _capsuleCollider.radius, Layers.Dude)){
            Instantiate(explosionParticles, transform.position, Quaternion.identity);
            _dying = true;
            Destroy(gameObject, 1);
        }
    }
}
