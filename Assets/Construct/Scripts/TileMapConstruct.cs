using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;

namespace MultiTool
{
    public class TileMapConstruct : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;                // ������ �� Tilemap
        [SerializeField] private TilesData _tileDataSO;           // ScriptableObject � ������� � ������

        /// <summary>
        /// ��������� ������� ����� ������ � JSON ����.
        /// </summary>
        public void SaveTileMap()
        {
            Dictionary<Vector2Int, string> tileMapData = new Dictionary<Vector2Int, string>();
            Vector2Int startPos = Vector2Int.zero;
            Vector2Int finishPos = Vector2Int.zero;

            // �������� �� ���� ������� Tilemap � ��������� ���������� � ����������� �������.
            BoundsInt bounds = _tilemap.cellBounds;
            for(int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for(int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int localPlace = new Vector3Int(x, y, 0);
                    TileBase tile = _tilemap.GetTile(localPlace);
                    if(tile != null)
                    {
                        if(tile.name == "Start")
                        {
                            startPos = new Vector2Int(x, y);
                        }
                        else if(tile.name == "Finish")
                        {
                            finishPos = new Vector2Int(x, y);
                        }

                        string tileName = _tileDataSO.GetTileDataByName(tile.name).NameEn;
                        if(!string.IsNullOrEmpty(tileName))
                        {
                            tileMapData[new Vector2Int(x, y)] = tileName;
                        }
                    }
                }
            }

            // ������� ������ LevelData � ��������� ��� �������.
            List<CellData> cellDataList = new List<CellData>();
            foreach(var tileEntry in tileMapData)
            {
                cellDataList.Add(new CellData { position = tileEntry.Key, tileName = tileEntry.Value });
            }

            LevelData levelData = new()
            {
                gridWidth = _tilemap.size.x,
                gridHeight = _tilemap.size.y,
                startPosition = startPos,
                finishPosition = finishPos,
                cells = cellDataList.ToArray()
            };

            // ��������� � JSON ����.
            string json = JsonUtility.ToJson(levelData, true);
            string filePath = Path.Combine(Application.dataPath, "Game/Data/JSON/TileMapData.json");
            File.WriteAllText(filePath, json);
            Debug.Log($"Tilemap �������� � {filePath}");
        }
    }
}
