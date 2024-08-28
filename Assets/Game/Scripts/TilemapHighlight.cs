using UnityEngine;
using UnityEngine.Tilemaps;

namespace MultiTool
{
    public class TilemapHighlight : MonoBehaviour
    {
        [SerializeField]
        private Tilemap _tilemap;

        [SerializeField]
        private TilemapStrengthDisplay _strengthDisplay;

        [SerializeField]
        private PlayerController _playerController;

        private Vector3Int _previousHighlightPos;
        private GameObject _previousHighlightObject;

        private void Update()
        {
            if(!_playerController.gameObject.activeInHierarchy)
                return;

            HighlightTileUnderCursor();

            if(Input.GetMouseButtonDown(0)) // �������� ������� ����� ������ ����
            {
                HandleTileClick();
            }
        }

        private void HighlightTileUnderCursor()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePos = _tilemap.WorldToCell(mouseWorldPos);

            if(_tilemap.HasTile(tilePos))
            {
                // ������� ��������� � ����������� �����
                if(_previousHighlightObject != null && _previousHighlightPos != tilePos)
                {
                    _previousHighlightObject.GetComponent<SpriteRenderer>().enabled = false;
                }

                // �������� ������� ����
                GameObject highlightObject = _tilemap.GetInstantiatedObject(tilePos);
                if(highlightObject != null)
                {
                    if(highlightObject.TryGetComponent(out SpriteRenderer spriteRenderer))
                    {
                        spriteRenderer.enabled = true;
                        _previousHighlightPos = tilePos;
                        _previousHighlightObject = highlightObject;
                    }
                }
                else
                {
                    _previousHighlightObject = null;
                }

            }
            else if(_previousHighlightObject != null)
            {
                // ������� ��������� � ����������� �����
                _previousHighlightObject.GetComponent<SpriteRenderer>().enabled = false;
                _previousHighlightObject = null;
            }
        }


        private void HandleTileClick()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePos = _tilemap.WorldToCell(mouseWorldPos);

            if(_tilemap.HasTile(tilePos))
            {
                _strengthDisplay.ReduceTileStrength(tilePos);
            }
        }

    }
}
