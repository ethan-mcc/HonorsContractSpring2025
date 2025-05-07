using AlgorithmVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonorsContractSpring2025
{
    public class AStarPathfinder : IAlgorithmVisualizer
    {
        private readonly Panel panel;
        private int gridSize = 20;
        private int cellSize;
        private CellState[,] grid;
        private Point startPoint;
        private Point endPoint;
        private Random random = new Random();

        // A* specific data structures
        private List<PathNode> openSet = new List<PathNode>();
        private HashSet<Point> closedSet = new HashSet<Point>();
        private Dictionary<Point, PathNode> nodeMap = new Dictionary<Point, PathNode>();
        private Dictionary<Point, Point> parent = new Dictionary<Point, Point>();
        private PathNode current = null;
        private bool pathFound = false;
        private bool algorithmCompleted = false;

        public int Steps { get; private set; } = 0;

        // Performance metrics
        public long NanosecondsTaken { get; private set; } = 0;
        public long CpuCyclesTaken { get; private set; } = 0;
        public long MemoryUsed { get; private set; } = 0;
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private long startMemory;

        public AStarPathfinder(Panel panel)
        {
            this.panel = panel;
            cellSize = Math.Min((panel.Width - 40) / gridSize, (panel.Height - 40) / gridSize);
            GenerateRandomGrid();
        }

        public void GenerateRandomGrid()
        {
            grid = new CellState[gridSize, gridSize];

            // Initialize grid with empty cells
            for (int x = 0; x < gridSize; x++)
                for (int y = 0; y < gridSize; y++)
                    grid[x, y] = CellState.Empty;

            // Place random obstacles (30% of cells)
            for (int i = 0; i < gridSize * gridSize * 0.3; i++)
            {
                int x = random.Next(gridSize);
                int y = random.Next(gridSize);
                grid[x, y] = CellState.Wall;
            }

            // Set start and end points
            do
            {
                startPoint.X = random.Next(gridSize);
                startPoint.Y = random.Next(gridSize);
            } while (grid[startPoint.X, startPoint.Y] == CellState.Wall);

            do
            {
                endPoint.X = random.Next(gridSize);
                endPoint.Y = random.Next(gridSize);
            } while (grid[endPoint.X, endPoint.Y] == CellState.Wall ||
                    (endPoint.X == startPoint.X && endPoint.Y == startPoint.Y));

            grid[startPoint.X, startPoint.Y] = CellState.Start;
            grid[endPoint.X, endPoint.Y] = CellState.End;

            Reset();
            DrawGrid();
        }

        public void Reset()
        {
            // Clear data structures
            openSet.Clear();
            closedSet.Clear();
            nodeMap.Clear();
            parent.Clear();

            pathFound = false;
            algorithmCompleted = false;
            Steps = 0;
            NanosecondsTaken = 0;
            CpuCyclesTaken = 0;
            MemoryUsed = 0;

            // Reset grid visualization
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (grid[x, y] == CellState.Visited || grid[x, y] == CellState.OpenSet ||
                        grid[x, y] == CellState.Current || grid[x, y] == CellState.Path)
                    {
                        grid[x, y] = CellState.Empty;
                    }
                }
            }

            // Add start node to open set
            var startNode = new PathNode(startPoint, 0, ManhattanDistance(startPoint, endPoint));
            openSet.Add(startNode);
            nodeMap[startPoint] = startNode;
            grid[startPoint.X, startPoint.Y] = CellState.Start;
            grid[endPoint.X, endPoint.Y] = CellState.End;

            // Start performance tracking
            startMemory = GC.GetTotalMemory(false);
            stopwatch.Restart();

            DrawGrid();
        }

        public bool NextStep()
        {
            if (algorithmCompleted)
                return true;

            Steps++;

            if (pathFound)
            {
                // Reconstruct path
                List<Point> path = new List<Point>();
                Point current = endPoint;

                while (parent.ContainsKey(current) && !current.Equals(startPoint))
                {
                    path.Add(current);
                    current = parent[current];
                }

                // Visualize path
                foreach (Point p in path)
                {
                    if (!p.Equals(endPoint))
                        grid[p.X, p.Y] = CellState.Path;
                }

                algorithmCompleted = true;

                // Capture performance metrics
                stopwatch.Stop();
                NanosecondsTaken = stopwatch.ElapsedTicks * 1000000000 / System.Diagnostics.Stopwatch.Frequency;
                CpuCyclesTaken = stopwatch.ElapsedTicks;
                MemoryUsed = GC.GetTotalMemory(false) - startMemory;

                DrawGrid();
                return true;
            }

            if (openSet.Count == 0)
            {
                // No path found
                algorithmCompleted = true;

                // Capture performance metrics
                stopwatch.Stop();
                NanosecondsTaken = stopwatch.ElapsedTicks * 1000000000 / System.Diagnostics.Stopwatch.Frequency;
                CpuCyclesTaken = stopwatch.ElapsedTicks;
                MemoryUsed = GC.GetTotalMemory(false) - startMemory;

                DrawGrid();
                return true;
            }

            // Find the node with the lowest f_score
            current = openSet.OrderBy(node => node.FScore).First();

            // Check if we reached the goal
            if (current.Position.Equals(endPoint))
            {
                pathFound = true;
                DrawGrid();
                return false;
            }

            // Move current from open to closed set
            openSet.Remove(current);
            closedSet.Add(current.Position);

            // Visualize current node
            if (!current.Position.Equals(startPoint))
                grid[current.Position.X, current.Position.Y] = CellState.Current;

            // Check neighbors
            int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };  // Include diagonals
            int[] dy = { -1, 0, 1, 0, -1, 1, 1, -1 };  // Include diagonals

            for (int i = 0; i < 8; i++)  // Check all 8 directions
            {
                int nx = current.Position.X + dx[i];
                int ny = current.Position.Y + dy[i];
                Point neighborPos = new Point(nx, ny);

                // Skip invalid or closed cells
                if (nx < 0 || nx >= gridSize || ny < 0 || ny >= gridSize ||
                    closedSet.Contains(neighborPos) || grid[nx, ny] == CellState.Wall)
                    continue;

                // Calculate g_score (using 14 for diagonal, 10 for cardinal directions)
                int moveCost = (i < 4) ? 10 : 14;  // 10 for cardinal, 14 for diagonal (approx sqrt(2)*10)
                int tentativeGScore = current.GScore + moveCost;

                // Get or create neighbor node
                if (!nodeMap.TryGetValue(neighborPos, out PathNode neighbor))
                {
                    neighbor = new PathNode(
                        neighborPos,
                        int.MaxValue,
                        ManhattanDistance(neighborPos, endPoint)
                    );
                    nodeMap[neighborPos] = neighbor;
                }

                // Skip if not a better path
                if (tentativeGScore >= neighbor.GScore)
                    continue;

                // Update with better path
                parent[neighborPos] = current.Position;
                neighbor.GScore = tentativeGScore;
                neighbor.FScore = neighbor.GScore + neighbor.HScore;

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);

                    // Visualize
                    if (grid[nx, ny] != CellState.End && grid[nx, ny] != CellState.Start)
                        grid[nx, ny] = CellState.OpenSet;
                }
            }

            DrawGrid();
            return false;
        }

        private int ManhattanDistance(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private void DrawGrid()
        {
            Bitmap bmp = new Bitmap(panel.Width, panel.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(panel.BackColor);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw each cell
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        int drawX = 20 + x * cellSize;
                        int drawY = 20 + y * cellSize;

                        Color cellColor;
                        switch (grid[x, y])
                        {
                            case CellState.Empty:
                                cellColor = Color.White;
                                break;
                            case CellState.Wall:
                                cellColor = Color.FromArgb(52, 73, 94); // Dark blue
                                break;
                            case CellState.Start:
                                cellColor = Color.FromArgb(46, 204, 113); // Green
                                break;
                            case CellState.End:
                                cellColor = Color.FromArgb(231, 76, 60); // Red
                                break;
                            case CellState.OpenSet:
                                cellColor = Color.FromArgb(241, 196, 15); // Yellow
                                break;
                            case CellState.Current:
                                cellColor = Color.FromArgb(230, 126, 34); // Orange
                                break;
                            case CellState.Visited:
                                cellColor = Color.FromArgb(189, 195, 199); // Light gray
                                break;
                            case CellState.Path:
                                cellColor = Color.FromArgb(155, 89, 182); // Purple
                                break;
                            default:
                                cellColor = Color.White;
                                break;
                        }

                        using (SolidBrush brush = new SolidBrush(cellColor))
                        {
                            g.FillRoundedRectangle(brush, drawX, drawY, cellSize - 4, cellSize - 4, 4);
                        }

                        // Display F, G, H scores for A* visualization
                        if (nodeMap.TryGetValue(new Point(x, y), out PathNode node) &&
                            grid[x, y] != CellState.Start && grid[x, y] != CellState.End &&
                            grid[x, y] != CellState.Wall)
                        {
                            using (Font font = new Font("Segoe UI", 6f))
                            using (SolidBrush textBrush = new SolidBrush(Color.Black))
                            {
                                string fScore = node.FScore.ToString();
                                string gScore = node.GScore.ToString();

                                g.DrawString(fScore, font, textBrush,
                                    drawX + cellSize / 2 - 6, drawY + cellSize / 2 - 8);
                                g.DrawString(gScore, font, textBrush,
                                    drawX + 2, drawY + 2);
                            }
                        }
                    }
                }

                // Add legend
                using (Font font = new Font("Segoe UI", 8f))
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                {
                    g.DrawString("Start", font, textBrush, 20, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(46, 204, 113)), 60, 5, 15, 15, 4);

                    g.DrawString("End", font, textBrush, 90, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(231, 76, 60)), 120, 5, 15, 15, 4);

                    g.DrawString("Wall", font, textBrush, 150, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(52, 73, 94)), 180, 5, 15, 15, 4);

                    g.DrawString("Open", font, textBrush, 210, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(241, 196, 15)), 245, 5, 15, 15, 4);

                    g.DrawString("Current", font, textBrush, 275, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(230, 126, 34)), 325, 5, 15, 15, 4);

                    g.DrawString("Path", font, textBrush, 355, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(155, 89, 182)), 385, 5, 15, 15, 4);
                }

                // Display performance metrics when available
                if (algorithmCompleted)
                {
                    using (Font font = new Font("Segoe UI", 8f, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                    {
                        string metrics = $"Time: {NanosecondsTaken:N0} ns | CPU Cycles: {CpuCyclesTaken:N0} | Memory: {MemoryUsed:N0} bytes";
                        g.DrawString(metrics, font, textBrush, panel.Width - 350, panel.Height - 25);
                    }
                }
            }

            using (Graphics panelGraphics = panel.CreateGraphics())
            {
                panelGraphics.DrawImage(bmp, 0, 0);
            }

            bmp.Dispose();
        }

        // Additional enum values for A*
        private enum CellState
        {
            Empty,
            Wall,
            Start,
            End,
            OpenSet,   // A* open set
            Current,   // Current node being processed
            Visited,   // Already processed (closed set)
            Path       // Final path
        }

        // Helper class for A* algorithm
        private class PathNode
        {
            public Point Position { get; }
            public int GScore { get; set; } // Cost from start
            public int HScore { get; } // Heuristic to goal
            public int FScore { get => GScore + HScore; } // Total estimated cost

            public PathNode(Point position, int gScore, int hScore)
            {
                Position = position;
                GScore = gScore;
                HScore = hScore;
            }
        }
    }
}
