using System;
using UnityEngine;

namespace Digger
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _player; // ����, �� ������� ������ ������
        [SerializeField] private float _smoothSpeed = 0.125f; // �������� �������� ���������� ������

        private Vector3 _offset; // ������ ����� ������� � �������
        private Vector3 _targetPosition;

        private void Start()
        {
            _offset = transform.position - _player.position; // ��������� ��������� ������
        }

        private void FixedUpdate()
        {
            if(_player != null)
            {
                //_targetPosition = _player.position + _offset; // �������� ������� ������

                // ���������� SmoothDamp ��� �������� ���������� ������
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, _targetPosition, _smoothSpeed);
                transform.position = smoothedPosition;
            }
        }

        internal void SetTargetPosition(Vector3 target)
        {
            _targetPosition = target + _offset;

        }
    }
}