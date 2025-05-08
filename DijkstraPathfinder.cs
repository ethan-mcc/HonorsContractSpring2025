using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;

namespace AlgorithmVisualizer
{
    public class DijkstraPathfinder : IAlgorithmVisualizer
    {
        private Panel panel;
        private int gridSize = 20; // Number of cells in each dimension
        private int cellSize; // Size of each cell in pixels
        private CellState[,] grid;
        private Point startPoint;
        private Point endPoint;
        private Random random = new Random();
        
        // Dijkstra's specific data structures
        private PriorityQueue<Point, int> queue = new PriorityQueue<Point, int>();
        private Dictionary<Point, int> distance = new Dictionary<Point, int>();
        private HashSet<Point> visited = new HashSet<Point>();
        private Dictionary<Point, Point> parent = new Dictionary<Point, Point>();
        private Point current;
        private bool pathFound = false;
        private bool algorithmCompleted = false;

        // For visualizing weights
        private int[,] weights;

        public int Steps { get; private set; } = 0;
        public long NanosecondsTaken { get; private set; } = 0;
        public long CpuCyclesTaken { get; private set; } = 0;
        public long MemoryUsed { get; private set; } = 0;

        public DijkstraPathfinder(Panel panel)
        {
            this.panel = panel;
            cellSize = Math.Min((panel.Width - 40) / gridSize, (panel.Height - 40) / gridSize);
            GenerateRandomGrid();
        }

        public void GenerateRandomGrid()
        {
            grid = new CellState[gridSize, gridSize];
            weights = new int[gridSize, gridSize];
            
            // Initialize grid with empty cells and random weights
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = CellState.Empty;
                    weights[x, y] = random.Next(1, 10); // Random weights from 1 to 9
                }
            }

            // Place random obstacles (20% of cells)
            for (int i = 0; i < gridSize * gridSize * 0.2; i++)
            {
                int x = random.Next(gridSize);
                int y = random.Next(gridSize);
                grid[x, y] = CellState.Wall;
            }

            // Set start and end points (random)
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
            // Reset data structures
            queue = new PriorityQueue<Point, int>();
            distance.Clear();
            visited.Clear();
            parent.Clear();
            pathFound = false;
            algorithmCompleted = false;
            Steps = 0;
            NanosecondsTaken = 0;
            CpuCyclesTaken = 0;
            MemoryUsed = 0;

