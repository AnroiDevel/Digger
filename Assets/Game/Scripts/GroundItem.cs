using System.Collections;
using UnityEngine;

namespace MultiTool
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class GroundItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _phisicCollider;

        private DropItem _dropItem;

        private PlayerController _playerController;
        private Transform _player;
        [SerializeField]private float _speed = 1;
        private Coroutine _coroutineWay;


        private void Start()
        {
            _player = PlayerController.Instance.transform;
            _playerController = _player.GetComponent<PlayerController>();
        }

        public void Init()
        {
            _dropItem = GameManager.Instance.DropItemDatabase.GetDropItemByNameEN(name);
            _spriteRenderer.sprite = _dropItem.Sprite;
            var collider = _spriteRenderer.GetComponent<CircleCollider2D>();
            collider.radius = _spriteRenderer.sprite.bounds.size.x/2;

            var rb = GetComponent<Rigidbody2D>();
            rb.AddForce(Vector2.up * Random.Range(1, 10));
        }


        private void FixedUpdate()
        {
            if(_coroutineWay == null)
                if(Vector2.Distance(transform.position, _player.position) < _playerController.GatherRadius)
                {
                    _coroutineWay = StartCoroutine(MoveToTargetCoroutine());
                }
        }

        // �������� ��� �������� �������� � ����
        private IEnumerator MoveToTargetCoroutine()
        {
            _phisicCollider.enabled = false;
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            while(Vector3.Distance(transform.position, _player.position) > 0.01f) // ��������� ���������� �� ����
            {
                transform.position = Vector3.MoveTowards(transform.position, _player.position, _speed * Time.deltaTime); // ������� ������
                yield return null; // ���� ���������� �����
            }

            transform.position = _player.position;
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.CompareTag("Player"))
            {
                if(InventoryManager.Instance.AddItem(_dropItem.NameItemEN, 1))
                {
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("�� ������");
                }

            }
        }
    }
}
