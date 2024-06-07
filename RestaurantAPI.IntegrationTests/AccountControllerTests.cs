using FluentAssertions;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantAPI.IntegrationTests
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient _client;
        private Mock<IAccountService> _accountServiceMock = new Mock<IAccountService>();

        public AccountControllerTests(WebApplicationFactory <Startup> factory)
        {
            _client = factory
                 .WithWebHostBuilder(builder =>
                 {
                     builder.ConfigureServices(services =>
                     {
                         var dbContextOptions = services
                             .SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
                         services.Remove(dbContextOptions);
                         //na tym etapie pozbylismy sie instniejacej rejestracji DbConxtextu, mozemy ja zastapic InMemory DbContext, ktora nie jest baza danych ale jej implementacja

                         services.AddSingleton<IAccountService>(_accountServiceMock.Object);


                         services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb"));
                         //teraz, nasze api ktore potrzebujemy do testow, nie bedzie korzystac z baz danych mssql tylko inmemory
                     });
                 })
                .CreateClient();
        }
        [Fact]
        public async Task Login_ForRegisterUser_ReturnsOk()
        {

            //mockowanie obiektu po to aby wartosc jwt zwracala jakas dowolna wartosc typu string
            //arrange

            _accountServiceMock
                .Setup(e => e.GenerateJwt(It.IsAny<LoginDto>()))
                .Returns("jwt");
            var loginDto = new LoginDto()
            {
                Email = "test@test.com",
                Password = "password123"
            };
            var httpContent = loginDto.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/account/login", httpContent);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }





        [Fact]
        public async Task RegisterUser_ForValidModel_ReturnsOk()
        {
            // arrange

            var registerUser = new RegisterUserDto()
            {
                Email = "test@test.com",
                Password = "password123",
                ConfirmPassword = "password123"

            };

            var httpContent = registerUser.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/account/register", httpContent);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        [Fact]
        public async Task RegisterUser_ForInValidModel_ReturnsBadRequest()
        {
            // arrange

            var registerUser = new RegisterUserDto()
            {
               
                Password = "password1233",
                ConfirmPassword = "password123"

            };

            var httpContent = registerUser.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/account/register", httpContent);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
