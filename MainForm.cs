using AlgorithmVisualizer;
using Timer = System.Windows.Forms.Timer;

namespace HonorsContractSpring2025
{
    public partial class MainForm : Form
    {
        private Timer animationTimer;
        private TrackBar speedSlider;
        private ComboBox algorithmSelector;
        private Button playButton;
        private Button pauseButton;
        private Button resetButton;
        private Button generateButton;
        private NumericUpDown arraySizeInput;
        private Panel visualizationPanel;
        private Label infoLabel;
        private Label complexityLabel;
        private Label stepsLabel;

        private SortingVisualizer sortingVisualizer;
        private PathfindingVisualizer pathfindingVisualizer;
        private AStarPathfinder aStarPathfinder;
        private DijkstraPathfinder dijkstraPathfinder;
        private IAlgorithmVisualizer currentVisualizer;

        private bool isPlaying = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            SetupEventHandlers();

            // Initialize visualizers
            sortingVisualizer = new SortingVisualizer(visualizationPanel);
            pathfindingVisualizer = new PathfindingVisualizer(visualizationPanel);
            aStarPathfinder = new AStarPathfinder(visualizationPanel);
            dijkstraPathfinder = new DijkstraPathfinder(visualizationPanel);

            // Set default visualizer
            currentVisualizer = sortingVisualizer;
            sortingVisualizer.SetAlgorithm(SortAlgorithm.BubbleSort);