            // Reset cells visualization
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (grid[x, y] == CellState.Visited || grid[x, y] == CellState.Path || 
                        grid[x, y] == CellState.Current)
                    {
                        grid[x, y] = CellState.Empty;
                    }
                }
            }

            grid[startPoint.X, startPoint.Y] = CellState.Start;
            grid[endPoint.X, endPoint.Y] = CellState.End;

            // Initialize Dijkstra's algorithm
            // Set all distances to infinity except start
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Point p = new Point(x, y);
                    distance[p] = int.MaxValue;
                }
            }
            
            distance[startPoint] = 0;
            queue.Enqueue(startPoint, 0);

            DrawGrid();
        }

        public bool NextStep()
        {
            if (algorithmCompleted)
                return true;

            Steps++; // Increment the step counter

            long memoryBefore = GC.GetTotalMemory(false);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            bool result;
            if (pathFound)
            {
                // Reconstruct and visualize the path
                List<Point> path = new List<Point>();
                Point current = endPoint;

                while (parent.ContainsKey(current) && !current.Equals(startPoint))
                {
                    path.Add(current);
                    current = parent[current];
                }

                // Mark path cells
                foreach (Point p in path)
                {
                    if (!p.Equals(endPoint)) // Don't overwrite the end point
                        grid[p.X, p.Y] = CellState.Path;
                }

                algorithmCompleted = true;
                result = true;
            }
            else if (queue.Count == 0)
            {
                // No path found
                algorithmCompleted = true;
                result = true;
            }
            else
            {
                // Dequeue point with smallest distance
                if (queue.TryDequeue(out current, out int _))
                {
                    // If we've already visited this node, skip
                    if (visited.Contains(current))
                    {
                        result = false;
                        return result;
                    }

                    // Mark as visited
                    visited.Add(current);

                    // Skip if it's the start point (don't change its visualization)
                    if (!current.Equals(startPoint))
                        grid[current.X, current.Y] = CellState.Current;

                    // Did we reach the end?
                    if (current.Equals(endPoint))
                    {
                        pathFound = true;
                        result = false; // Continue to visualize the path
                    }
                    else
                    {
                        // Explore neighbors (4-direction: up, right, down, left)
                        int[] dx = { 0, 1, 0, -1 };
                        int[] dy = { -1, 0, 1, 0 };

                        for (int i = 0; i < 4; i++)
                        {
                            int nx = current.X + dx[i];
                            int ny = current.Y + dy[i];

                            if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize)
                            {
                                Point neighbor = new Point(nx, ny);

                                // Skip walls or already visited nodes
                                if (grid[nx, ny] == CellState.Wall || visited.Contains(neighbor))
                                    continue;

                                // Calculate new distance
                                int alt = distance[current] + weights[nx, ny];
                                
                                // If we found a better path
                                if (alt < distance[neighbor])
                                {
                                    distance[neighbor] = alt;
                                    parent[neighbor] = current;
                                    queue.Enqueue(neighbor, alt);

                                    // Mark as visited for visualization
                                    if (grid[nx, ny] != CellState.End && grid[nx, ny] != CellState.Start)
                                        grid[nx, ny] = CellState.Visited;
                                }
                            }
                        }
                        result = false;
                    }
                }
                else
                {
                    // Should never happen but just in case
                    result = true;
                }
            }

            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);
            
            // Update performance metrics
            NanosecondsTaken += stopwatch.ElapsedTicks * (1000000000L / Stopwatch.Frequency);
            CpuCyclesTaken += stopwatch.ElapsedTicks;
            MemoryUsed += Math.Max(0, memoryAfter - memoryBefore);

            DrawGrid();
            return result;
        }

        private void DrawGrid()
        {
            // Create a bitmap for double buffering
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
                            case CellState.Visited:
                                cellColor = Color.FromArgb(189, 195, 199); // Light gray
                                break;
                            case CellState.Current:
                                cellColor = Color.FromArgb(52, 152, 219); // Blue
                                break;
                            case CellState.Path:
                                cellColor = Color.FromArgb(241, 196, 15); // Yellow
                                break;
                            default:
                                cellColor = Color.White;
                                break;
                        }

                        // Draw rounded rect cells
                        using (SolidBrush brush = new SolidBrush(cellColor))
                        {
                            g.FillRoundedRectangle(brush, drawX, drawY, cellSize - 4, cellSize - 4, 4);
                        }

                        // Draw weight value for empty, visited, and path cells
                        if (grid[x, y] != CellState.Wall && grid[x, y] != CellState.Start && 
                            grid[x, y] != CellState.End)
                        {
                            using (Font font = new Font("Segoe UI", 7f))
                            using (SolidBrush textBrush = new SolidBrush(Color.Black))
                            {
                                g.DrawString(weights[x, y].ToString(), font, textBrush, 
                                    drawX + cellSize / 2 - 5, drawY + cellSize / 2 - 6);
                            }
                        }

                        // Show distance label for visited cells
                        if ((grid[x, y] == CellState.Visited || grid[x, y] == CellState.Current || 
                             grid[x, y] == CellState.Path) && distance.ContainsKey(new Point(x, y)) && 
                             distance[new Point(x, y)] != int.MaxValue)
                        {
                            using (Font font = new Font("Segoe UI", 6f))
                            using (SolidBrush textBrush = new SolidBrush(Color.DarkBlue))
                            {
                                g.DrawString(distance[new Point(x, y)].ToString(), font, textBrush, 
                                    drawX + 2, drawY + 2);
                            }
                        }
                    }
                }

                // Add grid labels for clarity
                using (Font font = new Font("Segoe UI", 8f))
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                {
                    g.DrawString("Start", font, textBrush, 20, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(46, 204, 113)), 60, 5, 15, 15, 4);

                    g.DrawString("End", font, textBrush, 100, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(231, 76, 60)), 130, 5, 15, 15, 4);

                    g.DrawString("Wall", font, textBrush, 170, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(52, 73, 94)), 200, 5, 15, 15, 4);

                    g.DrawString("Path", font, textBrush, 240, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(241, 196, 15)), 270, 5, 15, 15, 4);

                    g.DrawString("Visited", font, textBrush, 310, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(189, 195, 199)), 350, 5, 15, 15, 4);

                    g.DrawString("Current", font, textBrush, 400, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(52, 152, 219)), 450, 5, 15, 15, 4);
                }

                // Performance metrics display
                if (NanosecondsTaken > 0)
                {
                    using (Font font = new Font("Segoe UI", 8f, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                    {
                        string metrics = $"Steps: {Steps} | Time: {NanosecondsTaken / 1000000.0:F2}ms | Memory: {MemoryUsed / 1024.0:F2}KB";
                        g.DrawString(metrics, font, textBrush, 20, panel.Height - 25);
                    }
                }
            }

            // Draw the final image to the panel
            using (Graphics panelGraphics = panel.CreateGraphics())
            {
                panelGraphics.DrawImage(bmp, 0, 0);
            }

            bmp.Dispose();
        }

        private enum CellState
        {
            Empty,
            Wall,
            Start,
            End,
            Visited,
            Current,
            Path
        }
    }

    // PriorityQueue polyfill 
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _elements = new List<(TElement, TPriority)>();

        public int Count => _elements.Count;

        public void Enqueue(TElement element, TPriority priority)
        {
            _elements.Add((element, priority));
            _elements.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (_elements.Count > 0)
            {
                element = _elements[0].Element;
                priority = _elements[0].Priority;
                _elements.RemoveAt(0);
                return true;
            }

            element = default;
            priority = default;
            return false;
        }
    }
} 