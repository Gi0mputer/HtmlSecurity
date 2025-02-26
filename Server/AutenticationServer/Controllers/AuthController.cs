using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;

namespace AutenticationServer.Controllers
{
    // Indichiamo il prefisso per tutte le route: api/auth
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // La chiave segreta usata per firmare e validare il JWT.
        // Deve essere la stessa configurata in Program.cs
        private readonly string _secretKey = "ChiaveSegretaMoltoLungaCheSupera32Byte";

        // Endpoint POST per il login: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin user)
        {
            // Controllo molto semplice: credenziali hardcoded (utente/password)
            if (user.Username == "utente" && user.Password == "password")
            {
                // Creiamo un'istanza del token handler per JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                // Convertiamo la chiave segreta in un array di byte
                var key = Encoding.ASCII.GetBytes(_secretKey);

                // Creiamo la descrizione del token, che include:
                // - Il subject: un ClaimsIdentity con il claim del nome utente
                // - La scadenza: 1 ora dalla creazione
                // - Le credenziali di firma: usiamo la chiave segreta e l'algoritmo HMAC-SHA256
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Username)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                // Creiamo il token vero e proprio basato sulla descrizione
                var token = tokenHandler.CreateToken(tokenDescriptor);
                // Scriviamo il token in formato stringa
                var tokenString = tokenHandler.WriteToken(token);

                // Non impostiamo più il token in un cookie, ma lo restituiamo nel body della risposta.
                return Ok(new { token = tokenString });
            }

            // Se le credenziali non corrispondono, restituiamo Unauthorized
            return Unauthorized();
        }

        // Endpoint GET per controllare se il token è valido: api/auth/check
        [HttpGet("check")]
        public IActionResult CheckAuth()
        {
            // Verifichiamo se la request contiene l'header "Authorization"
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Unauthorized(new { message = "Token mancante" });
            }

            // Estraiamo il valore dell'header
            string authHeader = Request.Headers["Authorization"];
            // Controlliamo che l'header inizi con "Bearer "
            if (!authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Formato del token non valido" });
            }

            // Rimuoviamo la parte "Bearer " per ottenere il token puro
            string token = authHeader.Substring("Bearer ".Length).Trim();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            try
            {
                // Validiamo il token usando i TokenValidationParameters:
                // - Non validiamo Issuer e Audience (false) perché non li abbiamo impostati.
                // - Validiamo il Lifetime per controllare che il token non sia scaduto.
                // - Validiamo la firma con la chiave segreta.
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                // Se la validazione va a buon fine, restituiamo OK
                return Ok(new { message = "Token valido" });
            }
            catch (Exception ex)
            {
                // Se la validazione fallisce (token scaduto, manomesso, ecc.), restituiamo Unauthorized
                return Unauthorized(new { message = "Token non valido", error = ex.Message });
            }
        }
    }

    // Classe usata per deserializzare il corpo della richiesta di login
    public class UserLogin
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
