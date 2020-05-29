using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.IO;
using System.Security.Principal;
using System.Windows.Input;


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
        static Position food2;
        static Position XFood;
        static Position life;
        static List<string[]> scoreboard = ReadScores(); //Scoreboard from text file

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            while (true)
            {
                bool gameLoop = true; //this is used so that the player could go back to main menu after played the game
                bool userPlay = false;//checks if the user wants to play snake game
                string userName = ""; //Player's name
                int difficultyLevel = 99;  // Player's selected difficulty.
                int chooseColour = 0; //Player's selected colour's index

                string[] lMenuOptions = new string[4] { "Play", "Scores", "Help", " Exit" };
                string[] lDifficultyOptions = new string[3] { "Easy", " Intermediate", "\t\tHardcore" };
                string[] lColourOptions = new string[8] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Purple", "Rainbow" };
                //to change colours for rainbow
                string[] lColorForRainbow = new string[7] { "Red", "DarkYellow", "Yellow", "Green", "Blue", "Magenta", "DarkMagenta" };


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
                bool chooseRainbow = false;
                int colourIndex = 0;

                //Difficulty effect
                DifficultyEffect(difficultyLevel, ref sleepTime, ref snakeLengthInit, ref numberOfObstaclesInit, ref foodDissapearTime, ref bonusPoints);

                //Make effect from the chosen colour
                ConsoleColor snakeColor = ColourEffect(chooseColour, ref foodDissapearTime, ref snakeHealth, ref bonusPoints, ref sleepTime, ref snakeLengthInit);

                //Checks if the player chose Rainbow colour
                if (snakeColor == ConsoleColor.White)
                {
                    chooseRainbow = true;
                }


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
                    if (chooseRainbow)
                    {
                        ChangeColour(ref snakeColor, ref colourIndex, lColorForRainbow);
                    }
                    Console.ForegroundColor = snakeColor;
                    Console.Write("*");
                }

                while (gameLoop)
                {

                    // Control direction of snake
                    if (Console.KeyAvailable)
                    {
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
                        // Pause during gameplay
                        if (userInput.Key == ConsoleKey.P)
                        {
                            Console.SetCursorPosition(0, 4);
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine("Game paused. Press any key to continue.");
                            Console.SetCursorPosition(0, 0);
                            Console.Write("G");
                            Console.ReadKey();
                            Console.SetCursorPosition(0, 4);
                            Console.WriteLine("                                       ");
                            Console.ForegroundColor = snakeColor;
                        }
                    }


                    // Reassign snake's head after crossing border
                    Position snakeHead = snakeElements.Last();  // Head at end of queue
                    Position nextDirection = directions[direction];

                    // If crossed border, move to other end of the terminal
                    Position snakeNewHead = new Position(snakeHead.row + nextDirection.row,
                        snakeHead.col + nextDirection.col);


                    if (snakeNewHead.col < 0) snakeNewHead.col = Console.WindowWidth - 1;
                    if (snakeNewHead.row < 6) snakeNewHead.row = Console.WindowHeight - 1; // if it reaches the info header
                    if (snakeNewHead.row >= Console.WindowHeight) snakeNewHead.row = 5;// if it goes to the bottom, it will come out from the info header
                    if (snakeNewHead.col >= Console.WindowWidth) snakeNewHead.col = 0;

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
                        //ObstacleEffect.Play();
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
                                if (chooseRainbow)
                                {
                                    ChangeColour(ref snakeColor, ref colourIndex, lColorForRainbow);
                                }
                                Console.Write("*");
                            }
                        }

                    }

                    // If the snake hits itself or has 0 health
                    // added new rule which is the game ends when the snake is gone
                    if (snakeElements.Contains(snakeNewHead) || snakeHealth == 0 || snakeElements.Count == 0)
                    {
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
                        //display score
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("Game Information for {0}:", userName);
                        if (userPoints >= 30)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        Console.SetCursorPosition(0, 1);
                        Console.WriteLine("Current points:      ");
                        Console.SetCursorPosition(0, 1);
                        Console.WriteLine("Current points: {0}", userPoints);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Current Life: {0}", snakeHealth);
                        Console.WriteLine();
                        //display timer for the duration of food
                        Console.SetCursorPosition(40, 1);
                        Console.WriteLine("The food will dissapear in:      ");
                        Console.SetCursorPosition(40, 1);
                        Console.WriteLine("The food will dissapear in: {0}s", ((foodDissapearTime / 1000) - (Environment.TickCount - lastFoodTime) / 1000));
                        //display top 3 of the scoreboard
                        Console.SetCursorPosition(80, 0);
                        Console.WriteLine("Scoreboard for Top 3:");
                        for (int i = 0; i < 3; i++)
                        {
                            Console.SetCursorPosition(80, i + 1);
                            Console.WriteLine("{0}) {1}\t{2}", i + 1, scoreboard[i][0], scoreboard[i][1]);
                        }
                        Console.SetCursorPosition(0, 5);
                        Console.WriteLine("________________________________________________________________________________________________________________________");
                    }

                    Console.SetCursorPosition(snakeHead.col, snakeHead.row);
                    if (chooseRainbow)
                    {
                        ChangeColour(ref snakeColor, ref colourIndex, lColorForRainbow);
                    }
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
                        superXFoodEffect = true;
                    }

                    // If snake consumes the food:
                    if ((snakeNewHead.col == food.col && snakeNewHead.row == food.row) || (snakeNewHead.col == food2.col && snakeNewHead.row == food2.row))
                    {
                        if (snakeNewHead.col == food.col && snakeNewHead.row == food.row)
                        {
                            Console.SetCursorPosition(food2.col, food2.row);
                            Console.Write(" ");
                        }
                        else
                        {
                            Console.SetCursorPosition(food.col, food.row);
                            Console.Write(" ");
                        }

                        // Add bonus point to score after eating X.
                        if (superXFoodEffect == true)
                        {
                            bonusPoints++;
                            superXFoodEffect = false;
                        }

                        Console.SetCursorPosition(XFood.col, XFood.row);
                        Console.Write(" ");

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
                            obstacle = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight - 1),
                                randomNumbersGenerator.Next(0, Console.WindowWidth));
                        }
                        while (snakeElements.Contains(obstacle) ||
                            obstacles.Contains(obstacle) ||
                            (food.row != obstacle.row && food.col != obstacle.row));
                        obstacles.Add(obstacle);
                        Console.SetCursorPosition(obstacle.col, obstacle.row);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("▒");
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
                        Console.SetCursorPosition(food2.col, food2.row);
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

                    if (sleepTime > 0.01)
                    {
                        sleepTime -= 0.01;
                    }
                    else
                    {
                        sleepTime = 0.01;
                    }

                    Console.SetCursorPosition(0, 0);
                    Thread.Sleep((int)sleepTime);
                }
                ClearSnake();
            }
        }

        // Initialise Obstacles
        public static void initialiseObstacles(int numberOfObstacles)
        {
            int counterX = 0;
            while (counterX < numberOfObstacles)
            {
                Position obstacle = new Position();
                int obstacleYLocation = randomNumbersGenerator.Next(0, Console.WindowHeight);
                if (obstacleYLocation < 6)//checks if it is spawned in the header area
                {
                    obstacleYLocation += 6;
                }
                obstacle = new Position(obstacleYLocation,
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
                Console.Write("▒");
            }
        }

        //Game Menu Function
        public static int GameMenu(string[] lMenuOptions)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(" ______  __   __  ______  __  __  ______       ______  ______  __    __  ______    ");
            Console.WriteLine("/\\  ___\\/\\ \"-.\\ \\/\\  __ \\/\\ \\/ / /\\  ___\\     /\\  ___\\/\\  __ \\/\\ \"-./  \\/\\  ___\\   ");
            Console.WriteLine("\\ \\___  \\ \\ \\-.  \\ \\  __ \\ \\  _\"-\\ \\  __\\     \\ \\ \\__ \\ \\  __ \\ \\ \\-./\\ \\ \\  __\\   ");
            Console.WriteLine(" \\/\\_____\\ \\_\\\\\"\\_\\ \\_\\ \\_\\ \\_\\ \\_\\ \\_____\\    \\ \\_____\\ \\_\\ \\_\\ \\_\\ \\ \\_\\ \\_____\\ ");
            Console.WriteLine("  \\/_____/\\/_/ \\/_/\\/_/\\/_/\\/_/\\/_/\\/_____/     \\/_____/\\/_/\\/_/\\/_/  \\/_/\\/_____/ ");

            //positions and spacing for printing the menu options
            const int startX = 10;
            const int startY = 7;
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
                    "\n     1) Red - Every food would stay longer for 3 seconds." +
                    "\n     2) Orange - Challenge yourself! The health of snake would be 1, but gain 5 points." +
                    "\n     3) Yellow - The snake is 2 units shorter." +
                    "\n     4) Green - Gain an extra health at the beginning of the game." +
                    "\n     5) Blue - Gain 3 points at the beginning of the game." +
                    "\n     6) Indigo - The snake is slower than normal speed." +
                    "\n     7) Purple - The snake is faster than normal speed." +
                    "\n     8) Rainbow - Nice visual effect!");
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
                DisplayScores();
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
            Console.WriteLine(" ______  __   __  ______  __  __  ______       ______  ______  __    __  ______    ");
            Console.WriteLine("/\\  ___\\/\\ \"-.\\ \\/\\  __ \\/\\ \\/ / /\\  ___\\     /\\  ___\\/\\  __ \\/\\ \"-./  \\/\\  ___\\   ");
            Console.WriteLine("\\ \\___  \\ \\ \\-.  \\ \\  __ \\ \\  _\"-\\ \\  __\\     \\ \\ \\__ \\ \\  __ \\ \\ \\-./\\ \\ \\  __\\   ");
            Console.WriteLine(" \\/\\_____\\ \\_\\\\\"\\_\\ \\_\\ \\_\\ \\_\\ \\_\\ \\_____\\    \\ \\_____\\ \\_\\ \\_\\ \\_\\ \\ \\_\\ \\_____\\ ");
            Console.WriteLine("  \\/_____/\\/_/ \\/_/\\/_/\\/_/\\/_/\\/_/\\/_____/     \\/_____/\\/_/\\/_/\\/_/  \\/_/\\/_____/ ");
            Console.WriteLine("\n\nWhat is your name?");
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
                "obtain as many \"apples\" (♥) spawned in the map. As the \"snake\" consumes each \"apple\", the length of the snake will increase " +
                "and thus, making it harder for the user to control. If the user hits a wall (▒) or any part of its body (*), the game would end" +
                "\n\nYou are required to provide a username before you play the game." +
                "\n\nThere will be 3 difficuly levels: Easy, Intermediate, Hardcore. Try your best to challenge them!" +
                "\n\nThen, you will have to choose a colour for your snake. There will be only 3 colours, but each of them has special ability!" +
                "\n1) Red - Every food would stay longer for 3 seconds" +
                "\n2) Orange - Challenge yourself! The health of snake would be 1, but gain 5 points." +
                "\n3) Yellow - The snake is 2 units shorter." +
                "\n4) Green - Gain an extra health at the beginning of the game." +
                "\n5) Blue - Gain 3 points at the beginning of the game." +
                "\n6) Indigo - The snake is slower than normal speed." +
                "\n7) Purple - The snake is faster than normal speed." +
                "\n8) Rainbow - Nice visual effect!");
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
                snakeElements.Enqueue(new Position(6, i));
            }
        }

        //Generate the health bonus
        public static void GenerateLife()
        {
            int LifeYLocation = randomNumbersGenerator.Next(0, Console.WindowHeight);
            if (LifeYLocation < 6)//checks if it is spawned in the header area
            {
                LifeYLocation += 6;
            }
            do
            {
                life = new Position(LifeYLocation,
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
            int clickPercentage = 20;  // 20 percent chance to generate bonus food.
            int randomValueBetween0And99 = randomNumbersGenerator.Next(100);
            int FoodYLocation = randomNumbersGenerator.Next(0, Console.WindowHeight);
            if (FoodYLocation < 6)//checks if it is spawned in the header area
            {
                FoodYLocation += 6;
            }

            int ExtraFoodYLocation = randomNumbersGenerator.Next(0, Console.WindowHeight);
            if (ExtraFoodYLocation < 6)
            {
                ExtraFoodYLocation += 6;
            }


            if (randomValueBetween0And99 < clickPercentage)
            {
                do
                {
                    XFood = new Position(ExtraFoodYLocation,
                        randomNumbersGenerator.Next(0, Console.WindowWidth));
                }
                while (snakeElements.Contains(XFood) || obstacles.Contains(XFood));
                Console.SetCursorPosition(XFood.col, XFood.row);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("X");
            }

            do
            {
                int nFoodXLocation = randomNumbersGenerator.Next(0, Console.WindowWidth);
                food = new Position(FoodYLocation, nFoodXLocation - 1);
                if (nFoodXLocation == Console.WindowWidth)
                {
                    food2 = new Position(FoodYLocation, nFoodXLocation - 2);
                }
                else
                {
                    food2 = new Position(FoodYLocation, nFoodXLocation);
                }
            }
            while ((snakeElements.Contains(food) || obstacles.Contains(food)) || (snakeElements.Contains(food2) || obstacles.Contains(food2)));
            Console.SetCursorPosition(food.col, food.row);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\u2665");
            Console.SetCursorPosition(food2.col, food2.row);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\u2665");
        }

        public static void DifficultyEffect(int diff, ref double sleepTime, ref int snakeLengthInit, ref int numberOfObstaclesInit, ref int foodDissapearTime, ref int bonusPoints)
        {
            if (diff == 2)  // Hardcore
            {
                sleepTime = 40;
                snakeLengthInit = 12;
                bonusPoints -= 9;
                numberOfObstaclesInit = 10;
                foodDissapearTime = 10000;
            }
            else if (diff == 1)  // Intermediate
            {
                sleepTime = 70;
                snakeLengthInit = 7;
                bonusPoints -= 4;
                numberOfObstaclesInit = 5;
                foodDissapearTime = 15000;
            }
            // Current configuration is for easy mode, therefore for easy mode nothing needs to change.
        }

        public static ConsoleColor ColourEffect(int colour, ref int time, ref int health, ref int bonusPoints, ref double sleepTime, ref int snakeLengthInit)
        {
            if (colour == 0)
            {
                time += 3000;
                return ConsoleColor.Red;
            }
            else if (colour == 1)
            {
                health = 1;
                bonusPoints += 5;
                return ConsoleColor.DarkYellow;
            }
            else if (colour == 2)
            {
                snakeLengthInit -= 2;
                bonusPoints += 2;
                return ConsoleColor.Yellow;
            }
            else if (colour == 3)
            {
                health++;
                return ConsoleColor.Green;
            }
            else if (colour == 4)
            {
                bonusPoints += 3;
                return ConsoleColor.Blue;

            }
            else if (colour == 5)
            {
                sleepTime -= 10;
                return ConsoleColor.Magenta;
            }
            else if (colour == 6)
            {
                sleepTime += 10;
                return ConsoleColor.DarkMagenta;
            }
            else
            {
                return ConsoleColor.White;
            }
        }

        public static void ChangeColour(ref ConsoleColor color, ref int colourIndex, string[] lColorForRainbow)
        {
            colourIndex = (colourIndex + 1) % 7;
            string lCurrentColour = lColorForRainbow[colourIndex];
            color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), lCurrentColour);
        }

        public static List<string[]> ReadScores()
        {
            //score txt file
            string mapFile = @"..\..\scores.txt";

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
                return scores;
            }
            else
            {
                string errMsg = "File not exist";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition((Console.WindowWidth - errMsg.Length) / 2, Console.WindowHeight / 4);
                Console.WriteLine(errMsg);
                return null;
            }
        }

        public static void DisplayScores()
        {
            Console.WriteLine("User Scores Board");
            Console.WriteLine("\tUsername \t\t\tScore");


            //for aligning the score
            int tabNum = 4;
            string tab = "";
            //to organize the string to be written
            string output = "";
            int inc = 1; //increment to show ranking    

            Console.ForegroundColor = ConsoleColor.Cyan;
            //the split will be used used when username is added
            foreach (string[] score in scoreboard)
            {
                //only top 20 is shown
                if (inc <= 20)
                {
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
                List<string[]> scores = scoreboard;

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