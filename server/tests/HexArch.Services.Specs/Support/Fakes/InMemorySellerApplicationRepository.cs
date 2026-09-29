using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;
using Moq;

namespace HexArch.Services.Specs.Support.Fakes
{
    // A Mock<ISellerApplicationRepository> backed by an in-memory list, so scenario steps can
    // seed and assert on stored applications while the handler talks to the mocked port.
    public class InMemorySellerApplicationRepository
    {
        private readonly List<SellerApplication> applications = new();
        private readonly Mock<ISellerApplicationRepository> mock = new();

        public InMemorySellerApplicationRepository()
        {
            mock.Setup(repository => repository.AddApplication(It.IsAny<SellerApplication>()))
                .Callback<SellerApplication>(application =>
                {
                    // Mirrors EF Core assigning the key on Add.
                    application.Id = Guid.NewGuid();
                    applications.Add(application);
                })
                .Returns(Task.CompletedTask);

            mock.Setup(repository => repository.UpdateApplication(It.IsAny<SellerApplication>()))
                .Callback<SellerApplication>(application =>
                {
                    var index = applications.FindIndex(a => a.Id == application.Id);
                    if (index >= 0)
                        applications[index] = application;
                })
                .Returns(Task.CompletedTask);

            mock.Setup(repository => repository.GetApplication(It.IsAny<Guid>()))
                .Returns<Guid>(id => Task.FromResult(applications.FirstOrDefault(a => a.Id == id)));

            mock.Setup(repository => repository.GetPendingApplicationForUser(It.IsAny<Guid>()))
                .Returns<Guid>(userId => Task.FromResult(applications.FirstOrDefault(
                    a => a.UserId == userId && a.Status == SellerApplicationStatus.Pending)));
        }

        public ISellerApplicationRepository Object => mock.Object;
        public IReadOnlyList<SellerApplication> Applications => applications;

        public void Seed(SellerApplication application) => applications.Add(application);
    }
}
