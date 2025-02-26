using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Carica la chiave segreta da variabile d'ambiente o da configurazione
var secretKey = "ChiaveSegretaMoltoLungaCheSupera32Byte";

// Configura l'autenticazione JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configuriamo l'evento per estrarre il token dal cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Leggiamo il cookie "auth_token"
                var token = context.Request.Cookies["auth_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };

        // Impostazioni di validazione del token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))
        };
    });

// Abilita CORS per permettere richieste dal client statico
//CORS:
// È il meccanismo che permette alle risorse di un dominio (ad esempio, il nostro server API)
// di essere richieste da un altro dominio (il nostro frontend statico).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
                        .AllowAnyHeader()       // Consente qualsiasi header nella richiesta, utile per non bloccare richieste con header personalizzati.
                        .AllowAnyMethod()       // Permette qualsiasi metodo HTTP (GET, POST, PUT, DELETE, ecc.).
                        .AllowCredentials());   // Permette l'invio dei cookie (e altre credenziali) nelle richieste cross-origin.
});

//Registra i controller (cioè le classi che gestiscono le richieste HTTP) nel container dei servizi.
//Questo è necessario affinché il framework sappia quali endpoint sono disponibili.
builder.Services.AddControllers();

//Costruisce l'applicazione a partire dalle configurazioni fatte finora.
//Questo genera l'oggetto app che rappresenta la pipeline HTTP.
var app = builder.Build();

// (Opzionale) Forza HTTPS se necessario
// app.UseHttpsRedirection();

// Applica la policy CORS definita
app.UseCors("AllowFrontend");


// Attiva il middleware per l’autenticazione.
//Questo middleware esaminerà ogni richiesta per verificare
//la presenza e la validità del token JWT 
//(o, nel nostro caso, del cookie contenente il token)
app.UseAuthentication();

// Mappa i controller
app.MapControllers();


//Mappa (cioè collega) i controller agli endpoint HTTP.
//Questo significa che ogni azione definita nei controller
//verrà resa disponibile come endpoint a cui il client può fare richieste.
app.Run();