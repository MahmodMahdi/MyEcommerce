using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using MimeKit;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using Utilities;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class EmailService : IEmailService, IEmailSender
	{
		private readonly EmailSettings _emailSettings;
		private readonly ILogger<EmailService> _logger;
		public EmailService(EmailSettings emailSettings,ILogger<EmailService> logger)
		{
			_emailSettings = emailSettings;
			_logger = logger;
		}
		public async Task<string> SendEmailAsync(string email, string Message, string title)
		{
			try
			{
				//sending the Message of passwordResetLink
				using (var client = new SmtpClient())
				{
					await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, true);
					client.Authenticate(_emailSettings.FromEmail, _emailSettings.Password);
					var bodybuilder = new BodyBuilder
					{
						HtmlBody = $"{Message}",
						TextBody = "Welcome",
					};
					var message = new MimeMessage
					{
						Body = bodybuilder.ToMessageBody()
					};
					message.From.Add(new MailboxAddress("ShopSphere", _emailSettings.FromEmail));
					message.To.Add(new MailboxAddress("Confirm", email));
					message.Subject = title == null ? "Not Submitted" : title;
					await client.SendAsync(message);
					await client.DisconnectAsync(true);
				}
				return "Success";
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Email Service Error: {ex.Message}");
				_logger.LogError(ex,"Failed: Sending email to {email} with subject '{title}'",email,title);

				return "Failed";
			}
		}
		public async Task SendOrderConfirmationEmail(OrderEmailDto orderEmailDto)
		{
			string emailBody = $@"
        <div style='text-align:center; font-family:sans-serif;'>
            <h1 style='color:#007bff;'>Order Confirmed!</h1>
            <p>Thank you <b>{orderEmailDto.Name}</b>, your order has been placed successfully.</p>
            <div style='background:#f8f9fa; padding:15px; border-radius:10px; display:inline-block;'>
                <p>Order Number: <b>#{orderEmailDto.OrderId}</b></p>
                <p>Total Amount: <b>{orderEmailDto.Total:C}</b></p>
            </div>
        </div>";

			await SendEmailAsync(orderEmailDto.Email, emailBody, "Order Confirmation #" + orderEmailDto.OrderId);
		}
		public async Task SendShippingEmail(OrderEmailDto orderEmailDto)
		{
			string carrierPart = !string.IsNullOrEmpty(orderEmailDto.Carrier) ? $"<p>Carrier: <b>{orderEmailDto.Carrier}</b></p>" : "";
			string trackingPart = !string.IsNullOrEmpty(orderEmailDto.TrackingNumber)
			? $"<p>Tracking Number: <b>{orderEmailDto.TrackingNumber}</b></p>" : "";
			string emailBody = $@"
        <div style='font-family:sans-serif; border:1px solid #ddd; padding:20px; border-radius:10px;'>
            <h2 style='color:#28a745;'>Your Order is on the way! 🚚</h2>
            <p>Hi <b>{orderEmailDto.Name}</b>, your order <b>#{orderEmailDto.OrderId}</b> has been shipped.</p>
               <div style='background:#f8f9fa; padding:15px; border-radius:10px;'>
                {trackingPart}
                {carrierPart} 
            </div>
            <p>You can now track your package through our website.</p>
        </div>";

			await SendEmailAsync(orderEmailDto.Email, emailBody, "Shipping Update - Order #" + orderEmailDto.OrderId);
		}

		async Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
		{
			string title = "ShopSphere Security";
			string actionText = "Click the button below";
			string buttonText = "Confirm Action";
			// تخصيص النص بناءً على العنوان (Subject) القادم من Identity
			if (subject.Contains("Confirm your email", StringComparison.OrdinalIgnoreCase))
			{
				// هذه الحالة عند التسجيل لأول مرة (Register)
				title = "Welcome to ShopSphere!";
				actionText = "We're excited to have you! Please confirm your account to start shopping:";
				buttonText = "Confirm Account";
			}
			if (subject.Contains("Reset Password", StringComparison.OrdinalIgnoreCase))
			{
				title = "Reset Your Password";
				actionText = "You requested to reset your password. Click below to proceed:";
				buttonText = "Reset Password";
			}
			else if (subject.Contains("Confirm", StringComparison.OrdinalIgnoreCase))
			{
				title = "Confirm Your New Email";
				actionText = "You requested to change your email. Please confirm it by clicking the button:";
				buttonText = "Confirm Email";
			}
			else if (subject.Contains("Account Locked", StringComparison.OrdinalIgnoreCase))
			{
				title = "Security Alert: Account Locked";
				actionText = "Your account has been temporarily locked for 24 hours due to multiple failed login attempts. If this wasn't you, please reset your password immediately:";
				buttonText = "Reset My Password";
			}
			else if (subject.Contains("Administrative Action", StringComparison.OrdinalIgnoreCase))
			{
				title = "Account Status Notice";
				// هنا نجعل الـ actionText هو السبب الذي سيرسله الأدمن
				actionText = htmlMessage;
				buttonText = "Contact Support";

				// رابط الدعم (يمكنك تغييره لصفحة اتصل بنا في موقعك)
				htmlMessage = "mailto:support@shopsphere.com";
			}

			string emailBody = $@"
    <div style='font-family: sans-serif; text-align: center; border: 1px solid #ddd; padding: 20px; border-radius: 10px; max-width: 500px; margin: auto;'>
        <h2 style='color: #007bff;'>{title}</h2>
        <p>{actionText}</p>
        
        <a href='{htmlMessage}' style='background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0;'>{buttonText}</a>
        
        <p style='color: #777; font-size: 12px;'>If you didn't request this, please ignore this email or contact support.</p>
    </div>";

			await SendEmailAsync(email, emailBody, subject);
		}
	}
}
