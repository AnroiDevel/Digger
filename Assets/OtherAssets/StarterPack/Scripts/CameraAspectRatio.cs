using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectRatio : MonoBehaviour
{
    [SerializeField] private float _targetAspect = 16.0f / 9.0f; // ������� ����������� ������
    private Camera _mainCamera;
    private float _lastAspectRatio;

    private void Start()
    {
        _mainCamera = GetComponent<Camera>();
        AdjustCamera(); // �������������� ��������� ������
    }

    private void Update()
    {
        // ���������, ���������� �� ����������� ������
        float currentAspect = (float)Screen.width / Screen.height;
        if(Mathf.Abs(currentAspect - _lastAspectRatio) > 0.01f)
        {
            _lastAspectRatio = currentAspect;
            AdjustCamera(); // ������������ ������ ��� ��������� ����������� ������
        }
    }

    private void AdjustCamera()
    {
        // �������� ������� ����������� ������ ������
        float windowAspect = (float)Screen.width / Screen.height;
        // ������������ �������, ����������� ��� ���������� �������� ����������� ������
        float scaleHeight = windowAspect / _targetAspect;

        if(scaleHeight < 1.0f) // ���� ����� ����, ��� �����
        {
            // ��������� ��� ������ � ����� (letterboxing)
            Rect rect = _mainCamera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            _mainCamera.rect = rect;
        }
        else // ���� ����� ���, ��� �����
        {
            // ��������� ��� ����� � ������ (pillarboxing)
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = _mainCamera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            _mainCamera.rect = rect;
        }
    }
}
