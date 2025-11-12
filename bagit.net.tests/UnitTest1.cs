namespace bagit.net.tests
{
    public class BagitTests
    {
        [Fact]
        public void IsEquals_ReturnsTrue_WhenNumbersAreEqual()
        {
            // Arrange
            var bagit = new Bagit();
            int a = 5;
            int b = 5;

            // Act
            bool result = bagit.IsEquals(a, b);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEquals_ReturnsFalse_WhenNumbersAreNotEqual()
        {
            // Arrange
            var bagit = new Bagit();
            int a = 5;
            int b = 10;

            // Act
            bool result = bagit.IsEquals(a, b);

            // Assert
            Assert.False(result);
        }
    }
}

