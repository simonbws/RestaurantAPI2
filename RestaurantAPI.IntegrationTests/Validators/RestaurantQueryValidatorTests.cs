using RestaurantAPI.Models.Validators;
using RestaurantAPI.Models;

namespace RestaurantAPI.IntegrationTests.Validators
{
    public class RestaurantQueryValidatorTests
    {
        public void Validate_ForCorrectModel_ReturnsSuccess(RestaurantQuery model)
        {
            // arrange
            var validator = new RestaurantQueryValidator();


            // act
            var result = validator.TestValidate(model);

            // assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
