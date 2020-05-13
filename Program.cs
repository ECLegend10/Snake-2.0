using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Windows.Media;
using System.Media;
using System.IO;
using System.Security.Principal;

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
        static Position XFood;
        static Position life;
        //Window's Height - 1 to prevent corner issue
        public static int height = Console.WindowHeight - 1;

        static void Main(string[] args)
        {
            while (true)
            {
                bool gameLoop = true; //this is used so that the player could go back to main menu after played the game
                bool userPlay = false;//checks if the user wants to play snake game
                string userName = ""; //Player's name
                int difficultyLevel = 99;  // Player's selected difficulty.
                int chooseColour = 0; //Player's selected colour's index

                string[] lMenuOptions = new string[4] { "Play", "Scores", "\tHelp", " Exit" };
                string[] lDifficultyOptions = new string[3] { "Easy", " Intermediate", "\t\tHardcore"};
                string[] lColourOptions = new string[3] { "Red", "Blue", "\tGreen" };

                while (!userPlay)//loop stays in place as long as user does not want to play yet
                {
                    int fUserChoice = GameMenu(lMenuOptions);
                    userPlay = DetermineUserMenuChoice(fUserChoice);
                    if (userPlay == true)
                    {
                        GetUserName(ref userName);
                        difficultyLevel = GameMenu(lDifficultyOptions);
                        chooseColour = GameMenu(lColourOptions);
                    }
                }

                byte right = 0, left = 1, down = 2, up = 3;
                int lastFoodTime = 0;
                int foodDissapearTime = 20000;
                int lastHealthBonusTime = 0;
                int lastHealthDissapearTime = 15000;
                int numberOfObstaclesInit = 0;
                int snakeLengthInit = 3;
                int snakeHealth = 3; 
                int bonusPoints = 0;
                double sleepTime = 100;  // Speed
                bool superXFoodEffect = false;

                //Difficulty effect
                DifficultyEffect(difficultyLevel, ref sleepTime, ref snakeLengthInit, ref numberOfObstaclesInit, ref foodDissapearTime);

                //Make effect from the chosen colour
                ConsoleColor snakeColor = ColourEffect(chooseColour, ref foodDissapearTime, ref snakeHealth, ref bonusPoints);

                //this is used to increment when the users missed some food (in this case 3) and the snake would lost one part
                int missedFoodCount = 0;

                Position[] directions = new Position[]
                {
                    new Position(0, 1), // right
                    new Position(0, -1), // left
                    new Position(1, 0), // down
                    new Position(-1, 0), // up
                };
                
                int direction = right;
                lastFoodTime = Environment.TickCount;

                initialiseObstacles(numberOfObstaclesInit);
                initialiseSnake(snakeLengthInit);
                createFood();

                foreach (Position position in snakeElements)
                {
                    Console.SetCursorPosition(position.col, position.row);
                    Console.ForegroundColor = snakeColor;
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

                    if (snakeNewHead.col < 0 || snakeNewHead.row < 0
                   || snakeNewHead.col >= Console.WindowWidth || snakeNewHead.row >= height)
                    {
                        snakeNewHead.col = (snakeNewHead.col + Console.WindowWidth) % Console.WindowWidth;
                        snakeNewHead.row = (snakeNewHead.row + height) % height;
                    }

                    //points count
                    int userPoints = (snakeElements.Count - 4) + bonusPoints;
                    if (missedFoodCount == 3)
                    {
                        //missed 3 food in a row, deduct 1 point, remove the tail
                        Position snakeTail = snakeElements.Dequeue();  // Head at beginning of queue (to remove when points deducted)
                        Console.SetCursorPosition(snakeTail.col, snakeTail.row); // Remove the last bit of snake.
                        Console.Write(" ");
                        missedFoodCount = 0;
                    }

                    if (obstacles.Contains(snakeNewHead)) // if the snake hits an obstacle it will lose 1 health
                    {
                        ObstacleEffect.Play();
                        snakeHealth--;
                        
                        int length = snakeElements.Count;
                        //delete the snake
                        for (int i = 0; i < length; i++)
                        {
                            Position snakeElement = snakeElements.Dequeue();
                            Console.SetCursorPosition(snakeElement.col, snakeElement.row);
                            Console.Write(" ");
                        }

                        if (snakeHealth != 0) // the snake will respawn again if it still has health left
                        {
                            initialiseSnake(snakeLengthInit);
                            direction = right;
                            snakeHead = snakeElements.Last();  // Head at end of queue
                            nextDirection = directions[direction];

                            // If crossed border, move to other end of the terminal
                            snakeNewHead = new Position(snakeHead.row + nextDirection.row,
                                snakeHead.col + nextDirection.col);
                            foreach (Position position in snakeElements)
                            {
                                Console.SetCursorPosition(position.col, position.row);
                                Console.ForegroundColor = snakeColor;
                                Console.Write("*");
                            }
                        }

                    }

                        // If the snake hits itself or has 0 health
                        // added new rule which is the game ends when the snake is gone
                        if (snakeElements.Contains(snakeNewHead) || snakeHealth == 0 || snakeElements.Count == 0)
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
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.SetCursorPosition(0, height);
                        Console.WriteLine("Current points:      ");
                        //display score
                        Console.SetCursorPosition(0, height);
                        Console.WriteLine("Current points: {0}", userPoints);
                        Console.SetCursorPosition(20, height);
                        Console.WriteLine("Current Life: {0}", snakeHealth);
                        
                    }

                    Console.SetCursorPosition(snakeHead.col, snakeHead.row);
                    Console.ForegroundColor = snakeColor;
                    Console.Write("*");

                    snakeElements.Enqueue(snakeNewHead);
                    Console.SetCursorPosition(snakeNewHead.col, snakeNewHead.row);
                    Console.ForegroundColor = snakeColor;
                    if (direction == right) Console.Write(">");
                    if (direction == left) Console.Write("<");
                    if (direction == up) Console.Write("^");
                    if (direction == down) Console.Write("v");

                    if (snakeNewHead.col == life.col && snakeNewHead.row == life.row) // If the snake consumes the health bonus
                    {
                        eatEffect.Play();
                        if (snakeHealth < 3) // can only gain health if it has less than 3 health
                        {
                            snakeHealth++;
                            // feeding the snake
                            lastHealthBonusTime = Environment.TickCount;  // Reset last food Time
                            sleepTime--;
                        }
                        else // the timer will reset if it consumes the health bonus but has 3 health prior to the consumption
                        {
                            lastHealthBonusTime = Environment.TickCount;  // Reset last food Time
                            sleepTime--;
                        }
                        
                    }

                        // If snake consumes X food:
                        if (snakeNewHead.col == XFood.col && snakeNewHead.row == XFood.row)
                    {
                        eatEffect.Play();
                        superXFoodEffect = true;
                    }

                    // If snake consumes the food:
                    if (snakeNewHead.col == food.col && snakeNewHead.row == food.row)
                    {
                        // Add bonus point to score after eating X.
                        if (superXFoodEffect == true) {
                            bonusPoints++;
                            superXFoodEffect = false;
                        }

                        Console.SetCursorPosition(XFood.col, XFood.row);
                        Console.Write(" ");

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
                            obstacle = new Position(randomNumbersGenerator.Next(0, height),
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
                    if (Environment.TickCount - lastFoodTime >= foodDissapearTime )
                    {
                        //Additional missed food
                        missedFoodCount++;
                        //negativePoints = negativePoints + 50;  // Additional negative points (no needed as the snake is shorter)
                        Console.SetCursorPosition(food.col, food.row);
                        Console.Write(" ");
                        Console.SetCursorPosition(XFood.col, XFood.row);
                        Console.Write(" ");
                        createFood();
                        lastFoodTime = Environment.TickCount;
                    }

                    //Health bonus will regenerate if not consumed before the time limit
                    if (Environment.TickCount - lastHealthBonusTime >= lastHealthDissapearTime && snakeHealth < 3)
                    {
                        Console.SetCursorPosition(life.col, life.row);
                        Console.Write(" ");
                        GenerateLife();
                        lastHealthBonusTime = Environment.TickCount;
                    }

                    sleepTime -= 0.01;

                    Console.SetCursorPosition(0, 0);
                    Thread.Sleep((int)sleepTime);
                }
                ClearSnake();
                backgroundMusic.Stop();
            }
        }

        // Initialise Obstacles
        public static void initialiseObstacles(int numberOfObstacles)
        {
            int counterX = 0;
            while (counterX < numberOfObstacles)
            {
                Position obstacle = new Position();
                obstacle = new Position(randomNumbersGenerator.Next(0, height),
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
        public static int GameMenu(string[] lMenuOptions)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("SNAKE GAME!");

            //positions and spacing for printing the menu options
            const int startX = 5;
            const int startY = 3;
            const int choiceSpacing = 5;

            int lcurrentChoice = 0;//get menu item that the user is pointing at

            ConsoleKey lUserChoice;//user's keyboard controls       
            Console.CursorVisible = false;

            //check whether the player is choosing colour now
            if (lMenuOptions[0] == "Red")
            {
                //show ability for each colour
                Console.SetCursorPosition(5, 10);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Ability of each colour: " +
                    "\n     1) Red - Every food would stay longer for 3 seconds" +
                    "\n     2) Blue - Gain 3 points at the beginning of the game" +
                    "\n     3) Green - Gain an extra health at the beginning of the game");
                Console.ResetColor();
            }

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
            } while (lUserChoice != ConsoleKey.Enter);  // loop stays in places as long as the user does not confirm an action

            Console.Clear();//remove game menu            
            Console.CursorVisible = true;

            return lcurrentChoice;
        }

        // Game Menu Choices
        public static bool DetermineUserMenuChoice(int aUserChoice)
        {
            if (aUserChoice == 3)  //Exit Game
            {
                Environment.Exit(0);
                return false;
            }
            else if (aUserChoice == 2)  // Help Menu
            {
                PrintGameInstructions();
                return false;
            }
            else if (aUserChoice == 1)  // Scores Menu
            {
                ReadScores();
                return false;
            }
            else if (aUserChoice == 0)  // Play Snake Game
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
                "and thus, making it harder for the user to control. If the user hits a wall (=) or any part of its body (*), the game would end" +
                "\n\nYou are required to provide a username before you play the game." +
                "\n\nThere will be 3 difficuly levels: Easy, Intermediate, Hardcore. Try your best to challenge them!" +
                "\n\nThen, you will have to choose a colour for your snake. There will be only 3 colours, but each of them has special ability!" +
                "\n1) Red - Every food would stay longer for 3 seconds" +
                "\n2) Blue - Gain 3 points at the beginning of the game" +
                "\n3) Green - Gain an extra health at the beginning of the game");
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
        public static void initialiseSnake(int initSnakeLength)
        {
            for (int i = 0; i <= initSnakeLength; i++) //change the initial length of snake from 5 to 3
            {
                snakeElements.Enqueue(new Position(0, i));
            }
        }

        //Generate the health bonus
        public static void GenerateLife()
        {
            do
            {
                life = new Position(randomNumbersGenerator.Next(0, height),
                    randomNumbersGenerator.Next(0, Console.WindowWidth));
            }
            while (snakeElements.Contains(life) || obstacles.Contains(food));

            Console.SetCursorPosition(life.col, life.row);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("L");
        }

        // Create Food
        public static void createFood()
        {
            int clickPercentage = 20;  // 10 percent chance to generate bonus food.
            int randomValueBetween0And99 = randomNumbersGenerator.Next(100);
            if (randomValueBetween0And99 < clickPercentage)
            {
                do
                {
                    XFood = new Position(randomNumbersGenerator.Next(0, height),
                        randomNumbersGenerator.Next(0, Console.WindowWidth));
                }
                while (snakeElements.Contains(XFood) || obstacles.Contains(XFood));
                Console.SetCursorPosition(XFood.col, XFood.row);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("X");
            }

            do
            {
                food = new Position(randomNumbersGenerator.Next(0, height),
                    randomNumbersGenerator.Next(0, Console.WindowWidth));
            }
            while (snakeElements.Contains(food) || obstacles.Contains(food));
            Console.SetCursorPosition(food.col, food.row);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("@");
        }

        public static void DifficultyEffect(int diff, ref double sleepTime, ref int snakeLengthInit, ref int numberOfObstaclesInit, ref int foodDissapearTime)
        {
            if (diff == 2)  // Hardcore
            {
                sleepTime = 40;
                snakeLengthInit = 12;
                numberOfObstaclesInit = 10;
                foodDissapearTime = 10000;
            }
            else if (diff == 1)  // Intermediate
            {
                sleepTime = 70;
                snakeLengthInit = 7;
                numberOfObstaclesInit = 5;
                foodDissapearTime = 15000;
            }
            // Current configuration is for easy mode, therefore for easy mode nothing needs to change.
        }

        public static ConsoleColor ColourEffect(int colour, ref int time, ref int health, ref int points)
        {
            if (colour == 0) 
            {
                time += 3000;
                return ConsoleColor.Red;
            }
            else if (colour == 1)
            {
                points += 3;
                return ConsoleColor.Blue;
            }
            else
            {
                health++;
                return ConsoleColor.Green;
            }
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
            Console.ResetColor();
        }
    }
}