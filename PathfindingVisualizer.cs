using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AlgorithmVisualizer
{
    public class PathfindingVisualizer : IAlgorithmVisualizer
    {
        private Panel panel;
        private int gridSize = 20; // Number of cells in each dimension
        private int cellSize; // Size of each cell in pixels
        private CellState[,] grid;
        private Point startPoint;
        private Point endPoint;
        private Random random = new Random();
        
        private Queue<Point> queue = new Queue<Point>();
        private HashSet<Point> visited = new HashSet<Point>();
        private Dictionary<Point, Point> parent = new Dictionary<Point, Point>();
        private bool pathFound = false;
        private bool algorithmCompleted = false;

        public int Steps { get; private set; } = 0;

        public PathfindingVisualizer(Panel panel)
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
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = CellState.Empty;
                }
            }

            // Place random obstacles (0.3 is 30% of the cells)
            for (int i = 0; i < gridSize * gridSize * 0.3; i++)
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
            // Reset
            queue.Clear();
            visited.Clear();
            parent.Clear();
            pathFound = false;
            algorithmCompleted = false;
            Steps = 0;

            // Initialize
            queue.Enqueue(startPoint);
            visited.Add(startPoint);

            // Reset cells visualization.
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (grid[x, y] == CellState.Visited || grid[x, y] == CellState.Path || grid[x, y] == CellState.Current)
                    {
                        grid[x, y] = CellState.Empty;
                    }
                }
            }

            grid[startPoint.X, startPoint.Y] = CellState.Start;
            grid[endPoint.X, endPoint.Y] = CellState.End;

            DrawGrid();
        }

        public bool NextStep()
        {
            if (algorithmCompleted)
                return true;

            Steps++; // Increment the step counter

            if (pathFound)
            {
                // Reconstruct and visualize the path (visualization)
                List<Point> path = new List<Point>();
                Point current = endPoint;

                while (parent.ContainsKey(current) && !current.Equals(startPoint))
                {
                    path.Add(current);
                    current = parent[current];
                }

                // Mark path cells (visualization)
                foreach (Point p in path)
                {
                    if (!p.Equals(endPoint)) // Don't overwrite the end point (keep that little red sqaure to see the end)
                        grid[p.X, p.Y] = CellState.Path;
                }

                algorithmCompleted = true;
                DrawGrid();
                return true;
            }

            if (queue.Count == 0)
            {
                // No path found (If you increase the walls, this will fail here)
                algorithmCompleted = true;
                DrawGrid();
                return true;
            }

            Point currentPoint = queue.Dequeue();

            // Skip if it's the start point
            if (!currentPoint.Equals(startPoint))
                grid[currentPoint.X, currentPoint.Y] = CellState.Current;

            // Did we reach the end?
            if (currentPoint.Equals(endPoint))
            {
                pathFound = true;
                DrawGrid();
                return false; // The visualization is next after this..
            }

            // Explore neighbors (4-direction: up, right, down, left)
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nx = currentPoint.X + dx[i];
                int ny = currentPoint.Y + dy[i];

                if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize)
                {
                    Point neighbor = new Point(nx, ny);

                    if (!visited.Contains(neighbor) &&
                        (grid[nx, ny] == CellState.Empty || grid[nx, ny] == CellState.End))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                        parent[neighbor] = currentPoint;

                        if (grid[nx, ny] != CellState.End) // Don't overwrite the end point, we want to see that.
                            grid[nx, ny] = CellState.Visited;
                    }
                }
            }

            DrawGrid();
            return false;
        }

        private void DrawGrid()
        {
            // Create a bitmap for double buffering, this is why it looks nice, but also a bit odd (blurry?) for winforms.
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
                                cellColor = Color.FromArgb(52, 73, 94); // Blue
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
                }
            }

            // Draw the final image to the panel, completed!
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
}