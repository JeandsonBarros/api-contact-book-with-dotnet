using System.Buffers.Text;
using System.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ContactBook.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ContactBook.Models;
using System.Net.Mail;
using System.Net;
using System.ComponentModel;
using ContactBook.Context;

namespace Integrando_APIs_NET_C__com_Entity_Framework.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UserAplication> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationContext _applicationContext;

        public AuthController(
            IConfiguration configuration,
            UserManager<UserAplication> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationContext applicationContext
            )
        {
            _configuration = configuration;
            _roleManager = roleManager;
            _userManager = userManager;
            _applicationContext = applicationContext;

        }

        /// <summary> User register </summary>
        /// <returns> Returns the user created </returns>
        /// <response code="400"> If any field is missing, invalid or there is already a user with the registered email </response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<UserAplication>> CreateUserAsync([FromBody] UserDto userDto)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(userDto.Email);
                if (userExists is not null)
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        new { Success = false, Message = "User already exists!" }
                    );
                }

                UserAplication user = new()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Email = userDto.Email,
                    UserName = userDto.Email,
                    Name = userDto.Name
                };

                var result = await _userManager.CreateAsync(user, userDto.Password);

                if (!result.Succeeded)
                {
                    Console.WriteLine(result.Errors);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error creating user", result.Errors });
                }

                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new("Admin"));
                }

                await _userManager.AddToRoleAsync(user, "Admin");

                user.PasswordHash = "";
                user.SecurityStamp = "";
                user.ConcurrencyStamp = "";

                return Created(nameof(LoginAsync), user);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error registering user!" });
            }
        }

        /// <summary> User login </summary>
        /// <returns> Return a token </returns>
        /// <response code="401"> If the credentials are wrong </response>
        /// <response code="400"> If any field is missing or invalid </response>
        /// <response code="404"> If user not exists </response>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto userDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userDto.Email);

                if (user is not null && await _userManager.CheckPasswordAsync(user, userDto.Password))
                {
                    string token = await GetTokenAsync(user);
                    return Ok(new { Data = token });
                }
                else if (user == null)
                {
                    return NotFound(new { Message = $"There is not even an account with email: {userDto.Email}" });
                }

                return Unauthorized(new { Message = "Incorrect password" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Login error!" });
            }
        }

        /// <summary> Get account data that is authenticated </summary>
        /// <returns> Return account data </returns>
        /// <response code="401"> If not authenticated </response>
        [Authorize]
        [HttpGet("account-data")]
        public async Task<ActionResult<UserAplication>> GetAccountData()
        {
            try
            {
                var id = User?.Identity?.Name;
                var user = await _userManager.FindByIdAsync(id);
                var userRoles = await _userManager.GetRolesAsync(user);

                user.PasswordHash = "";
                user.SecurityStamp = "";
                user.ConcurrencyStamp = "";

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error geting account data!" });
            }
        }

        /// <summary> Fully update user account data </summary>
        /// <returns> Returns updated account data </returns>
        /// <response code="401"> If not authenticated </response>
        /// <response code="400"> If any fields are missing or invalid </response>
        [Authorize]
        [HttpPut("account-update")]
        public async Task<ActionResult<UserAplication>> PutUpdateAccount([FromBody] UserDto userDto)
        {
            try
            {
                UserDtoViewModel userDtoViewModel = new()
                {
                    Email = userDto.Email,
                    Name = userDto.Name,
                    Password = userDto.Password
                };

                var user = await UpdateUser(userDtoViewModel);

                return Ok(user);
            }
            catch (BadHttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error updating account!" });
            }
        }

        /// <summary> Partially update user account data </summary>
        /// <returns> Returns updated account data </returns>
        /// <response code="401"> If not authenticated </response>
        /// <response code="400"> If any fields are missing or invalid </response>
        [Authorize]
        [HttpPatch("account-update")]
        public async Task<IActionResult> PatchUpdateAccount([FromBody] UserDtoViewModel userDtoViewModel)
        {
            try
            {
                await UpdateUser(userDtoViewModel);
                return Ok(new { message = "Update success" });
            }
            catch (BadHttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error updating account!" });
            }
        }

        /// <summary> Admin: Get all users </summary>
        /// <returns> Return a page of users </returns>
        /// <response code="401"> if unauthenticated </response>
        /// <response code="403"> if non-admin  </response>
        [Authorize(Roles = "Admin")]
        [HttpGet("list-all-users")]
        public ActionResult<PageResponse<List<UserAplication>>> GetAllUsers([FromQuery] Pagination pagination)
        {
            try
            {
                var validPagination = new Pagination(pagination.Page, pagination.Size);

                var users = _userManager.Users
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                foreach (var user in users)
                {
                    user.PasswordHash = "";
                    user.SecurityStamp = "";
                    user.ConcurrencyStamp = "";
                }

                var TotalRecords = _userManager.Users.Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}/api/auth/list-all-users";

                PageResponse<List<UserAplication>> pagedResponse = new(
                    data: users,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: TotalRecords,
                    uri: baseUri
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error geting users!" });
            }
        }

        /// <summary> Admin: Find user by email </summary>
        /// <returns> Return a page of users </returns>
        /// <response code="401"> if unauthenticated </response>
        /// <response code="403"> if non-admin  </response>
        [Authorize(Roles = "Admin")]
        [HttpGet("find-user-by-email/{email}")]
        public ActionResult<PageResponse<List<UserAplication>>> FindUserByEmail([FromQuery] Pagination pagination, string email)
        {
            try
            {
                var validPagination = new Pagination(pagination.Page, pagination.Size);

                var users = _userManager.Users
                    .Where(user => user.Email.Contains(email))
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                foreach (var user in _userManager.Users)
                {
                    user.PasswordHash = "";
                    user.SecurityStamp = "";
                    user.ConcurrencyStamp = "";
                }

                var usersSelect = users;

                var TotalRecords = _userManager.Users.Where(user => user.Email.Contains(email)).Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}/api/auth/find-user-by-email/{email}";

                PageResponse<List<UserAplication>> pagedResponse = new(
                    data: users,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: TotalRecords,
                    uri: baseUri
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error geting users!" });
            }
        }

        /// <summary> Authenticated user delete own account </summary>
        /// <response code="204"> If account deleted success </response>
        /// <response code="401"> if unauthenticated </response>
        [Authorize]
        [HttpDelete("delete-account")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var id = User?.Identity?.Name;

                var user = await _userManager.FindByIdAsync(id);
                await _userManager.DeleteAsync(user);

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error deleting account!" });
            }
        }

        /// <summary> Send forgotten password reset code to email </summary>
        /// <returns> Returns message notifying that the email was sent </returns>
        /// <response code="404"> If there is no user with the entered email </response>
        /// <response code="400"> If any field is missing or invalid </response>
        [HttpPost("forgotten-password/send-email-code")]
        public async Task<IActionResult> SendEmailCode(EmailToDto emailToDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(emailToDto.Email);
                if (user == null)
                {
                    return NotFound(new { Message = $"Email user {emailToDto.Email} not found!" });
                }

                Random random = new Random();
                long code = random.Next(1000000, 2000000);

                CodeForChangeForgottenPassword changeForgottenPassword = new()
                {
                    Code = code,
                    UserAplicationId = user.Id
                };
                await _applicationContext.CodeForChangeForgottenPassword.AddAsync(changeForgottenPassword);
                await _applicationContext.SaveChangesAsync();

                var client = new SmtpClient(_configuration["Email:Host"], Convert.ToInt32(_configuration["Email:Port"]))
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_configuration["Email:EmailFrom"], _configuration["Email:PasswordFrom"])
                };

                await client.SendMailAsync(
                     new MailMessage(
                         from: _configuration["Email:EmailFrom"],
                         to: emailToDto.Email,
                         subject: "Contact System - Your code reset forgotten password",
                         body: $"Your password reset code is {code}, valid for 15 minutes."
                     ));

                string uri = $"{Request.Scheme}://{Request.Host}/api/auth/forgotten-password/change-password";

                return Ok(new
                {
                    Message = $"Code send to {emailToDto.Email}",
                    Description = $"Use code in {uri}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "Error sending email!",
                        Description = ex.Message
                    });

            }
        }

        /// <summary> Reset password using code sent to email </summary>
        /// <returns> Returns message notifying that the password was successfully reset </returns>
        /// <response code="404"> If the entered code is incorrect or does not exist, or if there is no user with the entered email </response>
        /// <response code="400"> If any fields are missing, invalid, or the code is leaked </response>
        [HttpPut("forgotten-password/change-password")]
        public async Task<IActionResult> ChangeFogottenPassword(ChangeForgottenPasswordDto changeForgottenPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(changeForgottenPasswordDto.Email);
                if (user == null)
                {
                    return NotFound(new { Message = $"Email user {changeForgottenPasswordDto.Email} not found!" });
                }

                var codeForChangeForgottenPassword = _applicationContext.CodeForChangeForgottenPassword
                                                    .Where(x => x.Code == changeForgottenPasswordDto.Code
                                                     && x.UserAplicationId == user.Id).FirstOrDefault();
                if (codeForChangeForgottenPassword == null)
                {
                    return NotFound(new { Message = "Code entered does not exist!" });
                }

                if (DateTime.UtcNow > codeForChangeForgottenPassword.CodeExpires)
                {
                    _applicationContext.CodeForChangeForgottenPassword.Remove(codeForChangeForgottenPassword);
                    _applicationContext.SaveChanges();

                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        new { Message = "The maximum time in the code has expired" }
                    );
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultUpdatePassword = await _userManager.ResetPasswordAsync(user, token, changeForgottenPasswordDto.NewPassword);

                if (!resultUpdatePassword.Succeeded)
                {
                    return StatusCode(
                         StatusCodes.Status400BadRequest,
                         new
                         {
                             Message = "The maximum time in the code has expired",
                             Erros = resultUpdatePassword.Errors
                         }
                     );
                }

                return Ok(new { Message = "Password successfully updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "Error updating password!",
                        Description = ex.Message
                    });

            }
        }

        /* Generate token jwt */
        private async Task<string> GetTokenAsync(UserAplication user)
        {

            var authClaims = new List<Claim>
            {
                new (ClaimTypes.Name, user.Id),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        /* Function to update the user, used for PutUpdateAccount() and PatchUpdateAccount() */
        private async Task<UserAplication> UpdateUser(UserDtoViewModel userDtoViewModel)
        {

            var id = User?.Identity?.Name;
            var userLogged = await _userManager.FindByIdAsync(id);

            if (!userDtoViewModel.Name.IsNullOrEmpty())
            {
                userLogged.Name = userDtoViewModel.Name;
            }

            if (!userDtoViewModel.Email.IsNullOrEmpty())
            {
                string strModel = "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(userDtoViewModel.Email, strModel))
                {
                    throw new BadHttpRequestException("Email must be well-formed");
                }

                var userExists = await _userManager.FindByEmailAsync(userDtoViewModel.Email);
                if (userExists is not null && userExists.Email.ToString() != userLogged.Email)
                {
                    throw new BadHttpRequestException($"User with {userDtoViewModel.Email} already exists!");
                }

                userLogged.Email = userDtoViewModel.Email;
                userLogged.UserName = userDtoViewModel.Email;
            }

            var resultUpdateData = await _userManager.UpdateAsync(userLogged);

            if (!resultUpdateData.Succeeded)
            {
                string errorMessage = "Error updating data account! ";
                foreach (var erro in resultUpdateData.Errors)
                {
                    errorMessage += erro.Description;
                }
                throw new Exception(errorMessage);
            }

            if (!userDtoViewModel.Password.IsNullOrEmpty())
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(userLogged);
                var resultUpdatePassword = await _userManager.ResetPasswordAsync(userLogged, token, userDtoViewModel.Password);

                if (!resultUpdatePassword.Succeeded)
                {
                    Console.WriteLine(resultUpdatePassword.Errors);
                    throw new Exception("Error update password!");
                }
            }

            userLogged.PasswordHash = "";
            userLogged.SecurityStamp = "";
            userLogged.ConcurrencyStamp = "";

            return userLogged;

        }


    }
}