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

        }

        private void HighlightTileUnderCursor()
        {
            Vector3Int tilePos = GetTilePositionUnderCursor();

            if(_tilemap.HasTile(tilePos))
            {
                HighlightCurrentTile(tilePos);

                if(Input.GetMouseButtonUp(0))
                {
                    HandleTileClick(tilePos);
                }
            }
            else
            {
                ClearPreviousHighlight();
            }
        }

        private Vector3Int GetTilePositionUnderCursor()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return _tilemap.WorldToCell(mouseWorldPos);
        }

        private void HighlightCurrentTile(Vector3Int tilePos)
        {
            if(_previousHighlightObject != null && _previousHighlightPos != tilePos)
            {
                ClearPreviousHighlight();
            }

            GameObject highlightObject = _tilemap.GetInstantiatedObject(tilePos);

            if(highlightObject != null && highlightObject.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                SetHighlightColor(highlightObject, spriteRenderer);
                spriteRenderer.enabled = true;

                _previousHighlightPos = tilePos;
                _previousHighlightObject = highlightObject;
            }
            else
            {
                _previousHighlightObject = null;
            }
        }

        private void SetHighlightColor(GameObject highlightObject, SpriteRenderer spriteRenderer)
        {
            var tm = highlightObject.GetComponentInChildren<TextMesh>();
            if(tm != null)
            {
                spriteRenderer.color = tm.color;
            }
        }

        private void ClearPreviousHighlight()
        {
            if(_previousHighlightObject != null)
            {
                _previousHighlightObject.GetComponent<SpriteRenderer>().enabled = false;
                _previousHighlightObject = null;
            }
        }

        private void HandleTileClick(Vector3Int tilePos)
        {
            if(_tilemap.HasTile(tilePos))
            {
                int flip = tilePos.x + 0.5f > _playerController.transform.position.x ? 1 : -1;
                _playerController.Move(flip);
                _strengthDisplay.ReduceTileStrength(tilePos);
            }
        }

    }
}
