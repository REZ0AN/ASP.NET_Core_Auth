using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();

builder.Services.AddHttpContextAccessor();
// authentication service - for creating and setting the cookie
builder.Services.AddScoped<AuthService>();

var app = builder.Build();


// authentication middleware - for recognizing
app.Use((ctx, next)=> {

    var idp = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
    var protector = idp.CreateProtector("auth-cookie");
    var authCookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));
    
    var protectedPayload = authCookie.Split("=").Last();
    var payload = protector.Unprotect(protectedPayload);
    var username = payload.Split(":");
    var key = username[0];
    var value = username[1];

    var claims = new List<Claim>();
    claims.Add(new Claim(key, value));
    var identity = new ClaimsIdentity(claims);

    ctx.User = new ClaimsPrincipal(identity);

    return next();
});


app.MapGet("/username", (HttpContext ctx) => {

    return ctx.User.FindFirstValue("usr");
});

app.MapGet("/login", (AuthService auth) => {

    auth.SignIn("abir");
    return "ok";
});

app.Run();


public class AuthService {

    // to have the IDataProtectionProvider
    
    private readonly IDataProtectionProvider _idp;

    // to access the HttpContext
    private readonly IHttpContextAccessor _accessor;
    
    public AuthService(IDataProtectionProvider idp, IHttpContextAccessor accessor) {
        _idp = idp;
        _accessor = accessor;
    }

    public void SignIn(string username){
        var protector = _idp.CreateProtector("auth-cookie");
        _accessor.HttpContext.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:"+username)}";
    }


}