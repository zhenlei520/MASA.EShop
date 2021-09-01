using MASA.EShop.Services.Catalog.Application.CatalogTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MASA.EShop.Services.Catalog.Tests;
[TestClass]
public class CatalogTypeUnitTest
{
    [TestMethod]
    public void TestCreate()
    {
        //Arrange
        Mock<IUnitOfWork> uow = new();
        uow.Setup(u => u.CommitAsync(default)).Verifiable();

        Mock<ICatalogTypeRepository> catalogTypeRepository = new();
        catalogTypeRepository.Setup(r => r.CreateAsync(It.IsAny<CatalogType>())).Verifiable();

        CreateCatalogTypeCommand createCatalogTypeCommand = new() { Type = DateTime.Now.ToString("yyyyMMddHHmmss") };

        Mock<IEventBus> eventBus = new();
        eventBus
            .Setup(e => e.PublishAsync(It.IsAny<CreateCatalogTypeCommand>()))
            .Callback<CreateCatalogTypeCommand>(async cmd =>
            {
                await new CatalogTypeCommandHandler(catalogTypeRepository.Object).HandleAsync(createCatalogTypeCommand);
                await uow.Object.CommitAsync();
            });

        //Act
        eventBus.Object.PublishAsync(createCatalogTypeCommand);

        //Assert
        catalogTypeRepository.Verify(
            repo => repo.CreateAsync(It.Is<CatalogType>(catalogType => catalogType.Type == createCatalogTypeCommand.Type)),
            Times.Once,
            "CreateAsync must be called only once");
        uow.Verify(u => u.CommitAsync(default), Times.Once, "CommitAsync must be called only once");
    }
}