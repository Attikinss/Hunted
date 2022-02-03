using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public sealed class PlayerController : MonoBehaviour {
    
    // TODO: Decouple this class and allow pawn attaching via entity pool
    [SerializeField, Tooltip("The input handler used to link to a pawn.")] private PlayerInput m_Input;
    [SerializeField, Tooltip("[Temporary] The pawn this controller will attach to.")] private Pawn m_Pawn;

    private void Awake() {
        // Auto assign if user has not done it already
        if (m_Input == null)
            m_Input = GetComponent<PlayerInput>();
    }

    private void Start() {
        // Attach to pawn if one is assigned
        m_Pawn?.Attach(m_Input);
    }
}