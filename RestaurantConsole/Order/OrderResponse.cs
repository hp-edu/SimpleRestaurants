public class OrderResponse : IOrderResponse
{
    public string Message { get; set; } = "";
    public int TotalCookTime { get; set; } = 0;
    public int TotalCalories { get; set; } = 0;
}