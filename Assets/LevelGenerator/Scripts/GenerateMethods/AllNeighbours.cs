using Connect.Generator;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditorInternal;
using UnityEngine;

namespace Connect.Generator.AllNeighbours
{
    public class AllNeighbours : MonoBehaviour, GenerateMethod
    {
        [SerializeField] private TMP_Text _timeText, _gridCountText;
        [SerializeField] private bool _showOnlyResult;
        private List<GridData> checkingGrid;
        private LevelGenerator Instance;
        private bool isCreating;

        [SerializeField] private float speedMultiplier;
        [SerializeField] private float speed;

        private void Start()
        {
            checkingGrid = new List<GridData>();
            Instance = GetComponent<LevelGenerator>();
            isCreating = true;
        }

        public void Generate()
        {
            StartCoroutine(GeneratePaths());
        }

        private IEnumerator GeneratePaths()
        {
            for (int i = 0; i < Instance.levelSize; i++)
            {
                for (int j = 0; j < Instance.levelSize; j++)
                {
                    GridData tempGrid = new GridData(i, j, Instance.levelSize);
                    checkingGrid.Add(tempGrid);
                }
            }
            yield return new WaitForSeconds(speed);

            StartCoroutine(SolvePaths());

            int count = 0;

            while (isCreating)
            {
                if (count == checkingGrid.Count)
                {
                    yield break;
                }

                while (_showOnlyResult && !checkingGrid[count].IsGridComplete())
                {
                    count++;

                    if (count == checkingGrid.Count)
                    {
                        yield break;
                    }
                }

                Instance.RenderGrid(checkingGrid[count]._grid);
                count++;

                _timeText.text = count.ToString();
                _gridCountText.text = checkingGrid.Count.ToString();

                yield return new WaitForSeconds(speed);
            }
        }

        private List<Point> directionChecks = new List<Point>()
         { Point.up, Point.down, Point.left, Point.right};

        private IEnumerator SolvePaths()
        {
            bool canSolve = true;
            int iterationPerFrame = (int)(speedMultiplier / speed);
            int currentIteration = 0;

            while (canSolve)
            {
                List<GridData> resultGridData = new List<GridData>();

                foreach (var item in checkingGrid)
                {
                    if (!item.IsSolved)
                    {
                        resultGridData.Add(item);
                    }
                }

                if (resultGridData.Count == 0)
                {
                    canSolve = false;
                    yield break;
                }

                foreach (var item in resultGridData)
                {
                    int posIndex = checkingGrid.IndexOf(item);
                    int insertIndex = 1;

                    foreach (var direction in directionChecks)
                    {
                        Point checkingDirection = item.CurrentPos + direction;

                        if (item.IsInsideGrid(checkingDirection)
                            && item._grid[checkingDirection] == -1
                            && item.IsNotNeighbours(checkingDirection))
                        {
                            checkingGrid.Insert(posIndex + insertIndex,
                                new GridData(checkingDirection.x, checkingDirection.y, item.ColorId, item)
                                );
                            insertIndex++;
                        }
                    }

                    foreach (var emptyPos in item.EmptyPosition())
                    {
                        if (item.FlowLength() > 2)
                        {
                            checkingGrid.Insert(posIndex + insertIndex,
                                new GridData(emptyPos.x, emptyPos.y, item.ColorId + 1, item)
                                );
                            insertIndex++;
                        }
                    }

                    item.IsSolved = true;

                    currentIteration++;

                    if (currentIteration > iterationPerFrame)
                    {
                        currentIteration = 0;
                        yield return new WaitForSeconds(speed);

                    }
                }
            }
        }
    }

    public class GridData
    {
        private static List<Point> directionChecks = new List<Point>()
          { Point.up, Point.down, Point.left, Point.right};

        public Dictionary<Point, int> _grid;
        public bool IsSolved;
        public Point CurrentPos;
        public int ColorId;

        public GridData(int i, int j, int levelSize)
        {
            _grid = new Dictionary<Point, int>();

            for (int a = 0; a < levelSize; a++)
            {
                for (int b = 0; b < levelSize; b++)
                {
                    _grid[new Point(a, b)] = -1;
                }
            }
            IsSolved = false;
            CurrentPos = new Point(i, j);
            ColorId = 0;
            _grid[CurrentPos] = ColorId;
        }

        public GridData(int i, int j, int passedColor, GridData gridCopy)
        {
            _grid = new Dictionary<Point, int>();

            foreach (var item in gridCopy._grid)
            {
                _grid[item.Key] = item.Value;
            }

            CurrentPos = new Point(i, j);
            ColorId = passedColor;
            _grid[CurrentPos] = ColorId;
            IsSolved = false;
        }

        public bool IsInsideGrid(Point pos)
        {
            return _grid.ContainsKey(pos);
        }

        public bool IsGridComplete()
        {
            foreach (var item in _grid)
            {
                if (item.Value == -1)
                    return false;
            }

            for (int i = 0; i <= ColorId; i++)
            {
                int result = 0;

                foreach (var item in _grid)
                {
                    if (item.Value == i)
                        result++;
                }

                if (result < 3)
                    return false;
            }

            return true;
        }

        public bool IsNotNeighbours(Point pos)
        {
            foreach (var item in _grid)
            {
                if (item.Value == ColorId && item.Key != CurrentPos)

                {
                    foreach (var direction in directionChecks)
                    {
                        if (pos - item.Key == direction)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public List<Point> EmptyPosition()
        {
            List<Point> result = new List<Point>();

            foreach (var item in _grid)
            {
                if (item.Value == -1)
                    result.Add(item.Key);
            }

            return result;
        }

        public int FlowLength()
        {
            int result = 0;

            foreach (var item in _grid)
            {
                if (item.Value == ColorId)
                    result++;
            }

            return result;
        }
    }
}
