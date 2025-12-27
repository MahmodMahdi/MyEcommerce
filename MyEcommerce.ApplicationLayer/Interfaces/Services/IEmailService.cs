using MyEcommerce.ApplicationLayer.ViewModels;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IEmailService
	{
		public Task<string> SendEmailAsync(string email, string message, string title);
		Task SendOrderConfirmationEmailAsync(OrderEmailDto orderEmailDto);
		Task SendShippingEmailAsync(OrderEmailDto orderEmailDto);

	}
}