            // Set up animation timer
            animationTimer = new Timer();
            animationTimer.Interval = 200; // Default speed
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void InitializeUI()
        {
            // Algorithm selector with modern style
            algorithmSelector = new ComboBox
            {
                Location = new Point(20, 20),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };
            algorithmSelector.Items.AddRange(new string[] { 
                "Bubble Sort", 
                "Insertion Sort", 
                "Breadth-First Search",
                "A* Pathfinding",
                "Dijkstra's Algorithm"
            });
            algorithmSelector.SelectedIndex = 0;

            // Modern button style
            playButton = new Button
            {
                Text = "Play",
                Location = new Point(240, 20),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            pauseButton = new Button
            {
                Text = "Pause",
                Location = new Point(330, 20),
                Size = new Size(80, 30),
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            resetButton = new Button
            {
                Text = "Reset",
                Location = new Point(420, 20),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // Modern NumericUpDown
            arraySizeInput = new NumericUpDown
            {
                Location = new Point(520, 20),
                Size = new Size(80, 30),
                Minimum = 5,
                Maximum = 100,
                Value = 30,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };

            generateButton = new Button
            {
                Text = "Generate",
                Location = new Point(610, 20),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // Modern TrackBar
            speedSlider = new TrackBar
            {
                Location = new Point(730, 20),
                Size = new Size(200, 30),
                Minimum = 1,
                Maximum = 20,
                Value = 10
            };

            // Visualization panel with modern style
            visualizationPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(960, 500),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            // Enable double buffering for the visualization panel
            visualizationPanel.GetType()
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(visualizationPanel, true);

            // Modern labels
            infoLabel = new Label
            {
                Location = new Point(20, 590),
                Size = new Size(960, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Select an algorithm and press 'Generate' to create random data.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            complexityLabel = new Label
            {
                Location = new Point(20, 620),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Time Complexity: ",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            stepsLabel = new Label
            {
                Location = new Point(330, 620),
                Size = new Size(200, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Steps: 0",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            // Add controls to form
            this.Controls.Add(algorithmSelector);
            this.Controls.Add(playButton);
            this.Controls.Add(pauseButton);
            this.Controls.Add(resetButton);
            this.Controls.Add(arraySizeInput);
            this.Controls.Add(generateButton);
            this.Controls.Add(speedSlider);
            this.Controls.Add(visualizationPanel);
            this.Controls.Add(infoLabel);
            this.Controls.Add(complexityLabel);
            this.Controls.Add(stepsLabel);

            // Set form properties
            this.Text = "Algorithm Visualizer";
            this.Size = new Size(1000, 700);
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetupEventHandlers()
        {
            algorithmSelector.SelectedIndexChanged += AlgorithmSelector_SelectedIndexChanged;
            playButton.Click += PlayButton_Click;
            pauseButton.Click += PauseButton_Click;
            resetButton.Click += ResetButton_Click;
            generateButton.Click += GenerateButton_Click;
            speedSlider.ValueChanged += SpeedSlider_ValueChanged;
        }

        private void AlgorithmSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset the current visualizer
            if (currentVisualizer != null)
            {
                currentVisualizer.Reset();
            }

            // Stop the animation timer
            animationTimer.Stop();
            isPlaying = false;
            playButton.Enabled = true;
            pauseButton.Enabled = false;

            int selectedIndex = algorithmSelector.SelectedIndex;

            // Show/hide the array size input and generate button for appropriate algorithms
            arraySizeInput.Visible = true;
            generateButton.Visible = true;

            // Set algorithm information
            infoLabel.Text = GetAlgorithmDescription(selectedIndex);
            complexityLabel.Text = "Time Complexity: " + GetAlgorithmComplexity(selectedIndex);
            stepsLabel.Text = "Steps: 0";

            switch (selectedIndex)
            {
                case 0: // Bubble Sort
                case 1: // Insertion Sort
                    currentVisualizer = sortingVisualizer;
                    sortingVisualizer.SetAlgorithm(selectedIndex == 0 ? SortAlgorithm.BubbleSort : SortAlgorithm.InsertionSort);
                    break;
                case 2: // BFS
                    currentVisualizer = pathfindingVisualizer;
                    break;
                case 3: // A* Pathfinding
                    currentVisualizer = aStarPathfinder;
                    break;
                case 4: // Dijkstra's Algorithm
                    currentVisualizer = dijkstraPathfinder;
                    break;
            }

            // Generate new data for the selected algorithm
            GenerateButton_Click(this, EventArgs.Empty);
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            // Stop the animation timer
            animationTimer.Stop();
            isPlaying = false;
            playButton.Enabled = true;
            pauseButton.Enabled = false;

            int selectedIndex = algorithmSelector.SelectedIndex;
            int size = (int)arraySizeInput.Value;

            switch (selectedIndex)
            {
                case 0: // Bubble Sort
                case 1: // Insertion Sort
                    sortingVisualizer.GenerateRandomArray(size);
                    break;
                case 2: // BFS
                    pathfindingVisualizer.GenerateRandomGrid();
                    break;
                case 3: // A* Pathfinding
                    aStarPathfinder.GenerateRandomGrid();
                    break;
                case 4: // Dijkstra's Algorithm
                    dijkstraPathfinder.GenerateRandomGrid();
                    break;
            }

            stepsLabel.Text = "Steps: 0";
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            playButton.Enabled = false;
            pauseButton.Enabled = true;
            isPlaying = true;
            animationTimer.Start();
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            animationTimer.Stop();
            playButton.Enabled = true;
            pauseButton.Enabled = false;
            isPlaying = false;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            animationTimer.Stop();
            isPlaying = false;
            playButton.Enabled = true;
            pauseButton.Enabled = false;

            currentVisualizer.Reset();
            stepsLabel.Text = "Steps: 0";
        }

        private void SpeedSlider_ValueChanged(object sender, EventArgs e)
        {
            // Invert the speed slider so that higher value = faster animation
            animationTimer.Interval = 600 / speedSlider.Value;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                bool isComplete = currentVisualizer.NextStep();
                stepsLabel.Text = "Steps: " + currentVisualizer.Steps;

                // Update algorithm information
                int selectedIndex = algorithmSelector.SelectedIndex;
                infoLabel.Text = GetAlgorithmDescription(selectedIndex);

                // If algorithm is complete, stop the animation
                if (isComplete)
                {
                    animationTimer.Stop();
                    isPlaying = false;
                    playButton.Enabled = true;
                    pauseButton.Enabled = false;
                    infoLabel.Text = "Algorithm completed. " + infoLabel.Text;
                }
            }
        }

        private string GetAlgorithmDescription(int algorithmIndex)
        {
            switch (algorithmIndex)
            {
                case 0:
                    return "Bubble Sort: A simple sorting algorithm that repeatedly steps through the list, compares adjacent elements, and swaps them if they are in the wrong order.";
                case 1:
                    return "Insertion Sort: A simple sorting algorithm that builds the final sorted array one item at a time.";
                case 2:
                    return "Breadth-First Search (BFS): A graph traversal algorithm that explores all the neighbor nodes at the present depth before moving to nodes at the next depth level.";
                case 3:
                    return "A* Pathfinding: A popular pathfinding algorithm that uses heuristics to find the shortest path between two points.";
                case 4:
                    return "Dijkstra's Algorithm: A graph search algorithm that finds the shortest path between nodes in a graph.";
                default:
                    return "Select an algorithm to see its description.";
            }
        }

        private string GetAlgorithmComplexity(int algorithmIndex)
        {
            switch (algorithmIndex)
            {
                case 0:
                    return "O(n²)";
                case 1:
                    return "O(n²)";
                case 2:
                    return "O(V + E)";
                case 3:
                    return "O(E log V)";
                case 4:
                    return "O(E log V)";
                default:
                    return "";
            }
        }
    }
}
