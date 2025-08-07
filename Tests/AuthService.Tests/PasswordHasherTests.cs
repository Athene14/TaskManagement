using AuthService.Infra.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Tests
{
    public class PasswordHasherTests
    {
        private readonly IPasswordHasher _hasher;

        public PasswordHasherTests()
        {
            _hasher = new PasswordHasher();
        }

        [Fact]
        public void HashPassword_ShouldReturnNonEmptyString()
        {
            // Arrange
            var password = "securePassword123";

            // Act
            var hash = _hasher.HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void HashPassword_ShouldReturnDifferentHashesForSamePassword()
        {
            // Arrange
            var password = "securePassword123";

            // Act
            var hash1 = _hasher.HashPassword(password);
            var hash2 = _hasher.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
        {
            // Arrange
            var password = "securePassword123";
            var hash = _hasher.HashPassword(password);

            // Act
            var result = _hasher.VerifyPassword(hash, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
        {
            // Arrange
            var correctPassword = "securePassword123";
            var wrongPassword = "wrongPassword456";
            var hash = _hasher.HashPassword(correctPassword);

            // Act
            var result = _hasher.VerifyPassword(hash, wrongPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalseForEmptyPassword()
        {
            // Arrange
            var password = "securePassword123";
            var emptyPassword = string.Empty;
            var hash = _hasher.HashPassword(password);

            // Act
            var result = _hasher.VerifyPassword(hash, emptyPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalseForNullPassword()
        {
            // Arrange
            var password = "securePassword123";
            string nullPassword = null;
            var hash = _hasher.HashPassword(password);

            // Act
            var result = _hasher.VerifyPassword(hash, nullPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HashPassword_ShouldThrowForNullPassword()
        {
            // Arrange
            string nullPassword = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _hasher.HashPassword(nullPassword));
        }
    }
}
