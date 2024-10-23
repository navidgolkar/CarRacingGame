using System.Drawing;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace CarRacingGame
{
    public partial class Form1 : Form
    {
        private const int LaneCount = 5;
        //private const int MinCarSpeed = 10;
        //private const int MaxCarSpeed = 30;
        private int CarWidth;
        private int CarHeight;
        private int ObstacleHeight;
        private int ObstacleWidth;
        private int CoinWidth;

        private short time=0;
        private short timeCounter=0;

        private int carX;
        private int carY;
        private int RoadWidth;
        private int RoadHeight;
        private int LaneWidth;
        private int carSpeed = 10;
        //private int speed = 5; //The amount by which the carSpeed changes
        private int currentLane = (int)(LaneCount / 2) + 1; // Current lane where the car is

        private bool isMovingLeft;
        private bool isMovingRight;
        //private bool isAccelerating;
        //private bool isDescelarating;
        private bool canChangeLane = true;
        //private bool canSpeed = true;
        private int points = 0;
        private bool isGameOver = false;

        private List<int> lanePositions = new List<int>(); // Stores the positions of lane separators
        private Random random = new Random();
        private List<Rectangle> obstacles = new List<Rectangle>();
        private List<Rectangle> coins = new List<Rectangle>();
        private WinFormsTimer timer = new WinFormsTimer();

        public Form1()
        {
            InitializeComponent();
            RoadWidth = ClientSize.Width;
            RoadHeight = ClientSize.Height;
            LaneWidth = RoadWidth / LaneCount;
            InitializeGame();
            InitializeTimer();
            InitializeLanes();
            CarWidth = LaneWidth / 5;
            CarHeight = CarWidth * 3 / 2;
            ObstacleHeight = CarHeight / 4;
            ObstacleWidth = LaneWidth * 4 / 5;
            CoinWidth = CarWidth / 3;
            carX = lanePositions[currentLane] + (LaneWidth - CarWidth) / 2;
            carY = ClientSize.Height - CarHeight - 50;
        }
        private void InitializeLanes()
        {
            for (int i = 0; i < LaneCount; i++) lanePositions.Add(i * LaneWidth);
        }
        private void InitializeGame()
        {
            this.DoubleBuffered = true;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }
        private void InitializeTimer()
        {
            timer.Interval = 30;
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            MoveCar();
            //SpeedCar();
            GenerateObstacles();
            GenerateCoins();
            CheckCollisions();
            time++;
            SpeedPoint();
            Invalidate();
        }
        private void MoveCar()
        {
            if (isMovingLeft && currentLane > 0 && canChangeLane)
            {
                currentLane--;
                carX = lanePositions[currentLane] + (LaneWidth - CarWidth) / 2;
                canChangeLane = false;
            }
            if (isMovingRight && currentLane < LaneCount - 1 && canChangeLane)
            {
                currentLane++;
                carX = lanePositions[currentLane] + (LaneWidth - CarWidth) / 2;
                canChangeLane = false;
            }
        }
        /*private void SpeedCar()
        {
            if (isAccelerating && carSpeed + speed <= MaxCarSpeed && canSpeed)
            {
                carSpeed += speed;
                canSpeed = false;
            }
            if (isDescelarating && carSpeed - speed >= MinCarSpeed && canSpeed)
            {
                carSpeed -= speed;
                canSpeed = false;
            }
        }*/
        private void GenerateObstacles()
        {
            // Generate new obstacle at random positions without overlapping
            if (random.Next(0, 100) < 10) // Adjust the probability as needed
            {
                int obstacleX = random.Next(0, LaneCount) * LaneWidth + (LaneWidth - ObstacleWidth) / 2;
                int obstacleY = -ObstacleHeight; // Start above the form
                Rectangle newObstacle = new Rectangle(obstacleX, obstacleY, ObstacleWidth, ObstacleHeight);

                // Check for overlap with existing obstacles and coins
                bool overlap = false;
                foreach (var obstacle in obstacles)
                {
                    if (newObstacle.IntersectsWith(obstacle))
                    {
                        overlap = true;
                        break;
                    }
                    if (obstacle.X == obstacleX && Math.Abs(obstacle.Y - obstacleY) < CarHeight * 3)
                    {
                        overlap = true;
                        break;
                    }
                }
                foreach (var coin in coins)
                {
                    if (newObstacle.IntersectsWith(coin))
                    {
                        overlap = true;
                        break;
                    }
                    if (Math.Abs(coin.X - obstacleX) < LaneWidth && Math.Abs(coin.Y - obstacleY) < CarHeight * 3)
                    {
                        overlap = true;
                        break;
                    }
                }
                // If no overlap, add the new obstacle
                if (!overlap && obstacles.Count < LaneCount - 1)
                {
                    obstacles.Add(newObstacle);
                }
            }
            // Move obstacles downwards and remove off-screen obstacles
            for (int i = 0; i < obstacles.Count; i++)
            {
                obstacles[i] = new Rectangle(obstacles[i].X, obstacles[i].Y + carSpeed, obstacles[i].Width, obstacles[i].Height);

                // Remove obstacles that have passed the screen
                if (obstacles[i].Y > RoadHeight)
                {
                    obstacles.RemoveAt(i);
                    i--;
                }
            }
        }
        private void GenerateCoins()
        {
            // Generate new coin at random positions without overlapping
            if (random.Next(0, 100) < 5) // Adjust the probability as needed
            {
                int coinX = random.Next(0, LaneCount) * LaneWidth + (LaneWidth - CoinWidth) / 2;
                int coinY = -CoinWidth; // Start above the form
                Rectangle newCoin = new Rectangle(coinX, coinY, CoinWidth, CoinWidth);

                // Check for overlap with existing obstacles and coins
                bool overlap = false;
                foreach (var obstacle in obstacles)
                {
                    if (newCoin.IntersectsWith(obstacle))
                    {
                        overlap = true;
                        break;
                    }
                    if (Math.Abs(coinX - obstacle.X) < LaneWidth && Math.Abs(coinY - obstacle.Y) < CarHeight * 3)
                    {
                        overlap = true;
                        break;
                    }
                }
                foreach (var coin in coins)
                {
                    if (newCoin.IntersectsWith(coin))
                    {
                        overlap = true;
                        break;
                    }
                    if (coin.X == coinX && Math.Abs(coin.Y - coinY) < CarHeight * 2)
                    {
                        overlap = true;
                        break;
                    }
                }
                // If no overlap, add the new coin
                if (!overlap && coins.Count < LaneCount - 2)
                {
                    coins.Add(newCoin);
                }
            }
            // Move coins downwards and remove off-screen coins
            for (int i = 0; i < coins.Count; i++)
            {
                coins[i] = new Rectangle(coins[i].X, coins[i].Y + carSpeed, coins[i].Width, coins[i].Height);

                // Remove coins that have passed the screen
                if (coins[i].Y > RoadHeight)
                {
                    coins.RemoveAt(i);
                    i--;
                }
            }
        }
        private void CheckCollisions()
        {
            // Check collision with obstacles
            foreach (var obstacle in obstacles)
            {
                if (obstacle.IntersectsWith(new Rectangle(carX, carY, CarWidth, CarHeight)))
                {
                    isGameOver = true;
                    timer.Stop();
                    MessageBox.Show("Game Over! Your score: " + points);
                    break;
                }
            }
            // Check collision with coins
            for (int i = 0; i < coins.Count; i++)
            {
                if (coins[i].IntersectsWith(new Rectangle(carX, carY, CarWidth, CarHeight)))
                {
                    points += 5; // Increase points
                    coins.RemoveAt(i); // Remove collected coin
                    break;
                }
            }
        }
        private void SpeedPoint()
        {
            if (!isGameOver)
            {
                if ((time*timer.Interval)/1000  > 1)
                {
                    time = 0;
                    timeCounter++;
                    points += carSpeed;
                }
                if (timeCounter == 5)
                {
                    timeCounter = 0;
                    carSpeed ++;
                }
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isGameOver)
            {
                if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
                {
                    isMovingLeft = true;
                    canChangeLane = true;
                }
                if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
                {
                    isMovingRight = true;
                    canChangeLane = true;
                }
                /*if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
                {
                    isAccelerating = true;
                    canSpeed = true;
                }
                if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
                {
                    isDescelarating = true;
                    canSpeed = true;
                }*/
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
                isMovingLeft = false;
            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
                isMovingRight = false;
            /*if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
                isAccelerating = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
                isDescelarating = false;*/
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawRoad(e.Graphics);
            DrawCar(e.Graphics);
            DrawObstacles(e.Graphics);
            DrawCoins(e.Graphics);
            DrawPoints(e.Graphics);
        }
        private void DrawRoad(Graphics g)
        {
            g.FillRectangle(Brushes.Gray, 0, 0, RoadWidth, RoadHeight);
            Pen pen = new Pen(Color.White);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            for (int i = 0; i < lanePositions.Count; i++)
            {
                int x = lanePositions[i];
                g.DrawLine(pen, x, 0, x, ClientSize.Height);
            }
            pen.Dispose();
        }
        private void DrawCar(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, carX, carY, CarWidth, CarHeight);
        }
        private void DrawObstacles(Graphics g)
        {
            foreach (var obstacle in obstacles)
            {
                g.FillRectangle(Brushes.Red, obstacle);
            }
        }
        private void DrawCoins(Graphics g)
        {
            foreach (var coin in coins)
            {
                g.FillEllipse(Brushes.Yellow, coin);
            }
        }
        private void DrawPoints(Graphics g)
        {
            string pointsText = "Points: " + points;
            g.DrawString(pointsText, Font, Brushes.GreenYellow, new Point((int)(lanePositions[0] + (LaneWidth - (pointsText.Length * Font.Size)) / 2), 10));
            string speedText = "Speed: " + carSpeed;
            g.DrawString(speedText, Font, Brushes.LightGreen, new Point((int)(lanePositions[1] + (LaneWidth - (speedText.Length * Font.Size)) / 2), 10));
        }

    }
}