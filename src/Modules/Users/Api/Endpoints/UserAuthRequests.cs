namespace Web.Api.Endpoints.Users;

public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Code, string NewPassword);
public sealed record VerifyEmailRequest(string Email, string Code);
public sealed record ResendVerificationCodeRequest(string Email);



