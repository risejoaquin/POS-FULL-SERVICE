with open('PosCore.Tests/ViewModels/MainViewModelTests.cs', 'r') as f:
    c = f.read()

c = c.replace("new MainViewModel(dbContext, mockApiService.Object, settings, syncService, ticketPrinter, sessionManager)", "new MainViewModel(dbContext, mockApiService.Object, settings, syncService, ticketPrinter, sessionManager, new Moq.Mock<PosApplication.Interfaces.Local.IInventoryService>().Object)")

with open('PosCore.Tests/ViewModels/MainViewModelTests.cs', 'w') as f:
    f.write(c)
