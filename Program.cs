﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Windows.Media;
using System.Media;
using System.IO;

namespace Snake
{
    struct Position
    {
        public int row;
        public int col;
        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }

    class Program
    {
        static Random randomNumbersGenerator = new Random();
        static List<Position> obstacles = new List<Position>(){};
        static Queue<Position> snakeElements = new Queue<Position>();
        static Position food;

        static void Main(string[] args)
        {
            byte right = 0;
            byte left = 1;
            byte down = 2;
            byte up = 3;
            int lastFoodTime = 0;
            int foodDissapearTime = 15000;

            //this is used to increment when the users missed some food (in this case 3) and the snake would lost one part
            int missedFoodCount = 0;

            Position[] directions = new Position[]
            {
                new Position(0, 1), // right
                new Position(0, -1), // left
                new Position(1, 0), // down
                new Position(-1, 0), // up
            };
            double sleepTime = 100;
            int direction = right;
            Console.BufferHeight = Console.WindowHeight;
            lastFoodTime = Environment.TickCount;
           
            initialiseObstacles();
            initialiseSnake();
            createFood();

            foreach (Position position in snakeElements)
            {
                Console.SetCursorPosition(position.col, position.row);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("*");
            }


            //Below are the music packs
            MediaPlayer backgroundMusic = new MediaPlayer();//Continous background music
            backgroundMusic.Open(new System.Uri(Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"..\..\sounds\\backgroundMusic.wav")));


            SoundPlayer changeEffect = new SoundPlayer(@"..\..\sounds\changePosition.wav");//sound effect when changing directions
            SoundPlayer eatEffect = new SoundPlayer(@"..\..\sounds\munchApple.wav");//sound effect when eating an apple
            SoundPlayer ObstacleEffect = new SoundPlayer(@"..\..\sounds\obstacleHit.wav");//sound effect when an obstacle is hit

            while (true)
            {
                backgroundMusic.Play();
                if (backgroundMusic.Position >= new TimeSpan(0, 1, 25))
                {
                    backgroundMusic.Position = new TimeSpan(0, 0, 0);
                }

                // Control direction of snake
                if (Console.KeyAvailable)
                {
                    changeEffect.Play();
                    ConsoleKeyInfo userInput = Console.ReadKey();
                    if (userInput.Key == ConsoleKey.LeftArrow)
                    {
                        if (direction != right) direction = left;
                    }
                    if (userInput.Key == ConsoleKey.RightArrow)
                    {
                        if (direction != left) direction = right;
                    }
                    if (userInput.Key == ConsoleKey.UpArrow)
                    {
                        if (direction != down) direction = up;
                    }
                    if (userInput.Key == ConsoleKey.DownArrow)
                    {
                        if (direction != up) direction = down;
                    }
                }

                // Reassign snake's head after crossing border
                Position snakeHead = snakeElements.Last();  // Head at end of queue
                Position nextDirection = directions[direction];

                // If crossed border, move to other end of the terminal
                Position snakeNewHead = new Position(snakeHead.row + nextDirection.row,
                    snakeHead.col + nextDirection.col);

                if (snakeNewHead.col < 0) snakeNewHead.col = Console.WindowWidth - 1;
                if (snakeNewHead.row < 0) snakeNewHead.row = Console.WindowHeight - 1;
                if (snakeNewHead.row >= Console.WindowHeight) snakeNewHead.row = 0;
                if (snakeNewHead.col >= Console.WindowWidth) snakeNewHead.col = 0;

                //points count
                int userPoints = (snakeElements.Count - 4);
                if (missedFoodCount == 3)
                {
                    //missed 3 food in a row, deduct 1 point, remove the tail
                    Position snakeTail = snakeElements.Dequeue();  // Head at beginning of queue (to remove when points deducted)
                    Console.SetCursorPosition(snakeTail.col, snakeTail.row); // Remove the last bit of snake.
                    Console.Write(" ");
                    missedFoodCount = 0;
                }

                // If the snake hits itself or hits the obstacles
                // added new rule which is the game ends when the snake is gone
                if (snakeElements.Contains(snakeNewHead) || obstacles.Contains(snakeNewHead) || snakeElements.Count == 0)
                {
                    ObstacleEffect.Play();
                    Thread.Sleep(500);
                    userPoints = Math.Max(userPoints, 0);
                    string gameovertext = "Game over!";
                    string yourpointsare = "Your points are: {0}";
                    string resultmessage;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition((Console.WindowWidth - gameovertext.Length) / 2, Console.WindowHeight / 4);
                    Console.WriteLine(gameovertext);
                    Console.SetCursorPosition((Console.WindowWidth - yourpointsare.Length) / 2, (Console.WindowHeight / 4) + 1);
                    Console.WriteLine(yourpointsare, userPoints);
                    if (userPoints >= 30)//checks if the player meets winning requirement
                    {
                        resultmessage = "Congratulation! You've won the game :D";
                        Console.SetCursorPosition((Console.WindowWidth - resultmessage.Length) / 2, (Console.WindowHeight / 4) + 2);
                        Console.WriteLine(resultmessage);
                    }
                    else
                    {
                        resultmessage = "Sorry, you've lost :(";
                        Console.SetCursorPosition((Console.WindowWidth - resultmessage.Length) / 2, (Console.WindowHeight / 4) + 2);
                        Console.WriteLine(resultmessage);
                        Console.SetCursorPosition((Console.WindowWidth - 33) / 2, (Console.WindowHeight / 4) + 3);
                        Console.WriteLine("Reach 30 Points next time to win");
                    }
					//save the score into text file
					UpdateScores(userPoints);
                    // Pause screen
                    Console.SetCursorPosition((Console.WindowWidth - 33) / 2, (Console.WindowHeight / 4) + 6);
                    Console.WriteLine("Please ENTER key to exit the game.");
                    Console.ReadLine();
                    //! Pause screen
                    return;
                }
                else
                {
                    //reset score display
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Current points:              ");
                    //display score
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Current points: {0}", userPoints);
                }

                Console.SetCursorPosition(snakeHead.col, snakeHead.row);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("*");

                snakeElements.Enqueue(snakeNewHead);
                Console.SetCursorPosition(snakeNewHead.col, snakeNewHead.row);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (direction == right) Console.Write(">");
                if (direction == left) Console.Write("<");
                if (direction == up) Console.Write("^");
                if (direction == down) Console.Write("v");

                // If snake consumes the food:
                if (snakeNewHead.col == food.col && snakeNewHead.row == food.row)
                {
                    eatEffect.Play();
                    createFood();
                    // feeding the snake
                    lastFoodTime = Environment.TickCount;  // Reset last food Time
                    sleepTime--;
                    //reset missedFoodCount = 0 when the snake consume the food
                    missedFoodCount = 0;

                    // Generate new obstacle
                    Position obstacle = new Position();
                    do
                    {
                        obstacle = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight),
                            randomNumbersGenerator.Next(0, Console.WindowWidth));
                    }
                    while (snakeElements.Contains(obstacle) ||
                        obstacles.Contains(obstacle) ||
                        (food.row != obstacle.row && food.col != obstacle.row));
                    obstacles.Add(obstacle);
                    Console.SetCursorPosition(obstacle.col, obstacle.row);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("=");
                }
                else
                {
                    // moving...
                    Position last = snakeElements.Dequeue();  // Remove the last bit of snake.
                    Console.SetCursorPosition(last.col, last.row);
                    Console.Write(" ");
                    
                }

