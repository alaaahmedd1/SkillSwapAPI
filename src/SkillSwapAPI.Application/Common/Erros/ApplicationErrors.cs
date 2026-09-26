using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Common.Errors;

public static class ApplicationErrors
{
    public static class Token
    {
        public static Error ExpiredAccessTokenInvalid =>
            Error.Unauthorized("Token.ExpiredAccessTokenInvalid", "Expired access token is not valid.");

        public static Error UserIdClaimInvalid =>
            Error.Unauthorized("Token.UserIdClaimInvalid", "Token does not contain a valid user identifier.");

        public static Error RefreshTokenExpired =>
            Error.Unauthorized("Token.RefreshTokenExpired", "Refresh token has expired or does not exist.");

        public static Error GenerationFailed =>
            Error.Unexpected("Token.GenerationFailed", "An error occurred while generating the token.");
    }

    public static class Auth
    {
        public static Error InvalidCredentials =>
            Error.Unauthorized("Auth.InvalidCredentials", "Email or password is incorrect.");

        public static Error AccountLockedOut =>
            Error.Forbidden("Auth.AccountLockedOut", "Account is locked. Please try again later.");

        public static Error EmailAlreadyExists =>
            Error.Conflict("Auth.EmailAlreadyExists", "An account with this email already exists.");

        public static Error EmailNotVerified =>
            Error.Forbidden("Auth.EmailNotVerified", "Email address has not been verified. Please verify your email before logging in.");

        public static Error UserNotFound =>
            Error.NotFound("Auth.UserNotFound", "User not found.");

        public static Error NotFound(string identityId) =>
            Error.NotFound("User.NotFound", $"User with identity id '{identityId}' was not found.");
    }

    public static class Otp
    {
        public static Error Invalid =>
           Error.Validation("Otp.Invalid", "The OTP code is invalid or has expired.");

        public static Error SendFailed =>
            Error.Unexpected("Otp.SendFailed", "Failed to send OTP email. Please try again.");

        public static Error AlreadyVerified =>
            Error.Conflict("Otp.AlreadyVerified", "This email is already verified.");

        public static Error TooManyRequests =>
            Error.Failure("Otp.TooManyRequests", "Too many OTP requests. Please wait before trying again.");
    }

    public static class SocialAuth
    {
        public static Error InvalidToken =>
            Error.Unauthorized(
                "SocialAuth.InvalidToken",
                "The provided social token is invalid or has expired.");

        public static Error ProviderNotSupported =>
            Error.Validation(
                "SocialAuth.ProviderNotSupported",
                "The specified social provider is not supported.");

        public static Error EmailNotProvided =>
            Error.Validation(
                "SocialAuth.EmailNotProvided",
                "Could not retrieve email from the social provider. Please ensure email access is granted.");

        public static Error TokenVerificationFailed =>
            Error.Unexpected(
                "SocialAuth.TokenVerificationFailed",
                "An error occurred while verifying the social token.");
    }
}
