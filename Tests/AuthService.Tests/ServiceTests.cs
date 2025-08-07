using AuthService.App.Exceptions;
using AuthService.App.Services;
using AuthService.Domain.Abstractions;
using AuthService.Domain.Models;
using AuthService.Infra.Security;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagementServices.Shared.AuthService.DTO;
using TaskManagementServices.Shared.Exceptions;

namespace AuthService.Tests
{
    public class ServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
        private readonly AuthService.App.Services.AuthService _authService;

        public ServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _authService = new AuthService.App.Services.AuthService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _tokenGeneratorMock.Object,
                LoggerFactory.Create(t=>t.AddDebug()).CreateLogger<AuthService.App.Services.AuthService>()
            );
        }

        // Регистрация: Успешный сценарий
        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsUserResponse()
        {
            
            var request = new RegisterRequest
            {
                Email = "email@test.com",
                FullName = "Some Name",
                Password = "SuperSecretPass"
            };

            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: false, user: null));

            _passwordHasherMock
                .Setup(hasher => hasher.HashPassword(request.Password))
                .Returns("hashed_password");

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal(request.FullName, result.FullName);
            _userRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<UserDomain>()), Times.Once);
        }

        // Регистрация: Дубликат email
        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsUserAlreadyExistsException()
        {
            
            var request = new RegisterRequest
            {
                Email = "email@test.com",
                FullName = "Some Name",
                Password = "SuperSecretPass"
            };

            var existingUser = new UserDomain { Email = request.Email };
            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: true, user: existingUser));


            await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _authService.RegisterAsync(request));
        }

        // Регистрация: Невалидный email
        [Theory]
        [InlineData("invalid-email")]
        [InlineData("no@dot")]
        [InlineData("@missinglocal.com")]
        [InlineData("invalid")]
        [InlineData("missing@dot")]
        [InlineData("@no-local.com")]
        [InlineData("double@@at.com")]
        [InlineData("no-tld@domain.")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RegisterAsync_InvalidEmail_ThrowsInvalidArgumentException(string email)
        {
            
            var request = new RegisterRequest
            {
                Email = email,
                FullName = "Some Name",
                Password = "SuperSecretPass"
            };


            await Assert.ThrowsAsync<InvalidArgumentException>(() => _authService.RegisterAsync(request));
        }

        // Логин: Успешная аутентификация (тут же и проверка валидных email)
        [Theory]
        [InlineData("valid@example.com")]
        [InlineData("name@domain.co.uk")]
        [InlineData("email@test.com")]
        public async Task LoginAsync_ValidCredentials_ReturnsToken(string email)
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = "SuperSecretPass"
            };

            var user = new UserDomain
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = "hashed_password"
            };

            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: true, user: user));

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(user.PasswordHash, request.Password))
                .Returns(true);

            _tokenGeneratorMock
                .Setup(gen => gen.GenerateToken(user))
                .Returns("jwt_token");

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal("jwt_token", result.Token);
        }

        // Логин: Неверный email
        [Fact]
        public async Task LoginAsync_InvalidEmail_ThrowsInvalidCredentialsException()
        {
            
            var request = new LoginRequest
            {
                Email = "email@test.com",
                Password = "SuperSecretPass"
            };

            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: false, user: null));


            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(request));
        }

        // Логин: Неверный пароль
        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsInvalidCredentialsException()
        {
            var request = new LoginRequest
            {
                Email = "email@test.com",
                Password = "SuperSecretPass"
            };

            var user = new UserDomain { PasswordHash = "hashed_password" };
            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: true, user: user));

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyPassword(user.PasswordHash, request.Password))
                .Returns(false);


            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(request));
        }

        // Получение пользователя: Успешное получение
        [Fact]
        public async Task GetUserAsync_ValidId_ReturnsUserResponse()
        {
            var userId = Guid.NewGuid();
            var user = new UserDomain
            {
                Id = userId,
                Email = "email@test.com",
                FullName = "Some Name"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var result = await _authService.GetUserAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.FullName, result.FullName);
            Assert.Equal(user.Id, result.UserId);
        }

        // Получение пользователя: Пользователь не найден
        [Fact]
        public async Task GetUserAsync_InvalidId_ThrowsNotFoundException()
        {
            var userId = Guid.NewGuid();
            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((UserDomain)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _authService.GetUserAsync(userId));
        }

        // Регистрация: Null запрос
        [Fact]
        public async Task RegisterAsync_NullRequest_ThrowsInvalidArgumentException()
        {
            await Assert.ThrowsAsync<InvalidArgumentException>(() => _authService.RegisterAsync(null));
        }

        // Логин: Null запрос
        [Fact]
        public async Task LoginAsync_NullRequest_ThrowsInvalidArgumentException()
        {
            await Assert.ThrowsAsync<InvalidArgumentException>(() => _authService.LoginAsync(null));
        }

        // Регистрация: Пустое полное имя
        [Fact]
        public async Task RegisterAsync_EmptyFullName_ThrowsInvalidArgumentException()
        {
            var request = new RegisterRequest
            {
                Email = "valid@test.com",
                FullName = "",
                Password = "password"
            };

            await Assert.ThrowsAsync<InvalidArgumentException>(() => _authService.RegisterAsync(request));
        }

        // Регистрация: Пароль из пробелов
        [Fact]
        public async Task RegisterAsync_WhitespacePassword_ThrowsInvalidArgumentException()
        {
            var request = new RegisterRequest
            {
                Email = "valid@test.com",
                FullName = "John Doe",
                Password = "   "
            };

            await Assert.ThrowsAsync<InvalidArgumentException>(() => _authService.RegisterAsync(request));
        }

        // Регистрация: Проверка вызова хеширования пароля
        [Fact]
        public async Task RegisterAsync_ValidRequest_CallsPasswordHasherWithCorrectPassword()
        {
            
            var request = new RegisterRequest
            {
                Email = "email@test.com",
                FullName = "Some Name",
                Password = "SuperSecretPass"
            };

            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: false, user: null));

            // Act
            await _authService.RegisterAsync(request);

            // Assert
            _passwordHasherMock.Verify(
                hasher => hasher.HashPassword(request.Password),
                Times.Once);
        }


        // Регистрация: Проверка корректности создания объекта пользователя
        [Fact]
        public async Task RegisterAsync_ValidRequest_CreatesCorrectUser()
        {
            var request = new RegisterRequest
            {
                Email = "email@test.com",
                FullName = "Some Name",
                Password = "SuperSecretPass"
            };

            UserDomain createdUser = null;
            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: false, user: null));

            _userRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<UserDomain>()))
                .Callback<UserDomain>(user => createdUser = user);

            await _authService.RegisterAsync(request);

            Assert.NotNull(createdUser);
            Assert.NotEqual(Guid.Empty, createdUser.Id);
            Assert.Equal(request.Email, createdUser.Email);
            Assert.Equal(request.FullName, createdUser.FullName);
            Assert.True(createdUser.CreatedAt > 0); // Проверка временной метки
        }

        // Регистрация: Очень длинный email
        [Fact]
        public async Task RegisterAsync_LongEmail_ThrowsInvalidArgumentException()
        {
            
            var longString = new string('a', 1000);
            var request = new RegisterRequest
            {
                Email = $"{longString}@test.com",
                FullName = "Name Name",
                Password = "Password"
            };

            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: false, user: null));

            await Assert.ThrowsAsync<InvalidArgumentException>(() => _authService.RegisterAsync(request));
        }

        // Регистрация: Очень длинное имя
        [Fact]
        public async Task RegisterAsync_LongFullName_ThrowsInvalidArgumentException()
        {
            
            var longString = new string('a', 1000);
            var request = new RegisterRequest
            {
                Email = $"test@test.com",
                FullName = longString,
                Password = "Password"
            };

            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: false, user: null));

            await Assert.ThrowsAsync<InvalidArgumentException>(() => _authService.RegisterAsync(request));
        }

        // Регистрация: Очень длинный пароль
        [Fact]
        public async Task RegisterAsync_LongPassword_ThrowsInvalidArgumentException()
        {
            
            var longString = new string('a', 1000);
            var request = new RegisterRequest
            {
                Email = $"test@test.com",
                FullName = "Name Name",
                Password = longString
            };

            _userRepositoryMock
                .Setup(repo => repo.TryGetByEmailAsync(request.Email))
                .ReturnsAsync((success: false, user: null));

            await Assert.ThrowsAsync<InvalidArgumentException>(() => _authService.RegisterAsync(request));
        }

        // Получение пользователя: Пустой GUID
        [Fact]
        public async Task GetUserAsync_EmptyGuid_ThrowsInvalidArgumentException()
        {
            var emptyGuid = Guid.Empty;
            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(emptyGuid))
                .ReturnsAsync((UserDomain)null);

            await Assert.ThrowsAsync<InvalidArgumentException>(()=>_authService.GetUserAsync(emptyGuid));

        }
    }
}
