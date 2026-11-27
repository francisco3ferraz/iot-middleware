#:sdk Microsoft.NET.Sdk.Web

using Microsoft.AspNetCore.Identity;

// The default hasher ignores the user argument, so any string works
Console.WriteLine(new PasswordHasher<string>().HashPassword("admin", args[0]));
