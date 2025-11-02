using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class PerlinNoize : MonoBehaviour
{
    [SerializeField] private float _mapWidth;
    [SerializeField] private float _mapHeight;
    [SerializeField] private List<float> _heights = new List<float>();

    [SerializeField] private GameObject _square;

    private DevelopersInputs _inputs;
    private float _offsetX;
    private float _offsetY;
    private float _scale = 10f;

    private void Awake()
    {
        _inputs = new DevelopersInputs();
        _inputs.Enable();

        _offsetX = Random.Range(0f, 1000f);
        _offsetY = Random.Range(0f, 1000f);
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
    }

    private void CreateMap()
    {
        int heightNumber = 0;
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                GameObject square = Instantiate(_square);
                square.transform.position = new Vector3(j, i, 0);
                SetColorSquare(square, _heights[heightNumber]);
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
        square.GetComponent<SpriteRenderer>().color = new Color(pair, pair, pair, 1f);
    }
}
