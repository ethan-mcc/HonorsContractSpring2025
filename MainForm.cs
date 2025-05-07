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
        private IAlgorithmVisualizer currentVisualizer;

        private bool isPlaying = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            SetupEventHandlers();
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
            algorithmSelector.Items.AddRange(new string[] { "Bubble Sort", "Insertion Sort", "Breadth-First Search" });
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

            // Initialize visualizers
            sortingVisualizer = new SortingVisualizer(visualizationPanel);
            pathfindingVisualizer = new PathfindingVisualizer(visualizationPanel);
            currentVisualizer = sortingVisualizer;

            // Setup timer
            animationTimer = new Timer
            {
                Interval = 100
            };
        }

        private void SetupEventHandlers()
        {
            algorithmSelector.SelectedIndexChanged += AlgorithmSelector_SelectedIndexChanged;
            generateButton.Click += GenerateButton_Click;
            playButton.Click += PlayButton_Click;
            pauseButton.Click += PauseButton_Click;
            resetButton.Click += ResetButton_Click;
            speedSlider.ValueChanged += SpeedSlider_ValueChanged;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AlgorithmSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            isPlaying = false;
            animationTimer.Stop();
            playButton.Enabled = true;
            pauseButton.Enabled = false;

            switch (algorithmSelector.SelectedIndex)
            {
                case 0: // Bubble Sort
                case 1: // Insertion Sort
                    currentVisualizer = sortingVisualizer;
                    arraySizeInput.Visible = true;
                    generateButton.Visible = true;
                    complexityLabel.Text = algorithmSelector.SelectedIndex == 0 
                        ? "Time Complexity: O(n²)" 
                        : "Time Complexity: O(n²)";
                    break;
                case 2: // BFS
                    currentVisualizer = pathfindingVisualizer;
                    arraySizeInput.Visible = false;
                    generateButton.Visible = true;
                    complexityLabel.Text = "Time Complexity: O(V + E)";
                    break;
            }

            stepsLabel.Text = "Steps: 0";
            currentVisualizer.Reset();
            infoLabel.Text = GetAlgorithmDescription(algorithmSelector.SelectedIndex);
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            isPlaying = false;
            animationTimer.Stop();
            playButton.Enabled = true;
            pauseButton.Enabled = false;

            switch (algorithmSelector.SelectedIndex)
            {
                case 0: // Bubble Sort
                    sortingVisualizer.GenerateRandomArray((int)arraySizeInput.Value);
                    sortingVisualizer.SetAlgorithm(SortAlgorithm.BubbleSort);
                    break;
                case 1: // Insertion Sort
                    sortingVisualizer.GenerateRandomArray((int)arraySizeInput.Value);
                    sortingVisualizer.SetAlgorithm(SortAlgorithm.InsertionSort);
                    break;
                case 2: // BFS
                    pathfindingVisualizer.GenerateRandomGrid();
                    break;
            }

            stepsLabel.Text = "Steps: 0";
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            isPlaying = true;
            animationTimer.Start();
            playButton.Enabled = false;
            pauseButton.Enabled = true;
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            isPlaying = false;
            animationTimer.Stop();
            playButton.Enabled = true;
            pauseButton.Enabled = false;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            isPlaying = false;
            animationTimer.Stop();
            playButton.Enabled = true;
            pauseButton.Enabled = false;
            currentVisualizer.Reset();
            stepsLabel.Text = "Steps: 0";
        }

        private void SpeedSlider_ValueChanged(object sender, EventArgs e)
        {
            // Speed is inversely proportional to interval
            animationTimer.Interval = 500 / speedSlider.Value;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                bool isCompleted = currentVisualizer.NextStep();
                stepsLabel.Text = $"Steps: {currentVisualizer.Steps}";

                if (isCompleted)
                {
                    isPlaying = false;
                    animationTimer.Stop();
                    playButton.Enabled = true;
                    pauseButton.Enabled = false;
                    infoLabel.Text = "Algorithm completed!";
                }
            }
        }

        private string GetAlgorithmDescription(int algorithmIndex)
        {
            switch (algorithmIndex)
            {
                case 0:
                    return "Bubble Sort: A simple sorting algorithm that repeatedly steps through the list, compares adjacent elements and swaps them if they are in wrong order.";
                case 1:
                    return "Insertion Sort: Builds the sorted array one item at a time by comparing each with the prior elements.";
                case 2:
                    return "Breadth-First Search: Explores all neighbor nodes at the present depth before moving on to nodes at the next depth level.";
                default:
                    return "";
            }
        }
    }
}
