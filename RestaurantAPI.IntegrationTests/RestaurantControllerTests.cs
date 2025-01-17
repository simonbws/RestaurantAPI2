﻿using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using Microsoft.Extensions.DependencyInjection;
using RestaurantAPI.Models;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Security.Cryptography.X509Certificates;
namespace RestaurantAPI.IntegrationTests

{
    public class RestaurantControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;
        public RestaurantControllerTests(WebApplicationFactory<Startup> factory)
        {

            _factory = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContextOptions = services
                            .SingleOrDefault(service => service.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));
                        services.Remove(dbContextOptions);
                        //na tym etapie pozbylismy sie instniejacej rejestracji DbConxtextu, mozemy ja zastapic InMemory DbContext, ktora nie jest baza danych ale jej implementacja
                        services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>(); //dzieki tej linijce podczas procesowania zapytania ktore wymaga autentykacji czyli na endpoint z atrybutem authorize, to wykonanie takiej ewaluacji dostanie oddelegowane do fakepolicyevaluatior
                        services.AddMvc(option => option.Filters.Add(new FakeUserFilter()));

                        services.AddDbContext<RestaurantDbContext>(options => options.UseInMemoryDatabase("RestaurantDb"));
                        //teraz, nasze api ktore potrzebujemy do testow, nie bedzie korzystac z baz danych mssql tylko inmemory
                    });
                });
            _client = _factory.CreateClient();
        }
        [Theory]
        [InlineData("pageSize=5&pageNumber=1")]
        [InlineData("pageSize=15&pageNumber=2")]
        [InlineData("pageSize=10&pageNumber=3")]
        public async Task GetAll_WithQueryParameters_ReturnsOkResult(string queryParams)
        {
           
            // act
            var response =  await _client.GetAsync("/api/restaurant?" + queryParams);
            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
       
        [Fact]
        public async Task Delete_ForRestaurantOwner_NoContent()
        {
            //arrange
            

            var restaurant = new Restaurant()
            {
                CreatedById = 1,
                Name = "Test"
            };

            //seed
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<RestaurantDbContext>();


            _dbContext.Restaurants.Add(restaurant);
            _dbContext.SaveChanges();
            // act
            var response = await _client.DeleteAsync("/api/restaurant/" + restaurant.Id);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ForNonExistingRestaurant_ReturnsNotFound()
        {
            // act
            var response = await _client.DeleteAsync("/api/restaurant/987");
            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
        [Fact]
        public async Task CreateRestaurant_WithValidModel_ReturnsCreatedStatus()
        {
            // arrange tworzymy przykladowymi model
            var model = new CreateRestaurantDto()
            {
                Name = "TestRestaurant",
                City = "Kraków",
                Street = "Długa 5"
            };
            ////serializujemy model do JSON
            //var json = JsonConvert.SerializeObject(model);
            ////wysylamy go jako zawartosc http na serwer
            //var httpContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            var httpContent = model.ToJsonHttpContent(); 
            // act
            var response = await _client.PostAsync("/api/restaurant", httpContent);

            // arrange
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }
        [Fact]
        public async Task CreateRestaurant_WithInvalidModel_ReturnsBadRequest()
        {
            // arrange
            var model = new CreateRestaurantDto()
            {
                ContactEmail = "test@test.com",
                Description = "test desc",
                ContactNumber = "323 322 323"
            };
            var httpContent = model.ToJsonHttpContent();
            // act
            var response = await _client.PostAsync("/api/restaurant", httpContent);
            // arrange
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            
        }



        [Theory]
        [InlineData("pageSize=100&pageNumber=3")]
        [InlineData("pageSize=11&pageNumber=1")]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetAll_WithInvalidQueryParams_ReturnsBadRequest400(string queryParams)
        {

            
            // act
            var response = await _client.GetAsync("/api/restaurant?" + queryParams);
            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
