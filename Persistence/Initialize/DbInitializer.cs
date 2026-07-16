using Domain.DbModels;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Initialize;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationContext>();
        var userRepository = services.GetRequiredService<IUserRepository>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var passwordHasher = services.GetRequiredService<IPasswordService>();
        var config = services.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync();

        var user = await userRepository.HasAdminAsync();

        var email = config["DefaultAdmin:Email"];
        var password = config["DefaultAdmin:Password"];

        if (!user && email is not null && password is not null)
        {
            var employeeSuperAdmin = new DbEmployee
            {
                Name = "string",
                Surname = "string",
                Patronymic = "string",
                PhoneNumber = "string",
                IsUser = true,
                Email = email
            };
            
            var superAdmin = new DbUser
            {

                Email = email,
                PasswordHash = passwordHasher.Hash(password),
                Role = UserRole.SuperAdmin,
                Employee = employeeSuperAdmin
            };

            await userRepository.AddAsync(superAdmin);
            await unitOfWork.SaveChangesAsync();
        }
    }
}