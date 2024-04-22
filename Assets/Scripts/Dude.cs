using UnityEngine;

public class Dude : MonoBehaviour{
    public Animator animator;

    public ParticleSystem deathParticles;
    public CapsuleCollider capsuleCollider;
    public bool deadMan;
    
    public Vector3 targetLocalPosition;
    
    private SkinnedMeshRenderer _meshRenderer;
    private Material _material;
    
    private void Awake(){
        targetLocalPosition = transform.localPosition;
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();
        
        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    
    public void SetMaterial(Material material){
        _meshRenderer.materials[0] = material;
    }
}
