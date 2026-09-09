using FluentAssertions;
using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KursuTV.UnitTests.Repositories;

public class GenericRepositoryTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"RepoTestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_ShouldPersistAndRetrieveEntity()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new GenericRepository<Tag>(context);
        var tag = new Tag { Name = "Yapay Zeka", Slug = "yapay-zeka" };

        // Act
        var addedTag = await repository.AddAsync(tag);
        await repository.SaveChangesAsync();

        var retrievedTag = await repository.GetByIdAsync(addedTag.Id);

        // Assert
        retrievedTag.Should().NotBeNull();
        retrievedTag!.Name.Should().Be("Yapay Zeka");
        retrievedTag.Slug.Should().Be("yapay-zeka");
    }

    [Fact]
    public async Task FindAsync_ShouldFilterEntitiesCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new GenericRepository<Tag>(context);
        await repository.AddAsync(new Tag { Name = "Teknoloji", Slug = "teknoloji" });
        await repository.AddAsync(new Tag { Name = "Spor", Slug = "spor" });
        await repository.AddAsync(new Tag { Name = "Ekonomi", Slug = "ekonomi" });
        await repository.SaveChangesAsync();

        // Act
        var matches = (await repository.FindAsync(t => t.Name.StartsWith("T"))).ToList();

        // Assert
        matches.Should().HaveCount(1);
        matches[0].Slug.Should().Be("teknoloji");
    }

    [Fact]
    public async Task Update_ShouldModifyEntityValues()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new GenericRepository<Tag>(context);
        var tag = new Tag { Name = "Eski İsim", Slug = "eski-isim" };
        await repository.AddAsync(tag);
        await repository.SaveChangesAsync();

        // Act
        tag.Name = "Yeni İsim";
        tag.Slug = "yeni-isim";
        repository.Update(tag);
        await repository.SaveChangesAsync();

        var updated = await repository.GetByIdAsync(tag.Id);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Yeni İsim");
        updated.Slug.Should().Be("yeni-isim");
    }

    [Fact]
    public async Task Delete_ShouldRemoveEntityFromDb()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new GenericRepository<Tag>(context);
        var tag = new Tag { Name = "Silinecek", Slug = "silinecek" };
        await repository.AddAsync(tag);
        await repository.SaveChangesAsync();

        // Act
        repository.Delete(tag);
        await repository.SaveChangesAsync();

        var deleted = await repository.GetByIdAsync(tag.Id);

        // Assert
        deleted.Should().BeNull();
    }
}
