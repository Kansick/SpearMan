using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
public class PerlinNoize : MonoBehaviour
{
    [SerializeField] private float _mapWidth;
    [SerializeField] private float _mapHeight;
    private List<float> _heights = new List<float>();

    [SerializeField] private GameObject _square;
    [SerializeField] private Transform _mapParent;

    private DevelopersInputs _inputs;
    private float _offsetX;
    private float _offsetY;
    private float _scale = 10f;
    [SerializeField] private string _seed;
    [SerializeField] private float _delayGenerateMap;
    [SerializeField] private int _countLive = 200;

    // Кэши для ускорения
    private Dictionary<string, GameObject> _cellCache;
    private Dictionary<GameObject, SpriteRenderer> _rendererCache;

    private void Awake()
    {
        _inputs = new DevelopersInputs();
        _inputs.Enable();

        _offsetX = Random.Range(0f, 1000f);
        _offsetY = Random.Range(0f, 1000f);

        if (string.IsNullOrEmpty(_seed))
            _seed = _offsetX.ToString() + "_" + _offsetY.ToString();
        else
        {
            string[] _offsets = _seed.Split('_');
            _offsetX = float.Parse(_offsets[0]);
            _offsetY = float.Parse(_offsets[1]);
        }
    }

    private void OnEnable() => _inputs.Scene.Reload.performed += ReloadScene;

    private void OnDisable() => _inputs.Scene.Reload.performed -= ReloadScene;

    private void ReloadScene(InputAction.CallbackContext obj)
    {
        _inputs.Disable();
        _inputs.Dispose();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Start()
    {
        CreateHeights();
        CreateMap();
        BuildCache(); // Строим кэш после создания карты
        //StartCoroutine(Live(0));
    }

    private void BuildCache()
    {
        _cellCache = new Dictionary<string, GameObject>();
        _rendererCache = new Dictionary<GameObject, SpriteRenderer>();

        for (int i = 0; i < _mapParent.childCount; i++)
        {
            GameObject cell = _mapParent.GetChild(i).gameObject;
            _cellCache[cell.name] = cell;
            _rendererCache[cell] = cell.GetComponent<SpriteRenderer>();
        }
    }

    private IEnumerator Live(int count)
    {
        for (int i = 0; i < _mapParent.childCount; i++)
        {
            GameObject cell = _mapParent.GetChild(i).gameObject;
            Color colorCell = _rendererCache[cell].color;
            string[] posCell = cell.name.Split('_');
            List<GameObject> _cellsNeighbors = GetAllNeighbors(int.Parse(posCell[0]), int.Parse(posCell[1]));
            if (colorCell == Color.black)
            {
                RuleThreeWhiteToOneWhite(cell, _cellsNeighbors);
            }
            if (_delayGenerateMap > 0)
                yield return new WaitForSeconds(_delayGenerateMap);
            else
                yield return null;
        }
        if (count < _countLive)
            StartCoroutine(Live(++count));
    }

    private void RuleThreeWhiteToOneWhite(GameObject _currentCell, List<GameObject> _cellsNeighbors)
    {
        int colorWhiteCount = 0;
        foreach (GameObject cell in _cellsNeighbors)
        {
            Color colorCell = _rendererCache[cell].color;
            if (colorCell == Color.white)
            {
                colorWhiteCount++;
            }
        }
        if (colorWhiteCount >= 2)
        {
            _rendererCache[_currentCell].color = Color.white;
        }
    }

    private List<GameObject> GetAllNeighbors(int x, int y)
    {
        List<GameObject> neighbors = new List<GameObject>();

        Vector2Int[] directions = {
            new Vector2Int(0, 1),    // верх
            new Vector2Int(1, 0),    // право
            new Vector2Int(0, -1),   // низ
            new Vector2Int(-1, 0),   // лево
        };

        foreach (var dir in directions)
        {
            string neighborName = (x + dir.x) + "_" + (y + dir.y);
            if (_cellCache.TryGetValue(neighborName, out GameObject neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private void CreateMap()
    {
        int heightNumber = 0;
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                GameObject square = Instantiate(_square, _mapParent);
                square.name = j + "_" + i;
                square.transform.position = new Vector3(j, i, 0);
                SetColorSquare(square, _heights[heightNumber]);
                _heights[heightNumber] = _heights[heightNumber] < 0.5 ? 0 : 1;
                heightNumber++;
            }
        }
    }

    private void CreateHeights()
    {
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                float xCoord = _offsetX + (float)j / _mapWidth * _scale;
                float yCoord = _offsetY + (float)i / _mapHeight * _scale;
                _heights.Add(Mathf.PerlinNoise(xCoord, yCoord));
            }
        }
    }

    private void SetColorSquare(GameObject square, float pair)
    {
        square.GetComponent<SpriteRenderer>().color = pair < 0.5 ? Color.white : Color.black;
    }
}