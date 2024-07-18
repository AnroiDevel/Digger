using UnityEngine;

namespace Digger
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private float _offsetBorder = 0.05f;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CameraController _cameraController;

        [SerializeField] private float _topOffsetMultiply = 10;
        [SerializeField] private float _bottomOffsetMultiply = 5;
        [SerializeField] private float _leftRightOffsetMultiply = 3;

        private Camera _camera;

        private void OnValidate()
        {
            _offsetBorder = Mathf.Clamp(_offsetBorder, 0.01f, 0.2f);
            _topOffsetMultiply = Mathf.Clamp(_topOffsetMultiply, 1, 10);
            _bottomOffsetMultiply = Mathf.Clamp(_bottomOffsetMultiply, 1, 10);
            _leftRightOffsetMultiply = Mathf.Clamp(_leftRightOffsetMultiply, 1, 10);

        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if(_playerController != null)
            {
                HandleMovementInput();
                HandleJumpInput();

                HandleInteractibleInput();
            }

        }


        private void FixedUpdate()
        {
            if(!GUIWindowManager.Instance.IsActive)
                _cameraController.SetTargetPosition(CheckCursorPosition(_playerController.transform.position, _camera));
        }

        private Vector3 CheckCursorPosition(Vector3 playerPosition, Camera camera)
        {
            Vector3 cursorPos = Input.mousePosition;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float edgeThresholdWidth = screenWidth * _offsetBorder;
            float edgeThresholdHeight = screenHeight * _offsetBorder;

            bool isLeft = cursorPos.x <= edgeThresholdWidth;
            bool isRight = cursorPos.x >= screenWidth - edgeThresholdWidth;
            bool isTop = cursorPos.y >= screenHeight - edgeThresholdHeight;
            bool isBottom = cursorPos.y <= edgeThresholdHeight;

            Vector3 offset;

            if(isLeft && isTop)
            {
                offset = new Vector3(-screenWidth / _leftRightOffsetMultiply, screenHeight / _topOffsetMultiply, 0);
            }
            else if(isLeft && isBottom)
            {
                offset = new Vector3(-screenWidth / _leftRightOffsetMultiply, -screenHeight / _bottomOffsetMultiply, 0);
            }
            else if(isRight && isTop)
            {
                offset = new Vector3(screenWidth / _leftRightOffsetMultiply, screenHeight / _topOffsetMultiply, 0);
            }
            else if(isRight && isBottom)
            {
                offset = new Vector3(screenWidth / _leftRightOffsetMultiply, -screenHeight / _bottomOffsetMultiply, 0);
            }
            else if(isLeft)
            {
                offset = new Vector3(-screenWidth / _leftRightOffsetMultiply, 0, 0);
            }
            else if(isRight)
            {
                offset = new Vector3(screenWidth / _leftRightOffsetMultiply, 0, 0);
            }
            else if(isTop)
            {
                offset = new Vector3(0, screenHeight / _topOffsetMultiply, 0);
            }
            else if(isBottom)
            {
                offset = new Vector3(0, -screenHeight / _bottomOffsetMultiply, 0);
            }
            else
            {
                return playerPosition;
            }

            // Convert screen offset to world space
            Vector3 screenOffset = cursorPos + offset;
            Vector3 worldOffset = camera.ScreenToWorldPoint(new Vector3(screenOffset.x, screenOffset.y, camera.nearClipPlane)) - camera.ScreenToWorldPoint(new Vector3(cursorPos.x, cursorPos.y, camera.nearClipPlane));

            return playerPosition + worldOffset;
        }


        private void HandleMovementInput()
        {
            if(GUIWindowManager.Instance.IsActive)
            {
                return;
            }

            float moveInput = Input.GetAxis("Horizontal");
            _playerController.Move(moveInput);
        }

        private void HandleJumpInput()
        {
            if(GUIWindowManager.Instance.IsActive)
            {
                return;
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                _playerController.Jump();
            }
        }

        private void HandleInteractibleInput()
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                _playerController.ReadyInteractible();
            }
        }

    }
}
