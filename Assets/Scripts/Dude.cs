using UnityEngine;

public class Dude : MonoBehaviour{
    public ParticleSystem deathParticles;
    public CapsuleCollider capsuleCollider;
    public bool deadMan;
    
    public Vector3 targetLocalPosition;
    
    private SkinnedMeshRenderer _meshRenderer;
    private Material _material;
    
    private void Awake(){
        targetLocalPosition = transform.localPosition;
        capsuleCollider = GetComponent<CapsuleCollider>();
        
        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    
    public void SetMaterial(Material material){
        _meshRenderer.materials[0] = material;
    }
}
