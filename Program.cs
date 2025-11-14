using Microsoft.AspNetCore.Authentication.JwtBearer; // (1) ≈÷«›…
using Microsoft.AspNetCore.Identity; // (2) ≈÷«›…
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // (3) ≈÷«›…
using System.Text;
using EmployeeApi.Data; // (4) ≈÷«›…

var builder = WebApplication.CreateBuilder(args);

// --- (√) ≈⁄œ«œ ﬁ«⁄œ… «·»Ì«‰«  (ﬂ„« ﬂ«‰) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- (») ≈÷«›… Œœ„… Identity ---
// ‰Œ»— «· ÿ»Ìﬁ √‰ Ì” Œœ„ IdentityUser («·„” Œœ„ «·«› —«÷Ì) Ê IdentityRole («·œÊ— «·«› —«÷Ì)
// Ê‰—»ÿÂ »ﬁ«⁄œ… »Ì«‰« ‰«
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- (Ã) ≈÷«›… Œœ„… «·„’«œﬁ… (Authentication) Ê ÂÌ∆… JWT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
    };
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- (œ) Â«„: ≈÷«›… «·‹ Middleware »«· — Ì» «·’ÕÌÕ ---
// (1) ÌÃ» √‰ ‰Œ»— «· ÿ»Ìﬁ √‰ "Ì Õﬁﬁ" „‰ «·ÂÊÌ…
app.UseAuthentication();
// (2) À„ Ì Õﬁﬁ „‰ "«·’·«ÕÌ« "
app.UseAuthorization();

app.MapControllers();

app.Run();