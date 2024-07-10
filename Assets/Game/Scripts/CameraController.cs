using UnityEngine;

namespace Digger
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]private Transform _player; // ����, �� ������� ������ ������
        [SerializeField]private float _smoothSpeed = 0.125f; // �������� �������� ���������� ������

        private Vector3 offset; // ������ ����� ������� � �������

        private void Start()
        {
            offset = transform.position - _player.position; // ��������� ��������� ������
        }

        private void FixedUpdate()
        {
            if(_player != null)
            {
                Vector3 desiredPosition = _player.position + offset; // �������� ������� ������

                // ���������� SmoothDamp ��� �������� ���������� ������
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed);
                transform.position = smoothedPosition;
            }
        }
    }
}
