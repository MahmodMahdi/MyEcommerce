using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
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
		private readonly IConfiguration _config;
		public EmailService(EmailSettings emailSettings,
			ILogger<EmailService> logger,
			IConfiguration config)
		{
			_emailSettings = emailSettings;
			_logger = logger;
			_config = config;
		}
		public async Task<string> SendEmailAsync(string email, string Message, string title)
		{
			try
			{
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
				//sending the Message of passwordResetLink
				//using (var client = new SmtpClient())
				//{
				//	client.ServerCertificateValidationCallback = (s, c, h, e) => true;
				//	await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
				//	await client.AuthenticateAsync("apikey", _emailSettings.Password);
				//	var bodybuilder = new BodyBuilder
				//	{
				//		HtmlBody = $"{Message}",
				//		TextBody = "Welcome",
				//	};
				//	var message = new MimeMessage
				//	{
				//		Body = bodybuilder.ToMessageBody()
				//	};
				//	message.From.Add(new MailboxAddress("ShopSphere Store", _emailSettings.FromEmail));
				//	message.To.Add(new MailboxAddress("Confirm", email));
				//	message.Subject = title == null ? "Not Submitted" : title;
				//	await client.SendAsync(message);
				//	await client.DisconnectAsync(true);
				//}
				//return "Success";
			}
			catch (Exception ex)
			{

				Console.WriteLine($"Email Service Error: {ex.Message}");
				_logger.LogError(ex, "Failed: Sending email to {email} with subject '{title}'", email, title);

				return "Failed";
			}
		}
		public async Task SendOrderConfirmationEmailAsync(OrderEmailDto orderEmailDto)
		{
			try
			{
				string emailBody = $@"
        <div style='text-align:center; font-family:sans-serif;'>
            <h1 style='color:#007bff;'>تم تأكيد طلبك! 🎉</h1>
            <p>شكراً لك يا <b>{orderEmailDto.Name}</b>,لقد استلمنا طلبك بنجاح وهو الآن قيد التجهيز.</p>
            <div style='background:#f8f9fa; padding:15px; border-radius:10px; display:inline-block;'>
                <p>رقم الطلب: <b>#{orderEmailDto.OrderId}</b></p>
                <p>إجمالى المبلغ: <b>{orderEmailDto.Total.ToString("N2"):C}</b></p>
            </div>
                <p style='color: #666; font-size: 14px;'>سنقوم بإرسال بريد إلكتروني آخر لك بمجرد شحن المنتجات وتزويدك برقم التتبع.</p>
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                <p style='font-size: 12px; color: #aaa;'>شكراً لتسوقك مع ShopSphere</p>
            </div>";

				await SendEmailAsync(orderEmailDto.Email, emailBody, "Order Confirmation #" + orderEmailDto.OrderId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SMTP Error: {Message}", ex.Message);

				throw;
			}
		}
		public async Task SendShippingEmailAsync(OrderEmailDto orderEmailDto)
		{
			try
			{
				string carrierPart = !string.IsNullOrEmpty(orderEmailDto.Carrier) ? $"<p>شركة الشحن: <b>{orderEmailDto.Carrier}</b></p>" : "";
				string trackingPart = !string.IsNullOrEmpty(orderEmailDto.TrackingNumber)? $"<p>رقم التتبع: <b>{orderEmailDto.TrackingNumber}</b></p>" : "";
				string emailBody = $@"
        <div style='font-family:sans-serif; border:1px solid #ddd; padding:20px; border-radius:10px;'>
            <h2 style='color:#28a745;'>طلبك فى الطريق اليك 🚚</h2>
        </b> بنجاح.</p> #{orderEmailDto.OrderId}  </b>,تم شحن طلبك رقم <b> {orderEmailDto.Name} <p> مرحبا <b>
               <div style='background:#f8f9fa; padding:15px; border-radius:10px;'>
                {trackingPart}
                {carrierPart} 
            </div>
            <p>You can now track your package through our website.</p>
        </div>";

				await SendEmailAsync(orderEmailDto.Email, emailBody, "Shipping Update - Order #" + orderEmailDto.OrderId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SMTP Error: {Message}", ex.Message);

				throw;
			}
		}

		async Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
		{
			try
			{
				string baseUrl = _config["BaseUrl"];
				string title = "ShopSphere Security";
				string actionText = "Click the button below to proceed:";
				string buttonText = "Confirm Action";

				if (subject.Contains("Welcome via Google", StringComparison.OrdinalIgnoreCase))
				{
					title = "Welcome to ShopSphere Family!";
					actionText = "تم إنشاء حسابك بنجاح عبر جوجل. يمكنك الآن البدء بالتسوق أو إكمال بيانات ملفك الشخصي لتسهيل عمليات الشحن مستقبلاً";
					htmlMessage = $"{baseUrl}/Identity/Account/Manage";
					buttonText = "إكمال بيانات حسابي";
				}
				else if (subject.Contains("Confirm your account", StringComparison.OrdinalIgnoreCase))
				{
					title = "تأكيد بريدك الإلكتروني";
					actionText = "سعداء بانضمامك إلينا! يرجى الضغط على الزر أدناه لتفعيل حسابك:";
					buttonText = "تأكيد الحساب";
				}
				else if (subject.Contains("Reset Password", StringComparison.OrdinalIgnoreCase))
				{
					title = "استعادة كلمة المرور";
					actionText = "لقد تلقينا طلباً لإعادة تعيين كلمة المرور الخاصة بك. إذا كنت أنت من قام بذلك، اضغط أدناه:";
					buttonText = "تغيير كلمة المرور";
				}
				else if (subject.Contains("Confirm", StringComparison.OrdinalIgnoreCase))
				{
					title = "تأكيد حسابك الجديد";
					actionText = "هل تريد تغيير ايميلك؟ لو سمحت اضغط على الرابط للتأكيد.";
					buttonText = "تأكيد الحساب";
				}
				else if (subject.Contains("Account Locked", StringComparison.OrdinalIgnoreCase))
				{
					title = "تنبيه أمني: تم قفل الحساب";
					actionText = "تم قفل حسابك مؤقتاً لمدة 24 ساعة بسبب محاولات تسجيل دخول خاطئة متكررة. إذا لم تكن أنت من قام بذلك، يرجى إعادة تعيين كلمة المرور فوراً لتأمين حسابك:";
					buttonText = "إعادة تعيين كلمة المرور";
					htmlMessage = $"{baseUrl}/Identity/Account/ForgotPassword";
				}
				else if (subject.Contains("Administrative Action", StringComparison.OrdinalIgnoreCase))
				{
					title = "إشعار بخصوص حالة الحساب";
					actionText = $"نود إعلامك بالتالي بخصوص حسابك: <br/> <b>{htmlMessage}</b>"; ;
					buttonText = "التواصل مع الدعم الفني";

					// رابط الدعم (يمكنك تغييره لصفحة اتصل بنا في موقعك)
					htmlMessage = "mailto:support@shopsphere.com";
				}

				string emailBody = $@"
            <div dir='rtl' style='font-family: sans-serif; text-align: center; border: 1px solid #ddd; padding: 20px; border-radius: 10px; max-width: 500px; margin: auto;'>
                <h2 style='color: #007bff;'>{title}</h2>
                <p style='color: #555;'>{actionText}</p>        
                <a href='{htmlMessage}' style='background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0;'>{buttonText}</a>
        
                <p style='color: #777; font-size: 12px;'>If you didn't request this, please ignore this email or contact support.</p>
            </div>";

				await SendEmailAsync(email, emailBody, subject);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SMTP Error: {Message}", ex.Message);
				throw;
			}
		}
	}
}
