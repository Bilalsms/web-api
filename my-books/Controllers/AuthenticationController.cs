using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using my_books.Data;
using my_books.Data.Models;
using my_books.Data.ViewModels.Authentication;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace my_books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext dbContext;
        private readonly IConfiguration configuration;

        // refresh tokens
        private readonly TokenValidationParameters tokenValidationParameters; 

        public AuthenticationController( UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            AppDbContext dbContext, 
            IConfiguration configuration,
            TokenValidationParameters tokenValidationParameters
            )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.tokenValidationParameters = tokenValidationParameters;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> Register([FromBody] RegisterVM payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all required fields");
            }

            var userExists = await userManager.FindByEmailAsync(payload.Email);
            if(userExists != null)
            {
                return BadRequest($"User {payload.Email} already exists");
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                Email = payload.Email,
                UserName = payload.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(newUser, payload.Password);

            if (!result.Succeeded)
            {
                return BadRequest("User could not be created");
            }

            switch (payload.Role)
            {
                case "Admin":
                    await userManager.AddToRoleAsync(newUser, UserRoles.Admin);
                    break;

                case "Publisher":
                    await userManager.AddToRoleAsync(newUser, UserRoles.Publisher);
                    break;

                case "Author":
                    await userManager.AddToRoleAsync(newUser, UserRoles.Author);
                    break;

                case "User":
                    await userManager.AddToRoleAsync(newUser, UserRoles.User);
                    break;

                default:
                    break;
            }
            return Created(nameof(Register), $"User {payload.Email} created");
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> Login([FromBody] LoginVM payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all required fields");
            }

            var user = await userManager.FindByEmailAsync(payload.Email);

            if(user != null && await userManager.CheckPasswordAsync(user, payload.Password))
            {
                var tokenValue = await GenerateJwtTokenAsync(user, "");
                return Ok(tokenValue);
            }
            return Unauthorized();
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVM payload)
        {
            try
            {
                var result = await VerifyAndGenerateTokenAsync(payload);

                if (result == null) return BadRequest("Invalid tokens");
                return Ok(result);


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<AuthResultVM> VerifyAndGenerateTokenAsync(TokenRequestVM payload)
        {
            try
            {
                var jtwTokenHandler = new JwtSecurityTokenHandler();

                //Validation - Check 1: Check JWT Format
                var tokenInVerification = jtwTokenHandler.ValidateToken(payload.Token, tokenValidationParameters, out var validatedToken);

                // Check 2: Encryption Algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (result == false) return null;
                }

                // Check 3: Validate Expiry date
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTimeInUTC(utcExpiryDate);

                if (expiryDate > DateTime.UtcNow) throw new Exception("Token has not expired yet!");

                // Check 4: Refresh Token Exist in The Database
                var dbRefreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(n => n.Token == payload.RefreshToken);

                if (dbRefreshToken == null) throw new Exception("Refresh token does not exist in our DB");
                else
                {
                    // Check 5 Validate Id
                    var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                    if (dbRefreshToken.JwtId != jti) throw new Exception("Token does not match");

                    //Check 6
                    if (dbRefreshToken.DateExpire <= DateTime.UtcNow) throw new Exception("Your refresh token has expired, please re-authenticate");

                    // Check 7
                    if (dbRefreshToken.IsRevoked) throw new Exception("Refresh token is revoked");

                    // Generate new token (with existing refresh token)
                    var dbUserData = await userManager.FindByIdAsync(dbRefreshToken.UserId);

                    var newTokenRespone = GenerateJwtTokenAsync(dbUserData, payload.RefreshToken);

                    return await newTokenRespone;

                }
            }
            catch (SecurityTokenExpiredException ex)
            {
                var dbRefreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(n => n.Token == payload.RefreshToken);

                // Generate new token (with existing refresh token)
                var dbUserData = await userManager.FindByIdAsync(dbRefreshToken.UserId);

                var newTokenRespone = GenerateJwtTokenAsync(dbUserData, payload.RefreshToken);

                return await newTokenRespone;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private DateTime UnixTimeStampToDateTimeInUTC(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp);
            return dateTimeVal;
        }

        private async Task<AuthResultVM> GenerateJwtTokenAsync(ApplicationUser user, string existingRefreshToken)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add user roles

            var userRoles = await userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                expires: DateTime.UtcNow.AddMinutes(10),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = new RefreshToken();


            if (string.IsNullOrEmpty(existingRefreshToken))
            {
                refreshToken = new RefreshToken()
                {
                    JwtId = token.Id,
                    IsRevoked = false,
                    UserId = user.Id,
                    DateAdded = DateTime.UtcNow,
                    DateExpire = DateTime.UtcNow.AddMonths(6),
                    Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
                };


                await dbContext.RefreshTokens.AddAsync(refreshToken);
                await dbContext.SaveChangesAsync();
                 
            }


            var response = new AuthResultVM()
            {
                Token = jwtToken,
                RefreshToken = (string.IsNullOrEmpty(existingRefreshToken)) ? refreshToken.Token : existingRefreshToken,
                ExpiresAt = token.ValidTo
            };
             
            return response;
        }
    }
}
