using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public sealed partial class GridManager : MonoBehaviour
{
    [Header("Settings")]
    public Score score;
    public MatrixTile matrixTile;
    public NewTimeValue newTimeValue;
    public SpacingOffset spacingOffset;
    public Border border;

    [Header("Others")]
    public GameDataContainer gameDataContainer;
    public LayerMask backgroundLayer;
    public float minSwipeDistance = 10.0f;

    private float halfTileWidth = 0.55f;
    private float spaceBetweenTiles = 1.1f;

    private int points;

    private List<GameObject> tiles;
    private Rect resetButton;
    private Rect gameOverButton;
    private Vector2 touchStartPosition = Vector2.zero;

    [SerializeField] private State state;

    private void Awake()
    {
        if (gameDataContainer == null)
            gameDataContainer = FindObjectOfType<GameDataContainer>();

        tiles = new List<GameObject>();
        state = State.Loaded;
    }

    private void Update()
    {
        if (state == State.GameOver)
        {
            gameDataContainer.DefeatPopup.popup.SetActive(true);
        }
        else if (state == State.Loaded)
        {
            state = State.WaitingForInput;
            GenerateRandomTile();
            GenerateRandomTile();
        }
        else if (state == State.WaitingForInput)
        {
#if UNITY_STANDALONE
            if (Input.GetButtonDown(Tag.Left))
            {
                if (MoveTilesLeft())
                {
                    state = State.CheckingMatches;
                }
            }
            else if (Input.GetButtonDown(Tag.Right))
            {
                if (MoveTilesRight())
                {
                    state = State.CheckingMatches;
                }
            }
            else if (Input.GetButtonDown(Tag.Up))
            {
                if (MoveTilesUp())
                {
                    state = State.CheckingMatches;
                }
            }
            else if (Input.GetButtonDown(Tag.Down))
            {
                if (MoveTilesDown())
                {
                    state = State.CheckingMatches;
                }
            }
            else if (Input.GetButtonDown(Tag.Reset))
            {
                Reset();
            }
            else if (Input.GetButtonDown(Tag.Quit))
            {
                Application.Quit();
            }
#endif

#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                touchStartPosition = Input.GetTouch(0).position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                Vector2 swipeDelta = (Input.GetTouch(0).position - touchStartPosition);
                if (swipeDelta.magnitude < minSwipeDistance)
                {
                    return;
                }
                swipeDelta.Normalize();
                if (swipeDelta.y > 0.0f && swipeDelta.x > -0.5f && swipeDelta.x < 0.5f)
                {
                    if (MoveTilesUp())
                    {
                        state = State.CheckingMatches;
                    }
                }
                else if (swipeDelta.y < 0.0f && swipeDelta.x > -0.5f && swipeDelta.x < 0.5f)
                {
                    if (MoveTilesDown())
                    {
                        state = State.CheckingMatches;
                    }
                }
                else if (swipeDelta.x > 0.0f && swipeDelta.y > -0.5f && swipeDelta.y < 0.5f)
                {
                    if (MoveTilesRight())
                    {
                        state = State.CheckingMatches;
                    }
                }
                else if (swipeDelta.x < 0.0f && swipeDelta.y > -0.5f && swipeDelta.y < 0.5f)
                {
                    if (MoveTilesLeft())
                    {
                        state = State.CheckingMatches;
                    }
                }
            }
#endif
        }
        else if (state == State.CheckingMatches)
        {
            GenerateRandomTile();
            if (CheckForMovesLeft())
            {
                ReadyTilesForUpgrading();
                state = State.WaitingForInput;
            }
            else
            {
                state = State.GameOver;
            }
        }
    }

    private Vector2 GridToWorldPoint(int x, int y) =>
         new(x + spacingOffset.Horizontal + border.Spacing * x,
            -y + spacingOffset.Vertical - border.Spacing * y);

    private Vector2 WorldToGridPoint(float x, float y) =>
        new((x - spacingOffset.Horizontal) / (1 + border.Spacing),
            (y - spacingOffset.Vertical) / -(1 + border.Spacing));

    private bool CheckForMovesLeft()
    {
        if (tiles.Count < matrixTile.Rows * matrixTile.Cols) return true;

        for (int x = 0; x < matrixTile.Cols; x++)
        {
            for (int y = 0; y < matrixTile.Rows; y++)
            {
                Tile currentTile = GetObjectAtGridPosition(x, y).GetComponent<Tile>();
                Tile rightTile = GetObjectAtGridPosition(x + 1, y).GetComponent<Tile>();
                Tile downTile = GetObjectAtGridPosition(x, y + 1).GetComponent<Tile>();

                if (x != matrixTile.Cols - 1 && currentTile.value == rightTile.value)
                {
                    return true;
                }
                else if (y != matrixTile.Rows - 1 && currentTile.value == downTile.value)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void GenerateRandomTile()
    {
        if (tiles.Count >= matrixTile.Rows * matrixTile.Cols)
            throw new UnityException("Unable to create new tile - grid is already full");

        int value;
        // find out if we are generating a tile with the lowest or highest value
        float highOrLowChance = Random.Range(0f, 0.99f);

        if (highOrLowChance >= 0.9f)
            value = newTimeValue.Highes;
        else
            value = newTimeValue.Lowest;

        // attempt to get the starting position
        int x = Random.Range(0, matrixTile.Cols);
        int y = Random.Range(0, matrixTile.Rows);

        // starting from the random starting position, loop through
        // each cell in the grid until we find an empty positio
        bool found = false;
        while (!found)
        {
            if (GetObjectAtGridPosition(x, y) == gameDataContainer.EmptyTile)
            {
                found = true;
                Vector2 worldPosition = GridToWorldPoint(x, y);
                GameObject obj;

                if (value == newTimeValue.Lowest)
                    obj = SimplePool.Spawn(gameDataContainer.tilePrefabs[0], worldPosition, transform.rotation);
                else
                    obj = SimplePool.Spawn(gameDataContainer.tilePrefabs[1], worldPosition, transform.rotation);

                tiles.Add(obj);
                TileAnimationHandler tileAnimManager = obj.GetComponent<TileAnimationHandler>();
                tileAnimManager.AnimateEntry();
            }

            x++;
            if (x >= matrixTile.Cols)
            {
                y++;
                x = 0;
            }

            if (y >= matrixTile.Rows)
            {
                y = 0;
            }
        }
    }

    private GameObject GetObjectAtGridPosition(int x, int y)
    {
        RaycastHit2D hit = Physics2D.Raycast(GridToWorldPoint(x, y), Vector2.right, border.Spacing);

        if (hit && hit.collider.gameObject.GetComponent<Tile>() != null)
            return hit.collider.gameObject;
        else
            return gameDataContainer.EmptyTile;
    }

    private bool MoveTilesUp()
    {
        bool hasMoved = false;
        for (int y = 0; y < matrixTile.Rows; y++)// for (int y = 1; y < matrixTile.Rows; y++)
        {
            for (int x = 0; x < matrixTile.Cols; x++)
            {
                GameObject obj = GetObjectAtGridPosition(x, y);

                if (obj == gameDataContainer.EmptyTile) continue;

                Vector2 raycastOrigin = obj.transform.position;
                raycastOrigin.y += halfTileWidth;
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.up, Mathf.Infinity);

                if (hit.collider != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != obj)
                    {
                        if (hitObject.CompareTag("Tile"))
                        {
                            Tile thatTile = hitObject.GetComponent<Tile>();
                            Tile thisTile = obj.GetComponent<Tile>();
                            if (CanUpgrade(thisTile, thatTile))
                            {
                                UpgradeTile(obj, thisTile, hitObject, thatTile);
                                hasMoved = true;
                            }
                            else
                            {
                                Vector3 newPosition = hitObject.transform.position;
                                newPosition.y -= spaceBetweenTiles;
                                if (!Mathf.Approximately(obj.transform.position.y, newPosition.y))
                                {
                                    obj.transform.position = newPosition;
                                    //obj.transform.position = Vector3.MoveTowards(obj.transform.position, newPosition, Time.deltaTime * 2);
                                    hasMoved = true;
                                }
                            }
                        }
                        else if (hitObject.CompareTag("Border"))
                        {
                            Vector3 newPosition = obj.transform.position;
                            newPosition.y = hit.point.y - halfTileWidth - border.Offset;
                            if (!Mathf.Approximately(obj.transform.position.y, newPosition.y))
                            {
                                obj.transform.position = newPosition;
                                hasMoved = true;
                            }
                        }
                    }
                }
            }
        }

        return hasMoved;
    }

    private bool MoveTilesDown()
    {
        bool hasMoved = false;
        for (int y = matrixTile.Rows - 1; y >= 0; y--)
        {
            for (int x = 0; x < matrixTile.Cols; x++)
            {
                GameObject obj = GetObjectAtGridPosition(x, y);

                if (obj == gameDataContainer.EmptyTile)
                {
                    continue;
                }

                Vector2 raycastOrigin = obj.transform.position;
                raycastOrigin.y -= halfTileWidth;
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, -Vector2.up, Mathf.Infinity);
                if (hit.collider != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != obj)
                    {
                        if (hitObject.CompareTag("Tile"))
                        {
                            Tile thatTile = hitObject.GetComponent<Tile>();
                            Tile thisTile = obj.GetComponent<Tile>();
                            if (CanUpgrade(thisTile, thatTile))
                            {
                                UpgradeTile(obj, thisTile, hitObject, thatTile);
                                hasMoved = true;
                            }
                            else
                            {
                                Vector3 newPosition = hitObject.transform.position;
                                newPosition.y += spaceBetweenTiles;
                                if (!Mathf.Approximately(obj.transform.position.y, newPosition.y))
                                {
                                    obj.transform.position = newPosition;
                                    hasMoved = true;
                                }
                            }
                        }
                        else if (hitObject.CompareTag("Border"))
                        {
                            Vector3 newPosition = obj.transform.position;
                            newPosition.y = hit.point.y + halfTileWidth + border.Offset;
                            if (!Mathf.Approximately(obj.transform.position.y, newPosition.y))
                            {
                                obj.transform.position = newPosition;
                                hasMoved = true;
                            }
                        }
                    }
                }
            }
        }

        return hasMoved;
    }

    private bool MoveTilesLeft()
    {
        bool hasMoved = false;
        for (int x = 0; x < matrixTile.Cols; x++)//  3 loop //for (int x = 1; x < matrixTile.Cols; x++)
        {
            for (int y = 0; y < matrixTile.Rows; y++) // 4 loop
            {
                GameObject obj = GetObjectAtGridPosition(x, y);

                if (obj == gameDataContainer.EmptyTile)
                {
                    continue;
                }

                Vector2 raycastOrigin = obj.transform.position;
                raycastOrigin.x -= halfTileWidth;
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, -Vector2.right, Mathf.Infinity);
                if (hit.collider != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != obj)
                    {
                        if (hitObject.CompareTag("Tile"))
                        {
                            Tile thatTile = hitObject.GetComponent<Tile>();
                            Tile thisTile = obj.GetComponent<Tile>();
                            if (CanUpgrade(thisTile, thatTile))
                            {
                                UpgradeTile(obj, thisTile, hitObject, thatTile);
                                hasMoved = true;
                            }
                            else
                            {
                                Vector3 newPosition = hitObject.transform.position;
                                newPosition.x += spaceBetweenTiles;
                                if (!Mathf.Approximately(obj.transform.position.x, newPosition.x))
                                {
                                    obj.transform.position = newPosition;
                                    hasMoved = true;
                                }
                            }
                        }
                        else if (hitObject.CompareTag("Border"))
                        {
                            Vector3 newPosition = obj.transform.position;
                            newPosition.x = hit.point.x + halfTileWidth + border.Offset;
                            if (!Mathf.Approximately(obj.transform.position.x, newPosition.x))
                            {
                                obj.transform.position = newPosition;
                                hasMoved = true;
                            }
                        }
                    }
                }
            }
        }

        return hasMoved;
    }

    private bool MoveTilesRight()
    {
        bool hasMoved = false;
        for (int x = matrixTile.Cols - 1; x >= 0; x--) // 3 loop
        {
            for (int y = 0; y < matrixTile.Rows; y++) // 4 loop
            {
                GameObject obj = GetObjectAtGridPosition(x, y);

                if (obj == gameDataContainer.EmptyTile)
                {
                    continue;
                }

                Vector2 raycastOrigin = obj.transform.position;
                raycastOrigin.x += halfTileWidth;
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.right, Mathf.Infinity);
                if (hit.collider != null)
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject != obj)
                    {
                        if (hitObject.CompareTag("Tile"))
                        {
                            Tile thatTile = hitObject.GetComponent<Tile>();
                            Tile thisTile = obj.GetComponent<Tile>();
                            if (CanUpgrade(thisTile, thatTile))
                            {
                                UpgradeTile(obj, thisTile, hitObject, thatTile);
                                hasMoved = true;
                            }
                            else
                            {
                                Vector3 newPosition = hitObject.transform.position;
                                newPosition.x -= spaceBetweenTiles;
                                if (!Mathf.Approximately(obj.transform.position.x, newPosition.x))
                                {
                                    obj.transform.position = newPosition;
                                    hasMoved = true;
                                }
                            }
                        }
                        else if (hitObject.CompareTag("Border"))
                        {
                            Vector3 newPosition = obj.transform.position;
                            newPosition.x = hit.point.x - halfTileWidth - border.Offset;
                            if (!Mathf.Approximately(obj.transform.position.x, newPosition.x))
                            {
                                obj.transform.position = newPosition;
                                hasMoved = true;
                            }
                        }
                    }
                }
            }
        }

        return hasMoved;
    }

    private bool CanUpgrade(Tile thisTile, Tile thatTile)
    {
        return (thisTile.value != score.MaxScore && thisTile.power == thatTile.power && !thisTile.upgradedThisTurn && !thatTile.upgradedThisTurn);
    }

    private void ReadyTilesForUpgrading()
    {
        foreach (var obj in tiles)
        {
            Tile tile = obj.GetComponent<Tile>();
            tile.upgradedThisTurn = false;
        }
    }

    public void Reset()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        /*
        gameDataContainer.DefeatPopup.popup.SetActive(false);
        foreach (var tile in tiles)
        {
            SimplePool.Despawn(tile);
        }

        tiles.Clear();
        points = 0;
        gameDataContainer.ScoreText.Current.text = "0";
        state = State.Loaded;
        */
    }

    private void UpgradeTile(GameObject toDestroy, Tile destroyTile, GameObject toUpgrade, Tile upgradeTile)
    {
        Vector3 toUpgradePosition = toUpgrade.transform.position;

        tiles.Remove(toDestroy);
        tiles.Remove(toUpgrade);

        SimplePool.Despawn(toDestroy);
        SimplePool.Despawn(toUpgrade);

        // create the upgraded tile
        GameObject newTile = SimplePool.Spawn(gameDataContainer.tilePrefabs[upgradeTile.power], toUpgradePosition, transform.rotation);
        tiles.Add(newTile);
        Tile tile = newTile.GetComponent<Tile>();
        tile.upgradedThisTurn = true;

        points += upgradeTile.value * 2;
        gameDataContainer.ScoreText.Current.text = points.ToString();

        TileAnimationHandler tileAnim = newTile.GetComponent<TileAnimationHandler>();
        tileAnim.AnimateUpgrade();
    }
}

public sealed class MoverHandler
{

}