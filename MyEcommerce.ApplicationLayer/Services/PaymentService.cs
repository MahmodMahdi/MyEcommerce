using Microsoft.Extensions.Logging;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using Stripe;
using Stripe.Checkout;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly ILogger<PaymentService> _logger;
		public PaymentService(ILogger<PaymentService> logger)
		{
			_logger = logger;
		}
		public async Task<(string Url, string SessionId)> CreateStripeSessionAsync(ShoppingCartViewModel model, string domain)
		{
			try {
				var options = new SessionCreateOptions
				{
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
					SuccessUrl = $"{domain}Customer/Cart/OrderConfirmation?id={model.OrderHeader.Id}",
					CancelUrl = $"{domain}Customer/Cart/Index",
				};

				foreach (var item in model.Carts)
				{
					var sessionLineOption = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Product.Price * 100), // تحويل للقروش
							Currency = "egp",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Name,
							},
						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineOption);
				}

				var service = new SessionService();
				Session session = await service.CreateAsync(options);
				return (session.Url, session.Id);
			}
			catch (StripeException ex)
			{
				// أخطاء خاصة بـ Stripe (مثل API Key خطأ أو مشكلة في الحساب)
				_logger.LogError(ex, "[STRIPE ERROR] Failed to create checkout session for Order #{OrderId}. Error Code: {StripeCode}", model.OrderHeader.Id, ex.Message);
				throw;
			}
			catch (HttpRequestException ex)
			{
				// أخطاء عامة (مثل مشكلة في الإنترنت)
				_logger.LogCritical("Network Error: Could not reach Stripe servers. Check internet connection.");
				throw new Exception("Service is temporarily unavailable due to network issues.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[UNEXPECTED ERROR] PaymentService failed for Order #{OrderId}", model.OrderHeader.Id);
				throw new Exception("An unexpected error occurred while processing your payment. Please try again later.");
			}
		}
		
	}
}
