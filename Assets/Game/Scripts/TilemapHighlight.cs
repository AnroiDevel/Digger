using UnityEngine;
using UnityEngine.Tilemaps;

namespace Digger
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
                    Debug.Log($"Unhighlighted tile at {_previousHighlightPos}");
                }

                // �������� ������� ����
                GameObject highlightObject = _tilemap.GetInstantiatedObject(tilePos);
                if(highlightObject != null)
                {
                    highlightObject.GetComponent<SpriteRenderer>().enabled = true;
                    Debug.Log($"Highlighted tile at {tilePos}");
                }

                _previousHighlightPos = tilePos;
                _previousHighlightObject = highlightObject;
            }
            else if(_previousHighlightObject != null)
            {
                // ������� ��������� � ����������� �����
                _previousHighlightObject.GetComponent<SpriteRenderer>().enabled = false;
                Debug.Log($"Unhighlighted tile at {_previousHighlightPos}");
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
