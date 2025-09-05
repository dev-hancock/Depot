namespace Depot.Auth.Tests.Setup;

public static partial class DatabaseExtensions
{
    public static async Task SeedAsync(this TestFixture fixture, params User[] users)
    {
        await fixture.Db.With(async context =>
        {
            foreach (var user in users)
            {
                await context.AddAsync(user);

                foreach (var session in user.Sessions)
                {
                    await fixture.Cache.SetSessionAsync(session.Id, session.Version);
                }
            }
        });
    }
}