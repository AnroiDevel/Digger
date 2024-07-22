using System.Collections;
using UnityEngine;

namespace MultiTool
{
    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(SpriteRenderer))]
    [ExecuteInEditMode]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _gravityCurve;
        [SerializeField] private AnimationCurve _jumpVelocityCurve;

        public static PlayerController Instance { get; private set; }

        [Header("Player Settings")]

        [Header("������ ����� ��������")]
        [Tooltip("������, � ������� ��� �������� ������������� � �����")]
        [SerializeField]
        [Range(0, 5)]
        private int _gatherRadius = 0;

        [Header("������ ���������� ��������")]
        [Tooltip("������, � ������� ����� ����� ������� ���� ��� ����������")]
        [SerializeField]
        [Range(1, 5)]
        private int _breakRadius = 3;

        [Header("������ ��������������")]
        [Tooltip("������, � ������� ����� ����� ����������������� � �������������� ���������")]
        [SerializeField]
        [Range(0, 5)]
        private int _interactionRadius = 0;

        [Header("������� ������ �����")]
        [Tooltip("���������� ����� �� ������� �����")]
        [SerializeField]
        private int _moneyAmount = 0;

        [Header("����� ����� ������������")]
        [Tooltip("������� ������ ��������� �������� � ���")]
        [SerializeField]
        private float _teleportationDelay = 3f;

        [Header("����� ����� �����")]
        [Tooltip("������� ������ ����� ������� �����")]
        [SerializeField]
        private float _shapeshiftDelay = 3;

        [Header("�������� �������� �� �����������")]
        [Tooltip("�������� �������� ����� �� �����������")]
        [SerializeField]
        [Range(1, 10)]
        private float _moveSpeed = 1.0f;

        [Header("������������ ������ ������")]
        [Tooltip("������������ ������, �� ������� ����� �������� �����")]
        [SerializeField]
        [Range(1, 5)]
        private int _maxJumpHeight = 1;

        [Header("����� ���������� ������������ ������ ������")]
        [Tooltip("�����, �� ������� ����� ��������� ������������ ������ ������")]
        [SerializeField]
        private float _timeToJumpApex = 0.4f;

        [Space]

        [SerializeField] private Transform _groundCheck;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private TilemapStrengthDisplay _tilemapStrengthDisplay;
        [SerializeField] private BoxCollider2D _interactionTrigger;
        [SerializeField] private BoxCollider2D _gatherTrigger;

        [SerializeField] private Transform _hubPosition; // ������� ���� ��� ������������

        private bool _isTeleporting = false; // ���� ��� �������� ������������
        private Coroutine _teleportCoroutine; // ��������� ��� ������������

        private bool _isShapeshifting = false; // ���� ��� �������� �������� ����� �����
        private Coroutine _shapeshiftCoroutine; // ��������� ��� ����� �����
        private FormType _targetFormType; // ��� ������� ����� ��� �����

        private BoxCollider2D _playerBoxCollider;
        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;
        private bool _isGrounded;
        private PlayerForm _currentForm;
        private InteractiveObject _currentInteractive;
        private Animator _animator;

        private float _gravityScale;
        private float _jumpVelocity;
        private float _lastMineTime;

        public bool IsReady => !_isTeleporting && !_isShapeshifting && IsCooldown();

        public InteractiveObject InteractiveSprite => _currentInteractive;


        #region Initialization
        private void Awake()
        {
            InitializeComponents();
            SetupSingleton();
            SubscribeToEvents();
            UpdateColliderSize();
            CalculateJumpForce(); // ��������� ���� ������ ��� �������������
        }

        private void Start()
        {
            if(GameManager.Instance.FormController != null)
                _currentForm = GameManager.Instance.FormController.CurrentForm;
            UpdateTileStrengthDisplay();
        }

        private void OnValidate()
        {
            UpdateColliderSize();
            _interactionTrigger.edgeRadius = _interactionRadius;
            _gatherTrigger.edgeRadius = _gatherRadius;
            _moneyAmount = Mathf.Clamp(_moneyAmount, 0, int.MaxValue);
            _teleportationDelay = Mathf.Clamp(_teleportationDelay, 0, 100);
            _shapeshiftDelay = Mathf.Clamp(_shapeshiftDelay, 0, 100);
            CalculateJumpForce(); // ��������� ���� ������ ��� ��������� �������� � ����������
        }

        private void InitializeComponents()
        {
            _playerBoxCollider = GetComponent<BoxCollider2D>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();

        }

        private static void SetupSingleton()
        {
            if(Instance == null)
            {
                Instance = FindFirstObjectByType<PlayerController>();
                if(Application.isPlaying)
                {
                    DontDestroyOnLoad(Instance.gameObject);
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(PlayerController)}: Singleton  trying to initialize! Destroying duplicate object...", Instance.gameObject);
                Destroy(Instance.gameObject);
            }
        }

        public void SubscribeToEvents()
        {
            GameEventManager.Instance.Subscribe(GameEvent.OnChangeForm, OnChangeForm);
        }
        #endregion


        private void FixedUpdate()
        {
            //if(_rigidbody2D.velocity != Vector2.zero)
            UpdateTileStrengthDisplay();
            _gravityScale *= _gravityCurve.Evaluate(Time.time);
        }

        #region Movement
        public void Move(float moveInput)
        {
            var moveVelocity = new Vector2(moveInput * _moveSpeed, _rigidbody2D.velocity.y);
            _rigidbody2D.velocity = moveVelocity;
            if(moveVelocity.x != 0)
            {
                _spriteRenderer.flipX = moveVelocity.x > 0;
            }

            _animator.SetBool("IsJump", Mathf.Abs(moveVelocity.y) > 0.5f);
            _animator.SetBool("IsWalk", Mathf.Abs(moveVelocity.x) > 0.5f);
        }

        public void Jump()
        {
            CheckIfGrounded();

            if(_isGrounded)
            {
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpVelocity) * _jumpVelocityCurve.Evaluate(Time.time);
            }
        }

        private void CheckIfGrounded()
        {
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, 0.1f, _groundLayer);
        }

        private void UpdateTileStrengthDisplay()
        {
            if(_tilemapStrengthDisplay == null)
            {
                return;
            }

            if(_currentForm != null)
                _tilemapStrengthDisplay.UpdateTileStrengthColor(transform.position, _breakRadius, Color.green, Color.red, _currentForm);
        }

        public void StartTeleport()
        {
            if(_isTeleporting)
                return;

            _isTeleporting = true;
            _teleportCoroutine = StartCoroutine(TeleportRoutine());
            UpdateTileStrengthDisplay();
        }

        private IEnumerator TeleportRoutine()
        {
            // �������� �������� ������������
            _animator.SetTrigger("StartTeleport");

            yield return new WaitForSeconds(_teleportationDelay);

            if(_isTeleporting)
            {
                // ����������� ������ � ���
                transform.position = _hubPosition.position;

                // ����� ��������� ������������
                _isTeleporting = false;
                _animator.SetTrigger("EndTeleport");
            }
        }

        public void CancelTeleport()
        {
            if(!_isTeleporting)
                return;

            _isTeleporting = false;

            if(_teleportCoroutine != null)
            {
                StopCoroutine(_teleportCoroutine);
                _teleportCoroutine = null;
            }

            // �������� �������� Idle
            _animator.SetTrigger("CancelTeleport");
        }


        #endregion

        #region Form Management

        public void SwitchForm(FormType formType)
        {
            if(_currentForm != null && formType.ToString() == _currentForm.FormName)
            {
                // ������� ����� ��� �������, ������ �� ������
                return;
            }

            if(_isShapeshifting)
            {
                // ��� ���� ������� ����� �����, ������ �� ������
                return;
            }

            _targetFormType = formType;
            _isShapeshifting = true;
            _shapeshiftCoroutine = StartCoroutine(ShapeshiftRoutine());
            UpdateTileStrengthDisplay();

        }

        private IEnumerator ShapeshiftRoutine()
        {
            // �������� �������� ����� �����
            _animator.SetTrigger("StartShapeshift");

            yield return new WaitForSeconds(_shapeshiftDelay);

            if(_isShapeshifting)
            {
                PerformShapeshift(_targetFormType);

                // ����� ��������� ����� �����
                _isShapeshifting = false;
                _animator.SetTrigger("EndShapeshift");
            }
        }

        private void PerformShapeshift(FormType formType)
        {
            GameManager.Instance.FormController.SwitchForm(formType);
            UpdateTileStrengthDisplay();

        }


        public void CancelShapeshift()
        {
            if(!_isShapeshifting)
                return;

            _isShapeshifting = false;

            if(_shapeshiftCoroutine != null)
            {
                StopCoroutine(_shapeshiftCoroutine);
                _shapeshiftCoroutine = null;
            }

            // �������� �������� Idle
            _animator.SetTrigger("CancelShapeshift");
            UpdateTileStrengthDisplay();

        }


        private void OnChangeForm()
        {
            _currentForm = GameManager.Instance.FormController.CurrentForm;
            UpdateColliderSize();
            UpdatePlayerSprite();
        }

        public bool IsCooldown()
        {
            return Time.time - _lastMineTime > _currentForm.Cooldown;
        }

        private void UpdateColliderSize()
        {
            if(_playerBoxCollider == null)
            {
                _playerBoxCollider = GetComponent<BoxCollider2D>();
            }

            if(_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _playerBoxCollider.size = _spriteRenderer.sprite.bounds.size;
            _groundCheck.localPosition = _spriteRenderer.sprite.bounds.size;

            UpdateGroundPosition();
        }

        private void UpdatePlayerSprite()
        {
            if(_spriteRenderer != null)
            {
                _spriteRenderer.sprite = _currentForm?.Sprite;
            }
        }

        private void UpdateGroundPosition()
        {
            Vector3 colliderBottom = GetGroundCheckPoint();
            _groundCheck.position = colliderBottom;
        }

        private Vector3 GetGroundCheckPoint()
        {
            Vector2 offset = _playerBoxCollider.offset;
            Vector2 size = _playerBoxCollider.size;

            offset.y -= size.y / 2;

            return transform.position + (Vector3)offset;
        }
        #endregion

        #region Interaction
        public void GetDamage(int val)
        {
            if(_currentForm == null)
            {
                return;
            }

            _animator.SetTrigger("Mine");
            _currentForm.GetDamage(val);
            _lastMineTime = Time.time;
            UIController.Instance.SetStrenghtValue(_currentForm.Index, _currentForm.Strength);
        }

        internal void ReadyInteractible()
        {
            TryShowInteractiveWindow(_currentInteractive);
        }

        private static void TryShowInteractiveWindow(InteractiveObject interactiveSprite)
        {
            if(interactiveSprite == null)
            {
                return;
            }

            GUIWindowManager.Instance.ShowWindowAbovePosition(interactiveSprite.GetTopColliderPosition());
        }

        internal void ActivateReadyInteractible(InteractiveObject interactiveSprite)
        {
            _currentInteractive = interactiveSprite;
        }

        internal void HideInteractableWindow()
        {
            _currentInteractive = null;
        }
        #endregion

        #region Cleanup
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        public void UnsubscribeFromEvents()
        {
            GameEventManager.Instance.Unsubscribe(GameEvent.OnChangeForm, OnChangeForm);
        }
        #endregion

        #region Jump Calculation
        private void CalculateJumpForce()
        {
            if(_rigidbody2D == null)
                _rigidbody2D = GetComponent<Rigidbody2D>();

            _gravityScale = (2 * _maxJumpHeight) / Mathf.Pow(_timeToJumpApex, 2);
            _jumpVelocity = Mathf.Sqrt(2 * _gravityScale * _maxJumpHeight);

            _rigidbody2D.gravityScale = _gravityScale / Mathf.Abs(Physics2D.gravity.y);
        }
        #endregion
    }
}
