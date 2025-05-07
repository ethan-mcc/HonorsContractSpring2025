using System.Drawing.Drawing2D;

namespace AlgorithmVisualizer
{
    public enum SortAlgorithm
    {
        BubbleSort,
        InsertionSort
    }

    public class SortingVisualizer : IAlgorithmVisualizer
    {
        private Panel panel;
        private int[] array;
        private int[] originalArray;
        private SortAlgorithm currentAlgorithm;
        private Random random = new Random();

        // Bubble sort state
        private int bubbleSortI = 0;
        private int bubbleSortJ = 0;
        private bool bubbleSortSwapped = false;

        // Insertion sort state
        private int insertionSortI = 1;
        private int insertionSortJ = 0;
        private int insertionSortKey = 0;
        private bool insertionSortMoving = false;

        public int Steps { get; private set; } = 0;

        public SortingVisualizer(Panel panel)
        {
            this.panel = panel;
            // Double buffering to prevent flicking, winforms single threaded..
            // So rendering between each step flickers, but I also have it slowed way down each step. 
            // Basically I'm trying to keep the previous step in the panel, a bit map actually, until the next step completed compute wise.
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, panel, new object[] { true });
            
            GenerateRandomArray(30);
        }

        public void GenerateRandomArray(int size)
        {
            array = new int[size];
            originalArray = new int[size];
            
            for (int i = 0; i < size; i++)
            {
                array[i] = random.Next(10, 400);
                originalArray[i] = array[i];
            }

            Reset();
            DrawArray();
        }

        public void SetAlgorithm(SortAlgorithm algorithm)
        {
            currentAlgorithm = algorithm;
            Reset();
        }

        public void Reset()
        {
            if (originalArray != null)
            {
                Array.Copy(originalArray, array, originalArray.Length);
            }

            bubbleSortI = 0;
            bubbleSortJ = 0;
            bubbleSortSwapped = false;

            insertionSortI = 1;
            insertionSortJ = 0;
            insertionSortKey = 0;
            insertionSortMoving = false;

            Steps = 0;
            DrawArray();
        }

        public bool NextStep()
        {
            Steps++;
            bool completed = false;

            switch (currentAlgorithm)
            {
                case SortAlgorithm.BubbleSort:
                    completed = BubbleSortStep();
                    break;
                case SortAlgorithm.InsertionSort:
                    completed = InsertionSortStep();
                    break;
            }

            DrawArray();
            return completed;
        }

        private bool BubbleSortStep()
        {
            if (bubbleSortI >= array.Length - 1)
            {
                return true; // This means it's completed the Algorithm
            }

            if (bubbleSortJ >= array.Length - bubbleSortI - 1)
            {
                bubbleSortJ = 0;
                bubbleSortI++;
                
                if (bubbleSortI >= array.Length - 1)
                {
                    return true; // This means it's completed the Algorithm
                }
            }

            // Compare adjacent elements
            if (array[bubbleSortJ] > array[bubbleSortJ + 1])
            {
                // Swap
                int temp = array[bubbleSortJ];
                array[bubbleSortJ] = array[bubbleSortJ + 1];
                array[bubbleSortJ + 1] = temp;
                bubbleSortSwapped = true;
            }
            
            bubbleSortJ++;
            return false; // Not completed keep going 
        }

        private bool InsertionSortStep()
        {
            if (insertionSortI >= array.Length)
            {
                return true; // This means it's completed the Algorithm
            }

            // First step of the iteration, we want to save the key
            if (!insertionSortMoving)
            {
                insertionSortKey = array[insertionSortI];
                insertionSortJ = insertionSortI - 1;
                insertionSortMoving = true;
                return false;
            }

            // Move elements that are greater than key to one position ahead
            if (insertionSortJ >= 0 && array[insertionSortJ] > insertionSortKey)
            {
                array[insertionSortJ + 1] = array[insertionSortJ];
                insertionSortJ--;
                return false;
            }

            // Place the key in its correct position
            array[insertionSortJ + 1] = insertionSortKey;
            insertionSortI++;
            insertionSortMoving = false;
            
            return insertionSortI >= array.Length; // Completed!
        }

        private void DrawArray()
        {
            if (array == null || array.Length == 0)
                return;

            // Create a bitmap for double buffering, this is why it looks nice, but also a bit odd (blurry?) for winforms.
            Bitmap bmp = new Bitmap(panel.Width, panel.Height);
            
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(panel.BackColor);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                int barWidth = (panel.Width - 20) / array.Length;
                
                for (int i = 0; i < array.Length; i++)
                {
                    int barHeight = array[i];
                    int x = 10 + i * barWidth;
                    int y = panel.Height - barHeight - 10;

                    Color barColor = Color.FromArgb(52, 152, 219); // Blue

                    // Highlight elements (based on current algo)
                    switch (currentAlgorithm)
                    {
                        case SortAlgorithm.BubbleSort:
                            if (i == bubbleSortJ)
                                barColor = Color.FromArgb(231, 76, 60); // Red
                            else if (i == bubbleSortJ + 1)
                                barColor = Color.FromArgb(46, 204, 113); // Green
                            break;
                        
                        case SortAlgorithm.InsertionSort:
                            if (i == insertionSortI)
                                barColor = Color.FromArgb(231, 76, 60); // Red
                            else if (insertionSortMoving && i == insertionSortJ)
                                barColor = Color.FromArgb(230, 126, 34); // Orange
                            else if (i < insertionSortI)
                                barColor = Color.FromArgb(46, 204, 113); // Green
                            break;
                    }

                    using (SolidBrush brush = new SolidBrush(barColor))
                    {
                        g.FillRoundedRectangle(brush, x, y, barWidth - 2, barHeight, 3);
                    }
                }

                // Add information text at the bottom
                using (Font font = new Font("Segoe UI", 8))
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                {
                    // Display colors legend
                    g.DrawString("Current", font, textBrush, 10, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(231, 76, 60)), 55, 5, 15, 15, 3);
                    
                    g.DrawString("Next", font, textBrush, 85, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(46, 204, 113)), 115, 5, 15, 15, 3);
                    
                    g.DrawString("Sorted", font, textBrush, 145, 5);
                    g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(52, 152, 219)), 185, 5, 15, 15, 3);
                    
                    if (currentAlgorithm == SortAlgorithm.InsertionSort)
                    {
                        g.DrawString("Key", font, textBrush, 215, 5);
                        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(230, 126, 34)), 240, 5, 15, 15, 3);
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
    }

    // Helper extension methods for Graphics class, this makes it look nice and rounded.
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, float x, float y, float width, float height, float radius)
        {
            var rectangle = new RectangleF(x, y, width, height);
            var path = GetRoundedRect(rectangle, radius);
            graphics.FillPath(brush, path);
        }

        private static GraphicsPath GetRoundedRect(RectangleF baseRect, float radius)
        {
            if (radius <= 0)
            {
                var path2 = new GraphicsPath();
                path2.AddRectangle(baseRect);
                return path2;
            }

            // Limit radius to half the size of the smaller dimension
            radius = Math.Min(radius, Math.Min(baseRect.Width, baseRect.Height) / 2);
            
            var diameter = radius * 2;
            var sizeRect = new SizeF(diameter, diameter);
            var arc = new RectangleF(baseRect.Location, sizeRect);
            var path = new GraphicsPath();

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = baseRect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = baseRect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = baseRect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}