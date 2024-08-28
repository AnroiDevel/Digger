using UnityEngine;
using UnityEngine.Tilemaps;

namespace MultiTool
{

    /// <summary>
    /// ���������� �������� ����� �� ������ ������ �� JSON-������ ��� ������� ������.
    /// </summary>
    public class TileMapGenerator : MonoBehaviour
    {
        [SerializeField] private TilemapStrengthDisplay _strengthDisplay;
        [SerializeField] private Tilemap _tilemap;                  // ������ �� ��������� Tilemap
        [SerializeField] private TilesData _tileDataSO;              // ������ �� ScriptableObject � ����������� � ������
        [SerializeField] private TextAsset[] _levelData;            // ������ JSON-������ ��� �������

        private LevelData _currentLevelData;                        // �������� ������� ������ ������

        /// <summary>
        /// ������� ������� �������� �����.
        /// </summary>
        public void ClearTileMap()
        {
            _tilemap.ClearAllTiles();
        }

        /// <summary>
        /// ��������� ������ ������ �� JSON-�����.
        /// </summary>
        /// <param name="file">JSON-���� ������ � ������� TextAsset.</param>
        private void LoadLevelData(TextAsset file)
        {
            if(file == null)
            {
                Debug.LogError("LoadLevelData: ��������� JSON-���� ����.");
                return;
            }

            _currentLevelData = JsonUtility.FromJson<LevelData>(file.text);
        }

        /// <summary>
        /// ���������� ����� �� ����� �� ������ ������ ������.
        /// </summary>
        private void GenerateTiles()
        {
            ClearTileMap();

            if(_currentLevelData == null)
            {
                Debug.LogError("GenerateTiles: ������ ������ �� ���� ���������.");
                return;
            }

            foreach(var cell in _currentLevelData.cells)
            {
                TileBase tile = _tileDataSO.GetTileDataByName(cell.tileName)?.Tiles[0];

                if(tile != null)
                {
                    _tilemap.SetTile((Vector3Int)cell.position, tile);
                }
            }

            Debug.Log($"��������� �������: {_currentLevelData.startPosition}, �������� �������: {_currentLevelData.finishPosition}");
            PlayerController.Instance.transform.position = new Vector3(_currentLevelData.startPosition.x, _currentLevelData.startPosition.y, 0);
            _strengthDisplay.InitializeTileStrengthDict();
            // ����� ����� �������� ������ ��� ������������� ��������� � �������� ������� � ����
        }

        /// <summary>
        /// ������������� ������� � ��������� ��� ������.
        /// </summary>
        /// <param name="levelIndex">������ ������ � ������� levelData.</param>
        public void SetLevel(int levelIndex)
        {
            if(levelIndex < 0 || levelIndex >= _levelData.Length)
            {
                Debug.LogError("SetLevel: ������������ ������ ������.");
                return;
            }

            LoadLevelData(_levelData[levelIndex]);
            GenerateTiles();
        }
    }
}
