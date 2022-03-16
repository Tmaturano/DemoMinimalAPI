using AutoMapper;
using DemoMinimalAPI.AutoMapper;
using DemoMinimalAPI.Data;
using DemoMinimalAPI.DTOs;
using DemoMinimalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniValidation;
using NetDevPack.Identity;
using NetDevPack.Identity.Jwt;
using NetDevPack.Identity.Model;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperConfig)));

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("DemoMinimalAPI")));

builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DeleteSupplier",
        policy => policy.RequireClaim("DeleteSupplier"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthConfiguration();
app.UseHttpsRedirection();

app.MapPost("/register", [AllowAnonymous] async (
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    IOptions<AppJwtSettings> appJwtSettings,
    RegisterUser registerUser) =>
{
    if (registerUser is null)
        return Results.BadRequest("User not sent");

    if (!MiniValidator.TryValidate(registerUser, out var errors))
        return Results.ValidationProblem(errors);

    var user = new IdentityUser
    {
        UserName = registerUser.Email,
        Email = registerUser.Email,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, registerUser.Password);
    if (!result.Succeeded)
        return Results.BadRequest(result.Errors);

    var jwt = new JwtBuilder()
                .WithUserManager(userManager)
                .WithJwtSettings(appJwtSettings.Value)
                .WithEmail(user.Email)
                .WithJwtClaims()
                .WithUserClaims()
                .WithUserRoles()
                .BuildUserResponse();

    return Results.Ok(jwt);
}).ProducesValidationProblem()
  .Produces(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status400BadRequest)
  .WithName("RegisterUser")
  .WithTags("User");


app.MapPost("/login", [AllowAnonymous] async (
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    IOptions<AppJwtSettings> appJwtSettings,
    LoginUser loginUser) =>
{
    if (loginUser == null)
        return Results.BadRequest("User not sent");

    if (!MiniValidator.TryValidate(loginUser, out var errors))
        return Results.ValidationProblem(errors);

    var result = await signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

    if (result.IsLockedOut)
        return Results.BadRequest("User blocked");

    if (!result.Succeeded)
        return Results.BadRequest("User or password invalid");

    var jwt = new JwtBuilder()
                .WithUserManager(userManager)
                .WithJwtSettings(appJwtSettings.Value)
                .WithEmail(loginUser.Email)
                .WithJwtClaims()
                .WithUserClaims()
                .WithUserRoles()
                .BuildUserResponse();

    return Results.Ok(jwt);
}).ProducesValidationProblem()
  .Produces(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status400BadRequest)
  .WithName("LoginUser")
  .WithTags("User");


app.MapGet("/suppliers", [AllowAnonymous] async (MinimalContextDb context) =>
    await context.Suppliers.ToListAsync())
  .Produces<IEnumerable<Supplier>>(StatusCodes.Status200OK)
  .Produces<IEnumerable<Supplier>>(StatusCodes.Status404NotFound)
  .WithName("GetSuppliers")
  .WithTags("Supplier");

app.MapGet("/supplier/{id}", [AllowAnonymous] async (Guid id, MinimalContextDb context) =>
    await context.Suppliers.FindAsync(id)
        is Supplier supplier
        ? Results.Ok(supplier)
        : Results.NotFound())
    .Produces<Supplier>(StatusCodes.Status200OK)
    .Produces<Supplier>(StatusCodes.Status404NotFound)
    .WithName("GetSupplierById")
    .WithTags("Supplier");

app.MapPost("/supplier", [Authorize] async (MinimalContextDb context, SupplierInputDto supplierInputDto, IMapper mapper) =>
{
    if (!MiniValidator.TryValidate(supplierInputDto, out var errors))
        return Results.ValidationProblem(errors);

    var supplier = mapper.Map<Supplier>(supplierInputDto);
    context.Suppliers.Add(supplier);

    var result = await context.SaveChangesAsync();
    return result > 0 
        ? Results.CreatedAtRoute("GetSupplierById", new { id = supplier.Id }, supplier)
        : Results.BadRequest("There was a problem when saving the data");
}).ProducesValidationProblem()
  .Produces<Supplier>(StatusCodes.Status201Created)
  .Produces<Supplier>(StatusCodes.Status400BadRequest)
  .WithName("PostSupplier")
  .WithTags("Supplier");


app.MapPut("/supplier/{id}", [Authorize] async (MinimalContextDb context, Guid id, SupplierInputDto supplierInputDto, IMapper mapper) =>
{
    var supplierFromDb = await context.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    if (supplierFromDb is null)
        return Results.NotFound();

    if (!MiniValidator.TryValidate(supplierInputDto, out var errors))
        return Results.ValidationProblem(errors);

    var supplier = mapper.Map<Supplier>(supplierInputDto);
    supplier.SetId(id);
    context.Suppliers.Update(supplier);

    var result = await context.SaveChangesAsync();
    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("There was a problem when updating the data");
}).ProducesValidationProblem()
  .Produces<Supplier>(StatusCodes.Status204NoContent)
  .Produces<Supplier>(StatusCodes.Status400BadRequest)
  .Produces<Supplier>(StatusCodes.Status404NotFound)
  .WithName("UpdateSupplier")
  .WithTags("Supplier");

app.MapDelete("/supplier/{id}", [Authorize] async (MinimalContextDb context, Guid id, IMapper mapper) =>
{
    var supplier = await context.Suppliers.FindAsync(id);
    if (supplier is null)
        return Results.NotFound();

    context.Suppliers.Remove(supplier);

    var result = await context.SaveChangesAsync();
    return result > 0
        ? Results.NoContent()
        : Results.BadRequest("There was a problem when deleting the data");
}).ProducesValidationProblem()
  .Produces<Supplier>(StatusCodes.Status204NoContent)
  .Produces<Supplier>(StatusCodes.Status400BadRequest)
  .Produces<Supplier>(StatusCodes.Status404NotFound)
  .RequireAuthorization("DeleteSupplier")
  .WithName("DeleteSupplier")
  .WithTags("Supplier");

app.Run();
