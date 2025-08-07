using AuthService.App.Exceptions;
using AuthService.Domain.Abstractions;
using AuthService.Domain.Models;
using AuthService.Infra.Security;
using TaskManagementServices.Shared.AuthService;
using TaskManagementServices.Shared.AuthService.DTO;
using TaskManagementServices.Shared.Exceptions;

namespace AuthService.App.Services
{
    internal class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AuthService> _log;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator,
            ILogger<AuthService> log)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _log = log;
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            ValidateRegisterRequest(request);
            if ((await _userRepository.TryGetByEmailAsync(request.Email)).success)
                throw new UserAlreadyExistsException("Email already exists");

            var user = new UserDomain
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                FullName = request.FullName,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            _log.LogInformation("User {UserMail} - {UserFullName} registrated", request.Email, request.FullName);
            await _userRepository.CreateAsync(user);
            
            return new UserResponse()
            {
                Email = user.Email,
                FullName = user.FullName,
                UserId = user.Id
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            ValidateLoginRequest(request);

            var user = await _userRepository.TryGetByEmailAsync(request.Email);
            if (!user.success) throw new InvalidCredentialsException("Invalid Email");

            if (!_passwordHasher.VerifyPassword(user.user.PasswordHash, request.Password))
                throw new InvalidCredentialsException("Invalid password");

            _log.LogInformation("User {UserMail} logged in", request.Email);
            var token = _tokenGenerator.GenerateToken(user.user);

            return new LoginResponse()
            {
                UserId = user.user.Id,
                Token = token
            };
        }

        public async Task<UserResponse> GetUserAsync(Guid userId)
        {
            if (userId == Guid.Empty) throw new InvalidArgumentException("User id is not provided");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("User not found");

            return new UserResponse()
            {
                Email = user.Email,
                FullName = user.FullName,
                UserId = user.Id
            };
        }

        private void ValidateLoginRequest(LoginRequest request)
        {
            if (request == null) throw new InvalidArgumentException(nameof(request));
            if (string.IsNullOrEmpty(request.Email)) throw new InvalidArgumentException("Invalid email");
            if (string.IsNullOrEmpty(request.Password)) throw new InvalidArgumentException("Invalid password");
        }

        private void ValidateRegisterRequest(RegisterRequest request)
        {
            if (request == null) throw new InvalidArgumentException(nameof(request));
            ValidateEmail(request.Email);
            ValidatePassword(request.Password);
            ValidateFullName(request.FullName);
        }

        private void ValidateEmail(string email)
        {
            if (email.Length > 64) throw new InvalidArgumentException("Email is too long. Max: 64");
            if (email.Length < 5) throw new InvalidArgumentException($"Email is too short");
            if (!IsValidEmail(email)) throw new InvalidArgumentException("Invalid email");
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new InvalidArgumentException("Password is not provided");
            if (string.IsNullOrWhiteSpace(password)) throw new InvalidArgumentException("Full name is not provided!");
            if (password.Length < 6) throw new InvalidArgumentException("Password should be at least 6 symbols long");
            if (password.Length > 32) throw new InvalidArgumentException("Password is too long. Maximum length: 32");
        }

        private void ValidateFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) throw new InvalidArgumentException("Full name is not provided!");
            if (string.IsNullOrWhiteSpace(fullName)) throw new InvalidArgumentException("Full name is not provided!");
            if (fullName.Length > 64) throw new InvalidArgumentException("Full name is too long. Max: 64");
            if (fullName.Length < 2) throw new InvalidArgumentException($"Full name is too short");
        }

        // простая проверка, что email похож на email
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                int atIndex = email.IndexOf('@');
                if (atIndex <= 0 || atIndex != email.LastIndexOf('@'))
                    return false;

                string localPart = email.Substring(0, atIndex);
                string domain = email.Substring(atIndex + 1);

                if (string.IsNullOrWhiteSpace(localPart))
                    return false;

                // Проверка домена
                if (string.IsNullOrWhiteSpace(domain))
                    return false;

                // Проверка точки в домене и домена верхнего уровня (TLD)
                int lastDot = domain.LastIndexOf('.');
                if (lastDot <= 0 ||
                    lastDot == domain.Length - 1 ||
                    domain.Length - lastDot < 3)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
