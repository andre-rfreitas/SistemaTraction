using Microsoft.EntityFrameworkCore;

namespace SistemaTraction.Application.Tests;

public static class TestDbContextFactory
{
    public static TestApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestApplicationDbContext(options);
    }
}
