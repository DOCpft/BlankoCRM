using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

/// <summary>
/// Аутентификация и регистрация пользователей (без JWT‑авторизации).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Зарегистрировать нового пользователя и организацию.
    /// </summary>
    /// <param name="request">Данные для регистрации.</param>
    /// <response code="201">Пользователь успешно зарегистрирован.</response>
    /// <response code="400">Некорректные входные данные (ошибки валидации).</response>
    /// <response code="409">Пользователь с таким email или названием организации уже существует.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _authService.RegisterAsync(request.Name, request.Email, request.Password, request.OrganizationName);
            return StatusCode(StatusCodes.Status201Created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Войти в систему и получить JWT‑токен.
    /// </summary>
    /// <param name="request">Данные для входа (email и пароль).</param>
    /// <response code="200">Успешный вход, возвращает токен и данные пользователя.</response>
    /// <response code="400">Некорректные входные данные (ошибки валидации).</response>
    /// <response code="401">Неверный email или пароль.</response>
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var (token, name, email) = await _authService.LoginAsync(request.Email, request.Password);
            return Ok(new AuthResponse(token, name, email));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}