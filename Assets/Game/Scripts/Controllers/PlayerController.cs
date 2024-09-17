using System;
using UnityEngine;

namespace MultiTool
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    //[ExecuteInEditMode]
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _audioSourceJump;
        [SerializeField] private AnimationCurve _gravityCurve;
        [SerializeField] private AnimationCurve _jumpVelocityCurve;

        [Header("Радиус сбора ресурсов")]
        [Tooltip("Радиус, в котором все предметы притягиваются к герою")]
        [SerializeField, Range(0, 5)] private int _gatherRadius = 0;

        [Header("Радиус разрушения ресурсов")]
        [Tooltip("Радиус, в котором игрок может выбрать блок для разрушения")]
        [SerializeField, Range(1, 5)] private int _breakRadius = 3;

        [Header("Радиус взаимодействия")]
        [Tooltip("Радиус, в котором игрок может взаимодействовать с интерактивными объектами")]
        [SerializeField, Range(0, 5)] private int _interactionRadius = 0;

        [Header("Текущий баланс денег")]
        [Tooltip("Количество денег на балансе героя")]
        [SerializeField] private int _moneyAmount = 0;

        [Header("Скорость движения по горизонтали")]
        [Tooltip("Скорость движения героя по горизонтали")]
        [SerializeField, Range(1, 10)] private float _moveSpeed = 1.0f;

        [Header("Максимальная высота прыжка")]
        [Tooltip("Максимальная высота, на которую может прыгнуть герой")]
        [SerializeField, Range(1, 5)] private int _maxJumpHeight = 1;

        [Header("Время достижения максимальной высоты прыжка")]
        [Tooltip("Время, за которое герой достигает максимальной высоты прыжка")]
        [SerializeField] private float _timeToJumpApex = 0.4f;

        [Space]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private TilemapStrengthDisplay _tilemapStrengthDisplay;
        [SerializeField] private BoxCollider2D _interactionTrigger;
        [SerializeField] private BoxCollider2D _gatherTrigger;

        private static PlayerController _instance;
        private PlayerAnimation _playerAnimation;
        private PlayerShapeshift _playerShapeshift;
        private PlayerTeleportation _playerTeleportation;
        private BoxCollider2D _playerBoxCollider;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody2D;

        private bool _isGrounded;
        private PlayerForm _currentForm;
        private InteractiveObject _currentInteractive;
        private float _gravityScale;
        private float _jumpVelocity;
        private float _lastMineTime;

        public static PlayerController Instance => _instance;
        public int MoneyAmount => _moneyAmount;
        public bool IsReady => !_playerTeleportation.IsTeleporting && !_playerShapeshift.IsShapeshifting && IsCooldown && gameObject.activeInHierarchy;
        public PlayerShapeshift PlayerShapeshift => _playerShapeshift;
        public PlayerTeleportation PlayerTeleportation => _playerTeleportation;
        public InteractiveObject InteractiveSprite => _currentInteractive;
        public int GatherRadius => _gatherRadius;
        public PlayerForm Form => _currentForm;

        public bool IsCooldown => Time.time - _lastMineTime > _currentForm.Cooldown;

        #region Initialization

        private void Awake()
        {
            if(_instance == null)
            {
                _instance = this;
                InitializeComponents();
                SubscribeToEvents();
                CalculateJumpForce(); // Вычислить силу прыжка при инициализации
            }
            else
            {
                Destroy(gameObject);
            }
        }


        private void Start()
        {
            if(GameManager.Instance.FormController != null)
            {
                _currentForm = GameManager.Instance.FormController.CurrentForm;
            }
            UpdateTileStrengthDisplay();
            _lastMineTime = Time.time;
        }


        private void OnValidate()
        {
            UpdateColliderSize();
            _interactionTrigger.edgeRadius = _interactionRadius;
            _gatherTrigger.edgeRadius = _gatherRadius;
            _moneyAmount = Mathf.Clamp(_moneyAmount, 0, int.MaxValue);
            CalculateJumpForce(); // Вычислить силу прыжка при изменении значений в инспекторе
        }


        private void InitializeComponents()
        {
            _playerAnimation = GetComponent<PlayerAnimation>();
            _playerShapeshift = GetComponent<PlayerShapeshift>();
            _playerTeleportation = GetComponent<PlayerTeleportation>();

            _playerBoxCollider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }


        public void SubscribeToEvents()
        {
            GameEventManager.Instance.Subscribe(GameEvent.OnChangeForm, OnChangeForm);
        }
        #endregion


        private void FixedUpdate()
        {
            UpdateTileStrengthDisplay();
            _gravityScale *= _gravityCurve.Evaluate(Time.time);
        }

        public void AddMoney(int moneyCount)
        {
            _moneyAmount += moneyCount;
            GameEventManager.Instance.TriggerEvent(GameEvent.OnChangeMoney);
        }

        #region Movement

        public void Move(float moveInput)
        {
            var moveVelocity = new Vector2(moveInput * _moveSpeed, _rigidbody2D.velocity.y);
            _rigidbody2D.velocity = moveVelocity;
            if(moveVelocity.x != 0)
            {
                _spriteRenderer.flipX = moveVelocity.x < 0;
            }

            _playerAnimation.SetWalk(Mathf.Abs(moveVelocity.x) > 1f);
        }

        public void Jump()
        {
            CheckIfGrounded();

            if(_isGrounded)
            {
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, _jumpVelocity) * _jumpVelocityCurve.Evaluate(Time.time);
                _audioSourceJump.Play();
                _playerAnimation.SetJump();

            }
        }

        private void CheckIfGrounded()
        {
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, 0.1f, _groundLayer);
        }

        private void UpdateTileStrengthDisplay()
        {
            if(_tilemapStrengthDisplay == null || !_tilemapStrengthDisplay.gameObject.activeInHierarchy)
            {
                return;
            }

            if(_currentForm != null)
                _tilemapStrengthDisplay.UpdateTileStrengthColor(transform.position, _breakRadius, _currentForm);
        }

        #endregion


        #region Form Management

        private void OnChangeForm()
        {
            _currentForm = GameManager.Instance.FormController.CurrentForm;
            _animator.enabled = false;
            if(_currentForm.RuntimeAnimatorController)
            {
                _animator.runtimeAnimatorController = _currentForm.RuntimeAnimatorController;
                _animator.enabled = true;
            }
            UpdatePlayerSprite();
            UpdateColliderSize();
        }

        private void UpdateColliderSize()
        {
            if(_playerBoxCollider == null || _spriteRenderer == null)
            {
                InitializeComponents();
            }
            _playerBoxCollider.size = _spriteRenderer.sprite.bounds.size;
            //_groundCheck.localPosition = _spriteRenderer.sprite.bounds.size;

            //UpdateGroundPosition();
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
            _groundCheck.position = GetGroundCheckPoint();
        }

        private Vector3 GetGroundCheckPoint()
        {
            Vector2 offset = _playerBoxCollider.offset;
            Vector2 size = _playerBoxCollider.size;
            offset.y -= size.y / 2 + 0.1f;
            return transform.position + (Vector3)offset;
        }

        #endregion


        #region Interaction

        public void Mine()
        {
            if(_currentForm != null)
            {

                _playerAnimation.Mine();
                _currentForm.GetDamage(1);
                _lastMineTime = Time.time;
                UIController.Instance.SetStrengthValue(_currentForm.Index, _currentForm.Strength);
            }
        }

        public void ReadyInteractible()
        {
            TryShowInteractiveWindow(_currentInteractive);
        }

        private void TryShowInteractiveWindow(InteractiveObject interactiveSprite)
        {
            if(interactiveSprite != null)
            {
                _rigidbody2D.velocity = Vector3.zero;
                GUIWindowManager.Instance.ShowWindowAbovePosition(interactiveSprite.GetTopColliderPosition());
            }
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
            _gravityScale = (2 * _maxJumpHeight) / Mathf.Pow(_timeToJumpApex, 2);
            _jumpVelocity = Mathf.Sqrt(2 * _gravityScale * _maxJumpHeight);

            _rigidbody2D.gravityScale = _gravityScale / Mathf.Abs(Physics2D.gravity.y);
        }

        internal void Stop()
        {
            Debug.Log("Финиш");
        }

        #endregion
    }
}
