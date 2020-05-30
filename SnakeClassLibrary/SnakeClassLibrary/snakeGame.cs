﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.IO;
using System.Security.Principal;
using System.Windows.Input;


namespace SnakeClassLibrary
{
    public struct Position
    {
        public int row;
        public int col;
        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }

    public class snakeGame
    {
        public static Random randomNumbersGenerator = new Random();
        public static List<Position> obstacles = new List<Position>() { };
        public static Queue<Position> snakeElements = new Queue<Position>();
        public static Position food;
        public static Position food2;
        public static Position XFood;
        public static Position life;

        public bool userPlay = false;//checks if the user wants to play snake game
        public string userName = ""; //Player's name
        public int difficultyLevel = 99;  // Player's selected difficulty.
        public int chooseColour = 0; //Player's selected colour's index

        
        public int foodDissapearTime = 20000;
        public int snakeHealth = 3;
        public int bonusPoints = 0;
        public double sleepTime = 100;  // Speed
        public int snakeLengthInit = 3;
        public int fUserChoice;

        public int lastFoodTime = 0;
        public int lastHealthBonusTime = 0;
        public int lastHealthDissapearTime = 15000;
        public int numberOfObstaclesInit = 0;
        public bool superXFoodEffect = false;
        public bool chooseRainbow = false;
        public int colourIndex = 0;

        string[] lMenuOptions = new string[4] { "Play", "Scores", "Help", " Exit" };
        string[] lDifficultyOptions = new string[3] { "Easy", " Intermediate", "\t\tHardcore" };
        string[] lColourOptions = new string[8] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Purple", "Rainbow" };
        //to change colours for rainbow
        string[] lColorForRainbow = new string[7] { "Red", "DarkYellow", "Yellow", "Green", "Blue", "Magenta", "DarkMagenta" };


       
        // Game Menu Choices
        public static bool DetermineUserMenuChoice(int aUserChoice)
        {
            if (aUserChoice == 3)  //Exit Game
            {
                //Environment.Exit(0);
                return false;
            }
            else if (aUserChoice == 2)  // Help Menu
            {
                //PrintGameInstructions();
                return false;
            }
            else if (aUserChoice == 1)  // Scores Menu
            {
                //DisplayScores();
                return false;
            }
            else if (aUserChoice == 0)  // Play Snake Game
            {
                return true;
            }
            return true;
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

       
    }
}