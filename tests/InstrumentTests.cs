using NUnit.Framework;
using PlayLiveInstruments.Models;

namespace PlayLiveInstruments.Tests
{
    [TestFixture]
    public class InstrumentTests
    {
        [Test]
        public void CreateInstrument_ValidParameters_ShouldCreateInstrument()
        {
            // Arrange
            var name = "Guitar";
            var type = "String";
            var price = 299.99m;

            // Act
            var instrument = new Instrument
            {
                Name = name,
                Type = type,
                Price = price
            };

            // Assert
            Assert.AreEqual(name, instrument.Name);
            Assert.AreEqual(type, instrument.Type);
            Assert.AreEqual(price, instrument.Price);
        }

        [Test]
        public void CreateInstrument_EmptyName_ShouldThrowArgumentException()
        {
            // Arrange
            var instrument = new Instrument();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => instrument.Name = string.Empty);
        }

        [Test]
        public void CreateInstrument_NegativePrice_ShouldThrowArgumentException()
        {
            // Arrange
            var instrument = new Instrument();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => instrument.Price = -1);
        }
    }
}