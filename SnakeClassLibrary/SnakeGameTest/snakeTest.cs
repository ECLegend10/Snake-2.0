using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SnakeClassLibrary;


namespace SnakeGameTest
{
    [TestClass]
    public class snakeTest
    {
        [TestMethod]
        public void ChangeColourTest()
        {
            snakeGame snakeGame1 = new snakeGame();
            snakeGame1.chooseColour = 0;
            ConsoleColor snakeColor = snakeGame.ColourEffect(snakeGame1.chooseColour, ref snakeGame1.foodDissapearTime, ref snakeGame1.snakeHealth, ref snakeGame1.bonusPoints, ref snakeGame1.sleepTime, ref snakeGame1.snakeLengthInit);
            Assert.AreEqual(snakeColor, ConsoleColor.Red);
            Assert.AreEqual(snakeGame1.foodDissapearTime, 23000);

            snakeGame1.chooseColour = 1;
            snakeColor = snakeGame.ColourEffect(snakeGame1.chooseColour, ref snakeGame1.foodDissapearTime, ref snakeGame1.snakeHealth, ref snakeGame1.bonusPoints, ref snakeGame1.sleepTime, ref snakeGame1.snakeLengthInit);
            Assert.AreEqual(snakeColor, ConsoleColor.DarkYellow);
            Assert.AreEqual(snakeGame1.snakeHealth, 1);
            Assert.AreEqual(snakeGame1.bonusPoints, 5);
        }

        [TestMethod]
        public void changeDifficultyTest()
        {
            snakeGame snakeGame1 = new snakeGame();
            snakeGame1.difficultyLevel = 2;
            snakeGame.DifficultyEffect(snakeGame1.difficultyLevel, ref snakeGame1.sleepTime, ref snakeGame1.snakeLengthInit, ref snakeGame1.numberOfObstaclesInit, ref snakeGame1.foodDissapearTime, ref snakeGame1.bonusPoints);
            Assert.AreEqual(snakeGame1.numberOfObstaclesInit, 10);
            Assert.AreEqual(snakeGame1.snakeLengthInit, 12);

            snakeGame1.difficultyLevel = 1;
            snakeGame.DifficultyEffect(snakeGame1.difficultyLevel, ref snakeGame1.sleepTime, ref snakeGame1.snakeLengthInit, ref snakeGame1.numberOfObstaclesInit, ref snakeGame1.foodDissapearTime, ref snakeGame1.bonusPoints);
            Assert.AreEqual(snakeGame1.numberOfObstaclesInit, 5);
            Assert.AreEqual(snakeGame1.snakeLengthInit, 7);
        }

        [TestMethod]
        public void DetermineUserMenuChoiceTest()
        {
            snakeGame snakeGame1 = new snakeGame();

            snakeGame1.fUserChoice = 0;
            Assert.AreEqual(snakeGame.DetermineUserMenuChoice(snakeGame1.fUserChoice), true);

            snakeGame1.fUserChoice = 1;
            Assert.AreEqual(snakeGame.DetermineUserMenuChoice(snakeGame1.fUserChoice), false);

            snakeGame1.fUserChoice = 2;
            Assert.AreEqual(snakeGame.DetermineUserMenuChoice(snakeGame1.fUserChoice), false);

            snakeGame1.fUserChoice = 3;
            Assert.AreEqual(snakeGame.DetermineUserMenuChoice(snakeGame1.fUserChoice), false);


        }
    }
}
