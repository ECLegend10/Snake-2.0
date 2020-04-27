using System;
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
        static List<Position> obstacles = new List<Position>() { };
        static Queue<Position> snakeElements = new Queue<Position>();
        static Position food;

        static void Main(string[] args)
        {
            while (true)
            {
                bool gameLoop = true; //this is used so that the player could go back to main menu after played the game
                bool userPlay = false;//checks if the user wants to play snake game
                string userName = ""; //Player's name
                while (!userPlay)//loop stays in place as long as user does not want to play yet
                {
                    int fUserChoice = GameMenu();
                    userPlay = DetermineUserMenuChoice(fUserChoice);
                    if (userPlay == true)
                    {
                        GetUserName(ref userName);
                    }
                }



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

                while (gameLoop)
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
                        string thankmessage = "Thank you for playing, {0}!";
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
                        //check username
                        Console.SetCursorPosition((Console.WindowWidth - thankmessage.Length) / 2, (Console.WindowHeight / 4) + 4);
                        Console.WriteLine(thankmessage, userName);

                        //save the score into text file
                        UpdateScores(userName, userPoints);

                        // Pause screen
                        Console.SetCursorPosition((Console.WindowWidth - 33) / 2, (Console.WindowHeight / 4) + 6);
                        Console.WriteLine("Please ENTER key to go back to Main Menu.");
                        Console.ReadLine();
                        //! Pause screen
                        //return to Main Menu
                        gameLoop = false;
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
                ClearSnake();
                backgroundMusic.Stop();

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

        //Game Menu Function
        public static int GameMenu()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("SNAKE GAME!");
            string[] lMenuOptions = new string[4] { "Play", "Scores", "\tHelp", " Exit" };

            //positions and spacing for printing the menu options
            const int startX = 5;
            const int startY = 3;
            const int choiceSpacing = 5;

            int lcurrentChoice = 0;//get menu item that the user is pointing at

            ConsoleKey lUserChoice;//user's keyboard controls       
            Console.CursorVisible = false;

            do
            {
                for (int i = 0; i < lMenuOptions.Length; i++)
                {
                    /*This will print all the menu options with spacing added
                     * The text colour will be red if its the one the user is pointing at*/
                    Console.SetCursorPosition((startX * i) + choiceSpacing, startY);
                    if (i == lcurrentChoice)
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine(lMenuOptions[i]);
                    Console.ResetColor();
                }

                //checks user keyboard key actions
                lUserChoice = Console.ReadKey(true).Key;
                switch (lUserChoice)
                {
                    case ConsoleKey.LeftArrow:
                        {
                            if (lcurrentChoice % lMenuOptions.Length > 0)
                                lcurrentChoice--;//changes the current option the user is pointing at
                            break;
                        }
                    case ConsoleKey.RightArrow:
                        {
                            if (lcurrentChoice % lMenuOptions.Length < lMenuOptions.Length - 1)
                                lcurrentChoice++;
                            break;
                        }
                }
            } while (lUserChoice != ConsoleKey.Enter);//loop stays in places as long as the user does not confirm an action

            Console.Clear();//remove game menu            
            Console.CursorVisible = true;

            return lcurrentChoice;
        }

        //Game Menu Choices
        public static bool DetermineUserMenuChoice(int aUserChoice)
        {
            if (aUserChoice == 3) //Exit Game
            {
                Environment.Exit(0);
                return false;
            }
            else if (aUserChoice == 2)//Help Menu
            {
                PrintGameInstructions();
                return false;
            }
            else if (aUserChoice == 1)//Scores Menu
            {
                ReadScores();
                return false;
            }
            else if (aUserChoice == 0)//Play Snake Game
            {
                return true;
            }
            return true;
        }

        //Get userName to be store in the text file
        public static void GetUserName(ref string aName)
        {
            Console.WriteLine("SNAKE GAME!");
            Console.WriteLine("What is your name?");
            aName = Console.ReadLine();
            Console.Clear();
        }

        //Help screen
        public static void PrintGameInstructions()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 3);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("What is Snake Game?\n\n");
            Console.WriteLine("Snake is a simple computer game program that requires the users to control a \"snake\" in the screen to " +
                "obtain as many \"apples\" (@) spawned in the map. As the \"snake\" consumes each \"apple\", the length of the snake will increase " +
                "and thus, making it harder for the user to control. If the user hits a wall (=) or any part of its body (*), the game would end");
            Console.ResetColor();

            Console.WriteLine("\n\nPress ENTER key to go back to menu");
            ConsoleKeyInfo userInput = Console.ReadKey();

            //waits for user to press enter to exit help menu
            while (userInput.Key != ConsoleKey.Enter)
            {
                userInput = Console.ReadKey();

            }
            Console.Clear();

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


        public static bool ReadScores()
        {
            //score txt file
            string mapFile = @"..\..\scores.txt";
            Console.WriteLine("User Scores Board");
            Console.WriteLine("\tUsername \t\t\tScore");

            if (File.Exists(mapFile))
            {
                // Read a text file line by line.
                string[] lines = File.ReadAllLines(mapFile);
                string[] score;
                //for aligning the score
                int tabNum = 4;
                string tab = "";
                //to organize the string to be written
                string output = "";
                int inc = 1; //increment to show ranking

                Console.ForegroundColor = ConsoleColor.Cyan;
                //the split will be used used when username is added
                foreach (string line in lines)
                {
                    //only top 20 is shown
                    if (inc <= 20)
                    {
                        score = line.Split(',');
                        //align score (through different amount of tab)
                        tabNum = 4 - (score[0].Length / 8);
                        for (int i = 0; i < tabNum; i++)
                        {
                            tab += "\t";
                        }
                        //optional for name with 7 letters
                        if (score[0].Length == 7)
                        {
                            tab = "\t\t\t";
                        }
                        output = inc.ToString() + ")\t " + score[0] + tab + " " + score[1];
                        Console.WriteLine(output);
                        inc++;
                        tab = "";
                    }
                }
                Console.ResetColor();

                Console.WriteLine("\n\nPress ENTER key to go back to menu");
                ConsoleKeyInfo userInput = Console.ReadKey();

                //waits for user to press enter to exit help menu
                while (userInput.Key != ConsoleKey.Enter)
                {
                    userInput = Console.ReadKey();

                }
                Console.Clear();

                return true;
            }
            else
            {
                string errMsg = "File not exist";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition((Console.WindowWidth - errMsg.Length) / 2, Console.WindowHeight / 4);
                Console.WriteLine(errMsg);
                return false;
            }
        }


        //Update scores
        public static void UpdateScores(string aName, int aScore)
        {
            //score txt file
            string mapFile = @"..\..\scores.txt";
            //for organizing the string before writing them to the file
            string write = "";

            if (File.Exists(mapFile))
            {
                // Read a text file line by line.
                string[] lines = File.ReadAllLines(mapFile);
                List<string[]> scores = new List<string[]>();

                //get every line from the text file
                foreach (string line in lines)
                {
                    scores.Add(line.Split(','));
                }
                //Add in the latest score
                string[] newScore = { aName, aScore.ToString() };
                scores.Add(newScore);
                //sort the score list based on the score
                scores = scores.OrderByDescending(x => int.Parse(x[1])).ToList();

                // Write the string array to the text file
                using (StreamWriter outputFile = new StreamWriter(mapFile))
                {
                    foreach (string[] score in scores)
                    {
                        write = score[0] + "," + score[1];
                        outputFile.WriteLine(write);
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

        //clear screen
        public static void ClearSnake()
        {
            int length = snakeElements.Count;
            int amount = obstacles.Count;
            //delete the snake
            for (int i = 0; i < length; i++)
            {
                Position snakeElement = snakeElements.Dequeue();
                Console.SetCursorPosition(snakeElement.col, snakeElement.row);
                Console.Write(" ");

            }
            //clear all obstacles
            for (int i = 0; i < amount; i++)
            {
                obstacles.Remove(obstacles[0]);
            }
            Console.Clear();
        }
    }
}