// Encapsulation: The KitchenService class hides the complexities of how it processes orders and manages inventory.
using System.Text;

public class KitchenService : IKitchenService
{
    private readonly IInventoryRepository _repository;

    public KitchenService(IInventoryRepository repository)
    {
        _repository = repository;
    }
    public string AddIngredients(IList<Ingredient> ingredients)
    {
        _repository.AddIngredients(ingredients);
        var addedIngredients = ingredients.Select(ingredient => $"Added {ingredient.Quantity} units of {ingredient.Name}").ToList();
        return string.Join("\r\n", addedIngredients);
    }

    public string RemoveIngredients(IList<Ingredient> ingredients)
    {
        _repository.RemoveIngredients(ingredients);
        var removedIngredients = ingredients.Select(ingredient => $"Removed {ingredient.Quantity} units of {ingredient.Name}").ToList();
        return string.Join("\r\n", removedIngredients);
    }

    public IOrderResponse GetOrder(IOrderRequest order)
    {
        IOrderResponse response = new OrderResponse();
        StringBuilder message = new StringBuilder();
        foreach (var foodQuantity in order.Quantities)
        {
            var food = foodQuantity.Key;
            var quantity = foodQuantity.Value;

            // Multiply each ingredient's required amount by the quantity of the food item
            var totalIngredientsNeeded = food.Ingredients.Select(
                ingredient => new Ingredient
                {
                    Name = ingredient.Name,
                    Quantity = ingredient.Quantity * quantity
                }).ToList();

            if (!_repository.HasIngredients(totalIngredientsNeeded))
            {
                var lacking = _repository.GetLackingIngredients(totalIngredientsNeeded);
                var lackingMessages = lacking.Select(ingredient => $"\r\n - {ingredient.Quantity} more units of {ingredient.Name} are needed for {quantity}x {food.Name}");
                message.AppendLine($"Cannot process order. {string.Join(". ", lackingMessages)}.");
            }
            else
            {
                message.AppendLine($"Order successful for {quantity}x {food.Name}");
                response.TotalCalories += food.Calories * quantity;
                response.TotalCookTime += food.CookTime * quantity;
                // If the required ingredients are available, remove them from the inventory
                _repository.RemoveIngredients(totalIngredientsNeeded);
            }


        }

        // Continue processing the order...
        message.AppendLine($"Total Calories: {response.TotalCalories}");
        message.AppendLine($"Total Cook Time: {response.TotalCookTime}");
        response.Message = message.ToString();
        return response;
    }

    public IOrderResponse GetOrder(IFood food, int quantity)
    {
        return GetOrder(new OrderRequest
        {
            Quantities = new Dictionary<IFood, int>
            {
                { food, quantity }
            }
        });
    }

    public string DisplayAvailableIngredients()
    {
        var inventory = _repository.GetInventory();
        // Lambda expression to select details of available ingredients
        var ingredientDetails = inventory.Select(kvp => $"{kvp.Key}: {kvp.Value} units");
        return string.Join("\r\n", ingredientDetails);
    }

}
