using MyEcommerce.ApplicationLayer.ViewModels;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IPaymentService
	{
		// > means it will return tuple
		Task<(string Url, string SessionId)> CreateStripeSessionAsync(ShoppingCartViewModel model, string domain);
	}
}
