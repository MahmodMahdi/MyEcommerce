using MyEcommerce.ApplicationLayer.ViewModels;
using Stripe.Checkout;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IPaymentService
	{
		// > means it will return tuple
		Task<(string Url, string SessionId)> CreateStripeSessionAsync(ShoppingCartViewModel model, string domain);
		Task<Session> GetStripeSession(string sessionId);

		Task<string> GetStripeSessionUrlAsync(string sessionId);

	}
}
