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
        public void SecondTest()
        {
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void ThirdTest()
        {
            Assert.AreEqual(1, 1);
        }
    }
}
