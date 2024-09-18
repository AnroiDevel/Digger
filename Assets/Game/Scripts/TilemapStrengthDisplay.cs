using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace MultiTool
{
    public class TilemapStrengthDisplay : MonoBehaviour
    {
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Tilemap _tilemapCracks;
        [SerializeField] private GameObject _dropPrefab;
        [SerializeField] private TilesData _tileData;
        [SerializeField] private DropItemDatabase _dropItemDatabase;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private BlockHitController _blockHitController;

        private Dictionary<TileBase, int> _tileStrengthDict;
        private Dictionary<Vector3Int, int> _tileCurrentStrengthDict;
        private Dictionary<Vector3Int, TextMesh> _tileTextObjects;
        private Dictionary<Vector3Int, GameObject> _tileGODictionary;

        private Camera _camera;
        private Color _disableColor = new(0, 0, 0, 0);
        private bool _tilemapInit = false;

        // Инициализация словарей и заполнение информации о тайлах
        public void InitializeTileStrengthDict()
        {
            _camera = Camera.main;

            _tilemapInit = false;
            InitializeDictionaries();
            FillTileStrengthData();
            DisplayStrengthOnTiles();

            _tilemapInit = true;
        }

        // Инициализация словарей
        private void InitializeDictionaries()
        {
            _tileStrengthDict = new Dictionary<TileBase, int>();
            _tileCurrentStrengthDict = new Dictionary<Vector3Int, int>();
            _tileTextObjects = new Dictionary<Vector3Int, TextMesh>();
            _tileGODictionary = new Dictionary<Vector3Int, GameObject>();
        }

        // Заполнение данных о прочности тайлов
        private void FillTileStrengthData()
        {
            foreach(TileData tileData in _tileData.TileDatas)
            {
                for(int i = 0; i < tileData.Tiles.Length; i++)
                {
                    TileBase tile = tileData.Tiles[i];
                    if(tileData.Durability.Length > i)
                    {
                        _tileStrengthDict[tile] = tileData.Durability[i];
                    }
                }
            }
        }

        // Отображение прочности на всех тайлах
        private void DisplayStrengthOnTiles()
        {
            foreach(Vector3Int pos in _tilemap.cellBounds.allPositionsWithin)
            {
                if(!_tilemap.HasTile(pos))
                    continue;

                TileBase tile = _tilemap.GetTile(pos);
                int strength = GetTileStrength(tile);

                _tileCurrentStrengthDict[pos] = strength;

                if(!_tilemapInit)
                    InitializeTileGameObject(pos, tile);

                UpdateTileText(pos, strength);
                IsObjectVisible(pos, _camera);
            }
        }

        // Инициализация игровых объектов для тайлов
        private void InitializeTileGameObject(Vector3Int pos, TileBase tile)
        {
            GameObject tileGO = _tilemap.GetInstantiatedObject(pos);
            if(tileGO == null)
                return;
            var finish = tileGO.GetComponent<Finish>();
            if(!finish)
                _tileGODictionary[pos] = tileGO;

            var animController = _tileData.GetTileAnimator(tile.name);
            if(animController != null)
            {
                _tilemap.SetColor(pos, _disableColor);
                var tileController = tileGO.GetComponent<TileGOController>();
                tileController.InitAnimation(animController);
            }
        }

        // Обновление текста на тайлах в зависимости от прочности
        private void UpdateTileText(Vector3Int pos, int strength)
        {
            if(_tileGODictionary.TryGetValue(pos, out GameObject savedGO))
            {
                TextMesh textMesh = savedGO.GetComponentInChildren<TextMesh>();
                if(textMesh != null)
                {
                    _tileTextObjects[pos] = textMesh;
                    textMesh.text = strength.ToString();
                    textMesh.color = (strength <= 0) ? _disableColor : Color.white;
                }
            }
        }

        // Проверка видимости объекта на экране
        public bool IsObjectVisible(Vector3Int pos, Camera camera)
        {
            // Преобразуем клетку тайла в мировые координаты и проверяем видимость на экране
            Vector3 viewportPoint = camera.WorldToViewportPoint(_tilemap.CellToWorld(pos));

            // Проверка, находится ли объект в пределах видимости камеры (от 0 до 1 по осям x и y, и z > 0)
            bool isVisible = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                             viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                             viewportPoint.z > 0;

            // Проверяем наличие объекта в словаре перед активацией
            if(_tileGODictionary.TryGetValue(pos, out var tileGO))
            {
                tileGO.SetActive(isVisible);
            }
            //else
            //{
            //    Debug.LogWarning($"Объект для позиции {pos} не найден в _tileGODictionary");
            //}

            return isVisible;
        }

        // Получение прочности тайла
        private int GetTileStrength(TileBase tile)
        {
            return _tileStrengthDict.TryGetValue(tile, out int strength) ? strength : 0;
        }

        // Обновление цвета прочности тайла в зависимости от расстояния и формы игрока
        public void UpdateTileStrengthColor(Vector3 playerPosition, float radius, PlayerForm playerForm)
        {
            Vector3Int playerCell = _tilemap.WorldToCell(playerPosition);

            foreach(Vector3Int pos in _tilemap.cellBounds.allPositionsWithin)
            {
                if(!_tileTextObjects.ContainsKey(pos))
                    continue;

                float distance = Vector3.Distance(_tilemap.CellToWorld(pos), playerCell);
                if(distance <= radius && _playerController.IsReady)
                {
                    IsObjectVisible(pos, _camera);
                    UpdateTileTextColor(pos, playerForm);
                }
                else
                {
                    HideTileText(pos);
                }
            }
        }

        // Обновление цвета текста прочности тайла
        private void UpdateTileTextColor(Vector3Int pos, PlayerForm playerForm)
        {
            if(_tileTextObjects[pos].TryGetComponent(out TextMesh textMesh))
            {
                int strength = int.Parse(textMesh.text);
                if(strength > 0)
                {
                    TileBase tile = _tilemap.GetTile(pos);
                    TileType tileType = _tileData.GetTileType(tile.name);
                    HarvestType harvestType = playerForm.GetHarvestType(tileType);

                    textMesh.color = harvestType switch
                    {
                        HarvestType.Perfect => Color.green,
                        HarvestType.Harvestable => Color.yellow,
                        HarvestType.Unharvestable => Color.red,
                        _ => Color.white,
                    };
                }
                else
                {
                    textMesh.color = _disableColor;
                }
            }
        }

        // Скрытие текста прочности тайла
        private void HideTileText(Vector3Int pos)
        {
            if(_tileTextObjects[pos].TryGetComponent(out TextMesh textMesh))
            {
                textMesh.color = _disableColor;
            }
        }

        // Уменьшение прочности тайла
        public void ReduceTileStrength(Vector3Int tilePos)
        {
            if(_tileTextObjects.ContainsKey(tilePos) &&
                _tileTextObjects[tilePos].TryGetComponent(out TextMesh textMesh) &&
                textMesh.color != Color.red && textMesh.color != _disableColor &&
                _playerController.IsReady)
            {
                _playerController.Mine();
                int currentStrength = int.Parse(textMesh.text);
                currentStrength -= _playerController.Form.Damage;
                if(currentStrength <= 0)
                {
                    DestroyTile(tilePos);
                }
                else
                {
                    UpdateTileAfterHit(tilePos, currentStrength);
                }
            }
        }

        // Уничтожение тайла и создание дропа
        private void DestroyTile(Vector3Int tilePos)
        {
            TileBase tile = _tilemap.GetTile(tilePos);
            _tilemapCracks.SetTile(tilePos, null);

            string dropName = _dropItemDatabase.GetNameByTileName(tile.name);
            if(dropName != null)
            {
                Vector3 worldPos = _tilemap.CellToWorld(tilePos);
                Instantiate(_dropPrefab, worldPos, Quaternion.identity).name = dropName;
            }

            _tilemap.SetTile(tilePos, null);
            _tileTextObjects.Remove(tilePos);
            _blockHitController.TileDestroy(true, tilePos);
        }

        // Обновление тайла после удара
        private void UpdateTileAfterHit(Vector3Int tilePos, int currentStrength)
        {
            TileBase tile = _tilemap.GetTile(tilePos);
            TileBase crackTile = _tileData.GetCrackTile(tile.name, currentStrength);
            _tilemapCracks.SetTile(tilePos, crackTile);

            if(_tileGODictionary.TryGetValue(tilePos, out GameObject savedGO))
            {
                TextMesh textMesh = savedGO.GetComponentInChildren<TextMesh>();
                textMesh.text = currentStrength.ToString();
                _tileTextObjects[tilePos] = textMesh;
            }

            _blockHitController.TileDestroy(false, tilePos);
        }
    }
}
