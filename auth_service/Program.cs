using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();

var app = builder.Build();
app.MapGet("/username", (HttpContext ctx, IDataProtectionProvider idp) => {
    
    var protector = idp.CreateProtector("auth-cookie");
    var authCookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));
    
    var protectedPayload = authCookie.Split("=").Last();
    var payload = protector.Unprotect(protectedPayload);
    var username = payload.Split(":");
    var key = username[0];
    var value = username[1];

    return value;
});

app.MapGet("/login", (HttpContext ctx, IDataProtectionProvider idp) => {

    var protector = idp.CreateProtector("auth-cookie");
    ctx.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:abir")}";

    return "ok";
});

app.Run();
