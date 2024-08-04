using UnityEngine;
using UnityEngine.Tilemaps;

namespace MultiTool
{
    public class TilemapHighlight : MonoBehaviour
    {
        [SerializeField]
        private Tilemap _tilemap; // ������ �� Tilemap

        [SerializeField]
        private TilemapStrengthDisplay _strengthDisplay;


        private Vector3Int _previousHighlightPos;
        private GameObject _previousHighlightObject;

        private void Update()
        {
            if(!PlayerController.Instance.gameObject.activeInHierarchy)
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
                    highlightObject.GetComponent<SpriteRenderer>().enabled = true;
                }

                _previousHighlightPos = tilePos;
                _previousHighlightObject = highlightObject;
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