                // If food not consumed before time limit, generate new food.
                if (Environment.TickCount - lastFoodTime >= foodDissapearTime)
                {
                    //Additional missed food
                    missedFoodCount++;
                    //negativePoints = negativePoints + 50;  // Additional negative points (no needed as the snake is shorter)
                    Console.SetCursorPosition(food.col, food.row);
                    Console.Write(" ");
                    createFood();
                    lastFoodTime = Environment.TickCount;
                }

                sleepTime -= 0.01;

                Thread.Sleep((int)sleepTime);
            }
            
        }

        // Initialise Obstacles
        public static void initialiseObstacles()
        {
            int counterX = 0;
            while (counterX < 5)
            {
                Position obstacle = new Position();
                obstacle = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight),
                    randomNumbersGenerator.Next(0, Console.WindowWidth));
                if (obstacles.Contains(obstacle))
                {
                    counterX = counterX - 1;
                }
                else
                {
                    counterX = counterX + 1;
                    obstacles.Add(obstacle);
                }
            }
            foreach (Position obstacle in obstacles)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.SetCursorPosition(obstacle.col, obstacle.row);
                Console.Write("=");
            }
        }

        // Initialise Snake
        public static void initialiseSnake()
        {
            for (int i = 0; i <= 3; i++) //change the initial length of snake from 5 to 3
            {
                snakeElements.Enqueue(new Position(0, i));
            }
        }

        // Create Food
        public static void createFood()
        {
            do
            {
                food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight),
                    randomNumbersGenerator.Next(0, Console.WindowWidth));
            }
            while (snakeElements.Contains(food) || obstacles.Contains(food));
            Console.SetCursorPosition(food.col, food.row);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("@");
        }
		
		//Update scores
		public static void UpdateScores(int aScore)
        {
            //score txt file
            string mapFile = @"..\..\scores.txt";

            if (File.Exists(mapFile))
            {
                // Read a text file line by line.
                string[] lines = File.ReadAllLines(mapFile);
                List<int> scores = new List<int>();

                //the split will be used used when username is added
                foreach (string line in lines)
                {
                    //the line can still be converted to int when there is only score
                    int getScore = Int32.Parse(line);
                    scores.Add(getScore); //scores.Add (line.Split (','));
                }
				//Add in the latest score
                scores.Add(aScore);
                scores.Sort((a, b) => b.CompareTo(a)); //descending sort
                //when name is added, this line can be used
                //scores = scores.OrderBy(x => x[1]);

                // Write the string array to the text file
                using (StreamWriter outputFile = new StreamWriter(mapFile))
                {
                    foreach (int score in scores)
					{
                        outputFile.WriteLine(score);
					}
                }
            }
            else
            {
                string errMsg = "File not exist";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition((Console.WindowWidth - errMsg.Length) / 2, Console.WindowHeight / 4);
				Console.WriteLine(errMsg);
            }
        }	
    }
}