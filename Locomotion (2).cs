using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private InputManager m_inputManager = default;
    [SerializeField]
    private Rigidbody m_rb = default;
    [SerializeField]
    private CapsuleCollider m_collider = default;
    [SerializeField]
    private Animator m_animator = default;
    [SerializeField]
    private float m_offsetFloorY = 0.4f;
    [SerializeField]
    private float m_movementSpeed = 3f;

    private Vector3 m_movementDir;
    private float m_inputAmount;
    private Vector3 m_raycastFloorPos;
    private Vector3 m_combinedRaycast;
    private Vector3 m_gravity;
    private Vector3 m_floorMovement;
    private float m_groundRayLength;
    #endregion

    #region Builtin Methods
    private void FixedUpdate()
    {
        UpdateMovementInput();
        UpdatePhysics();
        UpdateAnimations();
    }
    #endregion

    #region Custom Methods
    private void UpdateMovementInput()
    {
        m_movementDir = Vector3.zero;

        Vector3 forward = m_inputManager.Forward * transform.forward;
        Vector3 sideway = m_inputManager.Sideway * transform.right;

        Vector3 combinedInput = (forward + sideway).normalized;

        m_movementDir = new Vector3(combinedInput.x, 0f, combinedInput.z);

        float inputMagnitude = Mathf.Abs(m_inputManager.Forward) + Mathf.Abs(m_inputManager.Sideway);
        m_inputAmount = Mathf.Clamp01(inputMagnitude);
    }

    private void UpdatePhysics()
    {
        m_groundRayLength = (m_collider.height * 0.5f) + m_offsetFloorY;

        if (FloorRaycasts(0,0, m_groundRayLength).transform == null)
        {
            m_gravity += (Vector3.up * Physics.gravity.y * Time.fixedDeltaTime);
        }

        m_rb.velocity = (m_movementDir * m_movementSpeed * m_inputAmount) + m_gravity;

        m_floorMovement = new Vector3(m_rb.position.x, FindFloor().y, m_rb.position.z);

        if(FloorRaycasts(0,0,m_groundRayLength).transform !=null && m_floorMovement != m_rb.position)
        {
            m_rb.MovePosition(m_floorMovement);
            m_gravity.y = 0;
        }
    }

    private void UpdateAnimations()
    {
        m_animator.SetFloat("Forward", m_inputManager.Forward);
        m_animator.SetFloat("Sideway", m_inputManager.Sideway);
    }

    private Vector3 FindFloor()
    {
        float raycastWidth = 0.3f;
        int floorAverage = 1;

        m_combinedRaycast = FloorRaycasts(0, 0, m_groundRayLength).point;
        floorAverage += (GetFloorAverage(raycastWidth, 0) + GetFloorAverage(-raycastWidth, 0) + GetFloorAverage(0, raycastWidth) + GetFloorAverage(0, -raycastWidth));
        
        return m_combinedRaycast / floorAverage;
    }

    private RaycastHit FloorRaycasts(float t_offsetx, float t_offsetz, float t_raycastLength)
    {
        RaycastHit hit;

        m_raycastFloorPos = transform.TransformPoint(0 + t_offsetx, m_collider.center.y, 0 + t_offsetz);
        Debug.DrawRay(m_raycastFloorPos, Vector3.down * m_groundRayLength, Color.magenta);

        Physics.Raycast(m_raycastFloorPos, -Vector3.up, out hit, t_raycastLength);

        return hit;
    }

    private int GetFloorAverage(float t_offsetx, float t_offsetz)
    {
        if (FloorRaycasts(t_offsetx, t_offsetz, m_groundRayLength).transform != null)
        {
            m_combinedRaycast += FloorRaycasts(t_offsetx, t_offsetz, m_groundRayLength).point;
            return 1;
        }
        return 0;
    }
    #endregion
}
