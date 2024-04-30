using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JumpConnector : MonoBehaviour
{
    [SerializeField] private HingeJoint2D _closestHinge, _addedHinge;
    [SerializeField] [Range(0, 50)] private float _connectionRadius, _swingForce;
    [SerializeField] private LayerMask _layer;

    private PlayerMovement _playerMovement;
    private Rigidbody2D _playerRb;
    private bool _isGrabed;

    private void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.E)) Grab(_isGrabed);
        
        if (_isGrabed) Swinging();
    }

    private void Grab(bool value)
    {
        if (!value)
        {
            SetClosestJoint();
            GrabJoint();
        }
        else
        {
            FreeJoint();
        }
    }
    
    private void Swinging()
    {
        _playerRb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * _swingForce, 0));
    }
    
    private void GrabJoint()
    {
        if (_closestHinge == null) return;
        
        _closestHinge.transform.position = transform.position;
        _addedHinge = _closestHinge.AddComponent<HingeJoint2D>();
        _addedHinge.connectedBody = _playerRb;
        _playerRb.constraints = RigidbodyConstraints2D.None;
        _playerMovement.enabled = false;
        _playerRb.gravityScale = 2;
        _isGrabed = true;
    }

    private void FreeJoint()
    {
        transform.rotation = Quaternion.identity;
        _playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _playerMovement.enabled = true;
        Destroy(_addedHinge);
        _addedHinge = null;
        _isGrabed = false;
    }

    private void SetClosestJoint()
    {
        _closestHinge = null;
        
        var colliders = GetAllColliders();
        var joints = GetHingeJoints(colliders);

        if (joints.Length == 0) return;

        _closestHinge = GetClosestJoint(joints);
    }

    private Collider2D[] GetAllColliders() =>
        Physics2D.OverlapCircleAll(transform.position, _connectionRadius, _layer);

    private HingeJoint2D[] GetHingeJoints(Collider2D[] colliders)
    {
        var joints = new List<HingeJoint2D>();

        foreach (var c in colliders)
            if (c.TryGetComponent(out HingeJoint2D joint))
                joints.Add(joint);

        return joints.ToArray();
    }

    private HingeJoint2D GetClosestJoint(HingeJoint2D[] joints)
    {
        var closestJoint = joints[0];
        var distance = Vector2.Distance(transform.position, closestJoint.transform.position);

        foreach (var joint in joints)
        {
            if (Vector2.Distance(transform.position, joint.transform.position) < distance)
                closestJoint = joint;
        }

        return closestJoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _connectionRadius);
    }
}